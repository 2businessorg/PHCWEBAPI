using Audit.Application.Services;
using Microsoft.Extensions.DependencyInjection;
using Shared.Kernel.Responses;
using System.Text;
using System.Text.Json;

namespace PHCAPI.Host.Middleware;


public sealed class ResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseLoggingMiddleware> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public ResponseLoggingMiddleware(
        RequestDelegate next,
        ILogger<ResponseLoggingMiddleware> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _next = next;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        var method = context.Request.Method;
        
        var requestBody = await CaptureRequestBodyAsync(context.Request);
        var originalBodyStream = context.Response.Body;

        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await _next(context);
            await ProcessSuccessfulResponseAsync(context, requestBody, responseBody, originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[MIDDLEWARE EXCEPTION] {Method} {Path} - Exception: {Message}", 
                method, path, ex.Message);
            await ProcessExceptionResponseAsync(context, requestBody, ex);
            context.Response.Body = originalBodyStream;
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    private async Task ProcessSuccessfulResponseAsync(
        HttpContext context,
        string? requestBody,
        MemoryStream responseBody,
        Stream originalBodyStream)
    {
        var responseText = await ReadResponseTextAsync(responseBody);
        await responseBody.CopyToAsync(originalBodyStream);

        // ✅ Auditar JSON APIs OU operações importantes do Admin.UI
        if (!ShouldAudit(context, responseText)) return;

        var (responseDto, auditJson) = GetResponseForAudit(responseText, context);
        await LogAuditAsync(context, requestBody, responseDto, auditJson);
    }

    private (ResponseDTO response, string json) GetResponseForAudit(string responseText, HttpContext context)
    {
        // ✅ Admin.UI Razor Pages (HTML) - Criar ResponseDTO específico para auditoria
        if (!IsJsonResponse(context) && IsAdminUIAuditableRequest(context))
        {
            var adminAuditResponse = CreateAdminUIAuditResponse(context, responseText);
            var adminAuditJson = SerializeResponse(adminAuditResponse);
            return (adminAuditResponse, adminAuditJson);
        }

        // ✅ JSON API - Tentativa 1: Deserializar como ResponseDTO
        var responseDto = TryDeserializeResponse(responseText);
        if (responseDto != null)
            return (responseDto, responseText);

        // ✅ JSON API - Tentativa 2: Deserializar como objeto genérico
        var genericObject = TryDeserializeAsObject(responseText);
        if (genericObject != null)
        {
            var wrappedResponse = CreateGenericResponseWrapper(genericObject, context);
            return (wrappedResponse, responseText); // ✅ Preserva JSON original
        }

        // ✅ Fallback: Usar resposta raw como string
        var rawResponse = CreateRawResponseWrapper(responseText, context);
        return (rawResponse, responseText); // ✅ Preserva JSON original
    }

    private object? TryDeserializeAsObject(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<object>(json);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ResponseDTO CreateGenericResponseWrapper(object data, HttpContext context)
    {
        var correlationId = ExtractCorrelationId(context.TraceIdentifier);
        
        return new ResponseDTO(
            response: new ResponseCodeDTO(
                code: "0000",
                description: "Success - Non-standard response structure",
                id: correlationId
            ),
            data: data,
            content: new
            {
                path = context.Request.Path.ToString(),
                originalStructure = "generic-object"
            }
        );
    }

    private static ResponseDTO CreateRawResponseWrapper(string rawJson, HttpContext context)
    {
        var correlationId = ExtractCorrelationId(context.TraceIdentifier);
        
        return new ResponseDTO(
            response: new ResponseCodeDTO(
                code: "0000",
                description: "Success - Raw JSON captured",
                id: correlationId
            ),
            data: new { rawResponse = rawJson },
            content: new
            {
                path = context.Request.Path.ToString(),
                originalStructure = "raw-json"
            }
        );
    }

    /// <summary>
    /// Cria ResponseDTO para auditoria de operações Admin.UI (Razor Pages)
    /// Captura informações críticas para fiscalização e compliance
    /// </summary>
    private static ResponseDTO CreateAdminUIAuditResponse(HttpContext context, string htmlResponse)
    {
        var correlationId = ExtractCorrelationId(context.TraceIdentifier);
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;
        var statusCode = context.Response.StatusCode;

        // ✅ Extrair informações do usuário autenticado
        var username = context.User?.Identity?.Name ?? "Anonymous";
        var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var userRoles = context.User?.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList() ?? new List<string>();

        // ✅ Determinar a ação realizada baseado no path e método
        var action = DetermineAdminAction(path, method);

        // ✅ Capturar form data se disponível (para POST/PUT)
        var formData = context.Request.HasFormContentType 
            ? context.Request.Form.ToDictionary(kv => kv.Key, kv => kv.Value.ToString())
            : null;

        return new ResponseDTO(
            response: new ResponseCodeDTO(
                code: statusCode >= 200 && statusCode < 300 ? "0000" : "0007",
                description: $"Admin UI - {action}",
                id: correlationId
            ),
            data: new
            {
                action = action,
                user = new
                {
                    username = username,
                    userId = userId,
                    roles = userRoles,
                    isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false
                },
                request = new
                {
                    path = path,
                    method = method,
                    queryString = context.Request.QueryString.ToString(),
                    formData = formData != null ? SanitizeFormData(formData) : null,
                    contentType = context.Request.ContentType
                },
                response = new
                {
                    statusCode = statusCode,
                    statusDescription = GetStatusDescription(statusCode),
                    contentType = context.Response.ContentType,
                    htmlLength = htmlResponse?.Length ?? 0
                },
                network = new
                {
                    ipAddress = context.Connection.RemoteIpAddress?.ToString(),
                    userAgent = context.Request.Headers.UserAgent.ToString(),
                    referer = context.Request.Headers.Referer.ToString()
                },
                audit = new
                {
                    timestamp = DateTime.UtcNow,
                    traceIdentifier = context.TraceIdentifier,
                    correlationId = correlationId
                }
            },
            content: new
            {
                module = "Admin.UI",
                type = "razor-page",
                category = GetAuditCategory(path, method)
            }
        );
    }

    /// <summary>
    /// Determina a ação específica baseado no path e método
    /// </summary>
    private static string DetermineAdminAction(string path, string method)
    {
        var pathLower = path.ToLowerInvariant();

        // Login/Logout
        if (pathLower.Contains("/account/login"))
            return method == "POST" ? "User Login Attempt" : "Login Page Access";
        if (pathLower.Contains("/account/logout"))
            return "User Logout";
        if (pathLower.Contains("/account/accessdenied"))
            return "Access Denied";

        // Users Management
        if (pathLower.Contains("/users"))
        {
            if (pathLower.Contains("/create")) return "Create User";
            if (pathLower.Contains("/edit")) return "Edit User";
            if (pathLower.Contains("/delete")) return "Delete User";
            if (pathLower.Contains("/details")) return "View User Details";
            return method == "GET" ? "List Users" : "User Management Action";
        }

        // Roles Management
        if (pathLower.Contains("/roles"))
        {
            if (pathLower.Contains("/create")) return "Create Role";
            if (pathLower.Contains("/edit")) return "Edit Role";
            if (pathLower.Contains("/delete")) return "Delete Role";
            if (pathLower.Contains("/details")) return "View Role Details";
            if (pathLower.Contains("/assignpermissions")) return "Assign Role Permissions";
            return method == "GET" ? "List Roles" : "Role Management Action";
        }

        return $"Admin UI {method}";
    }

    /// <summary>
    /// Sanitiza dados de formulário removendo campos sensíveis
    /// </summary>
    private static Dictionary<string, string> SanitizeFormData(Dictionary<string, string> formData)
    {
        var sanitized = new Dictionary<string, string>(formData);
        var sensitiveFields = new[] { "password", "confirmpassword", "currentpassword", "newpassword" };

        foreach (var field in sensitiveFields)
        {
            if (sanitized.ContainsKey(field))
                sanitized[field] = "***REDACTED***";
            
            // Check case-insensitive variants
            var matchingKey = sanitized.Keys.FirstOrDefault(k => 
                k.Equals(field, StringComparison.OrdinalIgnoreCase));
            if (matchingKey != null)
                sanitized[matchingKey] = "***REDACTED***";
        }

        return sanitized;
    }

    /// <summary>
    /// Retorna descrição amigável do status HTTP
    /// </summary>
    private static string GetStatusDescription(int statusCode) => statusCode switch
    {
        200 => "OK - Success",
        201 => "Created - Resource created successfully",
        204 => "No Content - Success without content",
        302 => "Redirect - Temporary redirect",
        400 => "Bad Request - Invalid data",
        401 => "Unauthorized - Authentication required",
        403 => "Forbidden - Access denied",
        404 => "Not Found - Resource not found",
        500 => "Internal Server Error",
        _ => $"Status {statusCode}"
    };

    /// <summary>
    /// Categoriza a operação para relatórios de auditoria
    /// </summary>
    private static string GetAuditCategory(string path, string method)
    {
        var pathLower = path.ToLowerInvariant();

        if (pathLower.Contains("/account/")) return "Authentication";
        if (pathLower.Contains("/users/")) return "User Management";
        if (pathLower.Contains("/roles/")) return "Role Management";
        
        return method == "GET" ? "Data Access" : "Data Modification";
    }

    private async Task ProcessExceptionResponseAsync(
        HttpContext context,
        string? requestBody,
        Exception ex)
    {
        var internalAuditResponse = CreateExceptionAuditResponse(context, ex);
        var auditJson = SerializeResponse(internalAuditResponse);

        await LogAuditAsync(context, requestBody, internalAuditResponse, auditJson);
    }

    private async Task LogAuditAsync(
        HttpContext context,
        string? requestBody,
        ResponseDTO responseDto,
        string responseJson)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var auditLogService = scope.ServiceProvider.GetService<IAuditLogService>();

            if (auditLogService == null)
            {
                _logger.LogWarning("IAuditLogService not found in DI container for {Method} {Path}", 
                    context.Request.Method, context.Request.Path);
                return;
            }

            // ✅ Capturar informações do usuário autenticado
            var userId = context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var username = context.User?.Identity?.Name;

            auditLogService.LogResponseAsync(
                responseDto,
                context.TraceIdentifier,
                $"{context.Request.Method} {context.Request.Path}",
                context.Connection.RemoteIpAddress?.ToString(),
                context.Request.Headers.UserAgent.ToString(),
                context.Response.StatusCode,
                requestBody,
                responseJson,
                userId,
                username
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log audit for {Method} {Path}", 
                context.Request.Method, context.Request.Path);
        }
    }

    private static async Task<string> ReadResponseTextAsync(MemoryStream responseBody)
    {
        responseBody.Seek(0, SeekOrigin.Begin);
        var text = await new StreamReader(responseBody).ReadToEndAsync();
        responseBody.Seek(0, SeekOrigin.Begin);
        return text;
    }

    /// <summary>
    /// Determina se a requisição deve ser auditada (gravada em AuditLogs)
    /// </summary>
    private bool ShouldAudit(HttpContext context, string? responseText)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        
        if (string.IsNullOrWhiteSpace(responseText))
            return false;

        if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase))
            return true;

        if (IsJsonResponse(context))
            return true;

        return IsAdminUIAuditableRequest(context);
    }

    /// <summary>
    /// Verifica se é uma operação Admin.UI que deve ser auditada
    /// </summary>
    private static bool IsAdminUIAuditableRequest(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var method = context.Request.Method;

        // ✅ Auditar apenas rotas /Admin (Admin.UI)
        if (!path.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
            return false;

        // ✅ Auditar TODAS as operações de Account (Login/Logout/AccessDenied) - Crítico para compliance
        if (path.Contains("/Account", StringComparison.OrdinalIgnoreCase))
            return true;

        // ✅ Auditar operações de escrita (POST/PUT/DELETE)
        if (HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsDelete(method))
            return true;

        // ✅ Auditar GET apenas em endpoints críticos (visualização de dados sensíveis)
        if (HttpMethods.IsGet(method))
        {
            // Auditar listagens de usuários, roles, e páginas de detalhes
            return path.Contains("/Users", StringComparison.OrdinalIgnoreCase) ||
                   path.Contains("/Roles", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    private static bool IsJsonResponse(HttpContext context) =>
        context.Response.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) ?? false;

    private ResponseDTO? TryDeserializeResponse(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<ResponseDTO>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ResponseDTO CreateExceptionAuditResponse(HttpContext context, Exception ex)
    {
        var requestId = context.TraceIdentifier;
        var correlationId = ExtractCorrelationId(requestId);

        return new ResponseDTO(
            response: new ResponseCodeDTO(
                code: "0007",
                description: "Internal error - Exception caught in middleware",
                id: correlationId
            ),
            data: new
            {
                exceptionType = ex.GetType().FullName,
                message = ex.Message,
                source = ex.Source,
                stackTrace = ex.StackTrace,
                innerException = ex.InnerException != null ? new
                {
                    type = ex.InnerException.GetType().FullName,
                    message = ex.InnerException.Message,
                    stackTrace = ex.InnerException.StackTrace
                } : null,
                targetSite = ex.TargetSite?.ToString()
            },
            content: new
            {
                path = context.Request.Path.ToString(),
                method = context.Request.Method,
                queryString = context.Request.QueryString.ToString(),
                timestamp = DateTime.UtcNow
            }
        );
    }

    private static string SerializeResponse(ResponseDTO response) =>
        JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        });

    private static decimal ExtractCorrelationId(string requestId)
    {
        var numericOnly = new string(requestId.Where(char.IsDigit).ToArray());
        return decimal.TryParse(numericOnly.Length > 15 ? numericOnly[..15] : numericOnly, out var id) ? id : 0;
    }

    private async Task<string?> CaptureRequestBodyAsync(HttpRequest request)
    {
        if (!ShouldCaptureRequestBody(request)) return null;

        try
        {
            request.EnableBuffering();
            request.Body.Position = 0;

            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;

            return string.IsNullOrWhiteSpace(body) ? null : body;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to capture request body for {Method} {Path}", 
                request.Method, request.Path);
            return null;
        }
    }

    private static bool ShouldCaptureRequestBody(HttpRequest request) =>
        (HttpMethods.IsPost(request.Method) || 
         HttpMethods.IsPut(request.Method) || 
         HttpMethods.IsPatch(request.Method)) &&
        request.ContentLength is > 0;
}
