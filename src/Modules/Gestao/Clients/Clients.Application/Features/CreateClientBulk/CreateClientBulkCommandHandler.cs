using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using FluentValidation;
using Clients.Application.DTOs;
using Clients.Application.Errors;
using Clients.Application.Features.CreateClient;
using Clients.Domain.Repositories;
using Clients.Domain.Entities;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.Responses;
using Shared.Kernel.UserFields;

namespace Clients.Application.Features.CreateClientBulk;

/// <summary>
/// Handler para criar múltiplos clientes em bulk (melhor esforço)
/// </summary>
public class CreateClientBulkCommandHandler : IRequestHandler<CreateClientBulkCommand, CreateClientBulkResponseDTO>
{
    private readonly IClientRepository _repository;
    private readonly IValidator<CreateClientCommand> _singleValidator;
    private readonly IUserFieldService _userFieldService;
    private readonly ITenantContext _tenantContext;

    public CreateClientBulkCommandHandler(
        IClientRepository repository,
        IValidator<CreateClientCommand> singleValidator,
        IUserFieldService userFieldService,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _singleValidator = singleValidator;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    public async Task<CreateClientBulkResponseDTO> Handle(
        CreateClientBulkCommand request,
        CancellationToken cancellationToken)
    {
        var sqlMinDate = new DateTime(1900, 1, 1);
        var response = new CreateClientBulkResponseDTO();
        var results = new List<Shared.Kernel.Responses.BulkItemResultDTO>();

        IReadOnlyList<UserFieldDefinition> clFieldDefs = [];
        IReadOnlyList<UserFieldDefinition> cl2FieldDefs = [];
        if (_tenantContext.AppLicenseStamp is not null)
        {
            clFieldDefs = await _userFieldService.GetFieldsAsync(
                _tenantContext.AppLicenseStamp,
                "Clients",
                "cl",
                cancellationToken);

            cl2FieldDefs = await _userFieldService.GetFieldsAsync(
                _tenantContext.AppLicenseStamp,
                "Clients",
                "cl2",
                cancellationToken);
        }

        if (request.Dto.Items == null || request.Dto.Items.Count == 0)
        {
            response.Code = ClientsErrorCatalog.BulkEmpty.Code;
            response.Message = ClientsErrorCatalog.BulkEmpty.Description;
            response.Items = results;
            response.SuccessCount = 0;
            response.FailureCount = 0;
            return response;
        }

        // Processar cada item: best-effort (insere os que consegue, pula os com erro)
        for (int i = 0; i < request.Dto.Items.Count; i++)
        {
            var itemResult = new Shared.Kernel.Responses.BulkItemResultDTO { Index = i };
            var dto = request.Dto.Items[i];

            try
            {
                // 1. Validar item individual
                var command = new CreateClientCommand(dto, request.CreatedBy);
                var validationResult = await _singleValidator.ValidateAsync(command, cancellationToken);

                if (!validationResult.IsValid)
                {
                    var firstError = validationResult.Errors.FirstOrDefault();
                    itemResult.Success = false;
                    itemResult.Data = null;
                    itemResult.Error = new BulkErrorDTO
                    {
                        Code = firstError?.ErrorCode ?? "0012",
                        Message = firstError?.ErrorMessage ?? "Validação falhou"
                    };
                    results.Add(itemResult);
                    response.FailureCount++;
                    continue;
                }

                // 2. Validar regras de negócio (duplicatas no banco)
                var requestedNo = dto.No.GetValueOrDefault();
                var requestedEstab = dto.Estab.GetValueOrDefault();
                var usesAutomaticNoAndDefaultEstab = requestedNo == 0 && requestedEstab == 0;

                var existingByNcont = await _repository.GetByNcontAsync(dto.Ncont, cancellationToken);

                if (usesAutomaticNoAndDefaultEstab)
                {
                    if (existingByNcont is not null)
                    {
                        itemResult.Success = false;
                        itemResult.Data = null;
                        itemResult.Error = new BulkErrorDTO
                        {
                            Code = ClientsErrorCatalog.ClientNcontAlreadyExists.Code,
                            Message = string.Format(
                                ClientsErrorCatalog.ClientNcontAlreadyExists.Description, dto.Ncont)
                        };
                        results.Add(itemResult);
                        response.FailureCount++;
                        continue;
                    }
                }
                else
                {
                    var existsSameNoAndEstab = await _repository.ExistsByNoAndEstabAsync(
                        requestedNo, requestedEstab, cancellationToken);

                    if (existsSameNoAndEstab)
                    {
                        itemResult.Success = false;
                        itemResult.Data = null;
                        itemResult.Error = new BulkErrorDTO
                        {
                            Code = ClientsErrorCatalog.ClientNoEstabAlreadyExists.Code,
                            Message = string.Format(
                                ClientsErrorCatalog.ClientNoEstabAlreadyExists.Description, requestedNo, requestedEstab)
                        };
                        results.Add(itemResult);
                        response.FailureCount++;
                        continue;
                    }

                    if (existingByNcont is not null && existingByNcont.No != requestedNo)
                    {
                        itemResult.Success = false;
                        itemResult.Data = null;
                        itemResult.Error = new BulkErrorDTO
                        {
                            Code = ClientsErrorCatalog.ClientNcontLinkedToAnotherNo.Code,
                            Message = string.Format(
                                ClientsErrorCatalog.ClientNcontLinkedToAnotherNo.Description, dto.Ncont, existingByNcont.No)
                        };
                        results.Add(itemResult);
                        response.FailureCount++;
                        continue;
                    }
                }

                // 3. Gerar stamp e determinar No/Estab
                var stamp = GenerateStamp();
                var no = usesAutomaticNoAndDefaultEstab
                    ? await _repository.GetNextNoAsync(cancellationToken)
                    : requestedNo;

                var estab = usesAutomaticNoAndDefaultEstab ? 0 : requestedEstab;

                // 4. Criar entidades
                var cl = new Cl
                {
                    Clstamp = stamp,
                    Nome = dto.Nome,
                    Ncont = dto.Ncont,
                    Telefone = dto.Telefone ?? string.Empty,
                    Morada = dto.Morada ?? string.Empty,
                    Email = string.Empty,
                    No = no,
                    Estab = estab,
                    Inactivo = false,
                    Ousrinis = request.CreatedBy ?? "PHCAPI",
                    Ousrdata = DateTime.Now.Date,
                    Ousrhora = DateTime.Now.ToString("HH:mm:ss"),
                    Usrinis = request.CreatedBy ?? "PHCAPI",
                    Usrdata = DateTime.Now.Date,
                    Usrhora = DateTime.Now.ToString("HH:mm:ss"),
                };

                var cl2 = new Cl2
                {
                    Cl2stamp = stamp,
                    Ousrinis = request.CreatedBy ?? "PHCAPI",
                    Ousrdata = DateTime.Now.Date,
                    Ousrhora = DateTime.Now.ToString("HH:mm:ss"),
                    Usrinis = request.CreatedBy ?? "PHCAPI",
                    Usrdata = DateTime.Now.Date,
                    Usrhora = DateTime.Now.ToString("HH:mm:ss"),
                };

                // 5. Persistir individualmente (não transação global, por item)
                try
                {
                    await _repository.AddAsync(cl, cl2, cancellationToken);

                    // 5.1 Atualizar campos adicionais se fornecidos
                    if (dto.AddFields != null && dto.AddFields.Count > 0)
                    {
                        var (clUpdates, cl2Updates) = MapAddFieldsToColumns(
                            dto.AddFields,
                            clFieldDefs,
                            cl2FieldDefs);

                        if (clUpdates.Count > 0)
                            await _repository.UpdateUserFieldsAsync(stamp, clUpdates, cancellationToken);

                        if (cl2Updates.Count > 0)
                            await _repository.UpdateUserFieldsFromCl2Async(stamp, cl2Updates, cancellationToken);
                    }

                    Dictionary<string, object?>? userFields = null;
                    if (clFieldDefs.Count > 0 || cl2FieldDefs.Count > 0)
                    {
                        Dictionary<string, object?> clRawValues = [];
                        Dictionary<string, object?> cl2RawValues = [];

                        if (clFieldDefs.Count > 0)
                        {
                            var clColumns = clFieldDefs.Select(f => f.ColumnName).ToList();
                            clRawValues = await _repository.GetUserFieldValuesAsync(stamp, clColumns, cancellationToken);
                        }

                        if (cl2FieldDefs.Count > 0)
                        {
                            var cl2Columns = cl2FieldDefs.Select(f => f.ColumnName).ToList();
                            cl2RawValues = await _repository.GetUserFieldValuesFromCl2Async(stamp, cl2Columns, cancellationToken);
                        }

                        userFields = new Dictionary<string, object?>();

                        foreach (var field in clFieldDefs)
                        {
                            if (!userFields.ContainsKey(field.Alias))
                                userFields[field.Alias] = UserFieldValueFormatter.FormatForOutput(clRawValues.GetValueOrDefault(field.ColumnName), field.FieldType);
                        }

                        foreach (var field in cl2FieldDefs)
                        {
                            if (!userFields.ContainsKey(field.Alias))
                                userFields[field.Alias] = UserFieldValueFormatter.FormatForOutput(cl2RawValues.GetValueOrDefault(field.ColumnName), field.FieldType);
                        }
                    }

                    itemResult.Success = true;
                    itemResult.Data = new
                    {
                        No = no,
                        Estab = estab,
                        Nome = cl.Nome,
                        UserFields = userFields
                    };
                    itemResult.Error = null;
                    results.Add(itemResult);
                    response.SuccessCount++;
                }
                catch (Exception dbEx)
                {
                    itemResult.Success = false;
                    itemResult.Data = null;
                    itemResult.Error = new BulkErrorDTO
                    {
                        Code = ClientsErrorCatalog.DatabaseUpdateError.Code,
                        Message = $"Erro ao persistir: {dbEx.InnerException?.Message ?? dbEx.Message}"
                    };
                    results.Add(itemResult);
                    response.FailureCount++;
                }
            }
            catch (ClientsModuleException ex)
            {
                itemResult.Success = false;
                itemResult.Data = null;
                itemResult.Error = new BulkErrorDTO
                {
                    Code = ex.Code,
                    Message = ex.Message
                };
                results.Add(itemResult);
                response.FailureCount++;
            }
            catch (Exception ex)
            {
                itemResult.Success = false;
                itemResult.Data = null;
                itemResult.Error = new BulkErrorDTO
                {
                    Code = "CL001",
                    Message = $"Erro inesperado: {ex.Message}"
                };
                results.Add(itemResult);
                response.FailureCount++;
            }
        }

        // Montar resposta final
        response.Items = results;
        response.Code = response.FailureCount > 0
            ? ClientsErrorCatalog.BulkProcessed.Code
            : ClientsErrorCatalog.Success.Code;
        response.Message = string.Format(
            ClientsErrorCatalog.BulkProcessed.Description,
            response.SuccessCount,
            response.FailureCount);

        return response;
    }

    private static (Dictionary<string, object?> ClFields, Dictionary<string, object?> Cl2Fields) MapAddFieldsToColumns(
        IReadOnlyDictionary<string, object?> addFields,
        IReadOnlyList<UserFieldDefinition> clFieldDefs,
        IReadOnlyList<UserFieldDefinition> cl2FieldDefs)
    {
        var clUpdates = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        var cl2Updates = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        var hasDefinitions = clFieldDefs.Count > 0 || cl2FieldDefs.Count > 0;

        foreach (var entry in addFields)
        {
            if (string.IsNullOrWhiteSpace(entry.Key))
                continue;

            var clDef = clFieldDefs.FirstOrDefault(f =>
                string.Equals(f.Alias, entry.Key, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f.ColumnName, entry.Key, StringComparison.OrdinalIgnoreCase));

            if (clDef is not null)
            {
                clUpdates[clDef.ColumnName] = entry.Value;
                continue;
            }

            var cl2Def = cl2FieldDefs.FirstOrDefault(f =>
                string.Equals(f.Alias, entry.Key, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(f.ColumnName, entry.Key, StringComparison.OrdinalIgnoreCase));

            if (cl2Def is not null)
            {
                cl2Updates[cl2Def.ColumnName] = entry.Value;
                continue;
            }

            if (!hasDefinitions)
                clUpdates[entry.Key] = entry.Value;
        }

        return (clUpdates, cl2Updates);
    }

    private static string GenerateStamp()
    {
        var now = DateTime.Now;
        var timestamp = now.ToString("yyyyMMddHHmmss");
        var random = Guid.NewGuid().ToString().Substring(0, 11).ToUpper();
        return $"{timestamp}{random}".PadRight(25, '0')[..25];
    }
}
