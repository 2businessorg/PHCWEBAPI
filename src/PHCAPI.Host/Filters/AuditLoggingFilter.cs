using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Audit.Application.Services;
using Shared.Kernel.Responses;
using System.Text;
using System.Text.Json;

namespace PHCAPI.Host.Filters;

/// <summary>
/// ✅ Action Filter para log de auditoria nas respostas da API
/// Registrado globalmente em Program.cs
/// </summary>
public sealed class AuditLoggingFilter : IAsyncActionFilter
{
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<AuditLoggingFilter> _logger;

    public AuditLoggingFilter(
        IAuditLogService auditLogService,
        ILogger<AuditLoggingFilter> logger)
    {
        _auditLogService = auditLogService ?? throw new ArgumentNullException(nameof(auditLogService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        string? requestBody = null;
        var httpContext = context.HttpContext;

        try
        {
            // ✅ Capturar body do request (se existir)
            requestBody = await CaptureRequestBodyAsync(httpContext.Request);

            // ✅ Executar a action
            var executedContext = await next();

            // ✅ Tentar extrair ResponseDTO de vários tipos de resultado
            ResponseDTO? response = null;
            object? responseValue = null;
            int statusCode = httpContext.Response.StatusCode;

            // Caso 1: ObjectResult (retorno normal do controller)
            if (executedContext.Result is ObjectResult objectResult)
            {
                responseValue = objectResult.Value;
                statusCode = objectResult.StatusCode ?? statusCode;
                
                if (responseValue is ResponseDTO dto)
                {
                    response = dto;
                }
            }
            // Caso 2: JsonResult (pode acontecer em alguns casos)
            else if (executedContext.Result is JsonResult jsonResult)
            {
                responseValue = jsonResult.Value;
                statusCode = jsonResult.StatusCode ?? statusCode;
                
                if (responseValue is ResponseDTO dto)
                {
                    response = dto;
                }
            }
            // Caso 3: ContentResult ou outros
            else if (executedContext.Result != null)
            {
                _logger.LogDebug(
                    "[FILTER] Unsupported result type: {ResultType} for {Path}",
                    executedContext.Result.GetType().Name,
                    httpContext.Request.Path);
                return;
            }

            // Se não encontrou ResponseDTO, não logar
            if (response == null || responseValue == null)
            {
                _logger.LogDebug(
                    "[FILTER] No ResponseDTO found for {Path}. ResultType: {ResultType}",
                    httpContext.Request.Path,
                    executedContext.Result?.GetType().Name ?? "null");
                return;
            }

            // ✅ Preparar dados para audit
            var request = httpContext.Request;
            var requestId = httpContext.TraceIdentifier;
            var operation = $"{request.Method} {request.Path}";
            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();
            var userAgent = request.Headers.UserAgent.ToString();

            // ✅ Capturar informações do usuário autenticado
            var userId = httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var username = httpContext.User?.Identity?.Name;

            // ✅ Serializar o que foi retornado (sucesso OU erro)
            var responseJson = TrySerializeToJson(responseValue);

            _auditLogService.LogResponseAsync(
                response,
                requestId,
                operation,
                ipAddress,
                userAgent,
                statusCode,
                requestBody,
                responseJson,
                userId,
                username
            );

            _logger.LogDebug(
                "[FILTER] Audit log triggered for {RequestId} - Status: {StatusCode}",
                requestId, statusCode);
        }
        catch (Exception ex)
        {
            // ✅ NUNCA deixar o filtro quebrar o pipeline
            _logger.LogError(ex, 
                "[FILTER] ❌ CRITICAL: Filter failed for {Path}. Error: {ErrorMessage}", 
                httpContext.Request.Path, 
                ex.Message);
        }
    }

    private async Task<string?> CaptureRequestBodyAsync(HttpRequest request)
    {
        try
        {
            // Apenas capturar se for POST/PUT/PATCH e tiver content
            if (!HttpMethods.IsPost(request.Method) && 
                !HttpMethods.IsPut(request.Method) && 
                !HttpMethods.IsPatch(request.Method))
            {
                return null;
            }

            if (request.ContentLength == null || request.ContentLength == 0)
            {
                return null;
            }
            request.EnableBuffering(); // Permite ler múltiplas vezes
            request.Body.Position = 0;

            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                bufferSize: 1024,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0; // Reset para o controller ler

            return string.IsNullOrWhiteSpace(body) ? null : body;
        }
        catch (Exception ex)
        {
            return "N/A";
        }
    }

    /// <summary>
    /// Serializa qualquer objeto para JSON (captura exatamente o que o controller retorna)
    /// </summary>
    private string? TrySerializeToJson(object? obj)
    {
        if (obj == null)
            return null;

        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false, // Compacto
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                MaxDepth = 32,
                ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
            };

            // ✅ Serializa diretamente - System.Text.Json materializa IEnumerables automaticamente
            var json = JsonSerializer.Serialize(obj, obj.GetType(), options);
            
           

            return json;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[FILTER] Failed to serialize response");
            return $"[Error serializing: {ex.Message}]";
        }
    }
}

