using MediatR;
using Clients.Application.DTOs;
using Clients.Domain.Repositories;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Clients.Application.Features.GetAllClients;

/// <summary>
/// Handler para buscar todos os clientes
/// </summary>
public class GetAllClientsQueryHandler : IRequestHandler<GetAllClientsQuery, GetAllClientsResultDTO>
{
    private readonly IClientRepository _repository;
    private readonly IUserFieldService _userFieldService;
    private readonly ITenantContext _tenantContext;

    public GetAllClientsQueryHandler(
        IClientRepository repository,
        IUserFieldService userFieldService,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    public async Task<GetAllClientsResultDTO> Handle(
        GetAllClientsQuery request,
        CancellationToken cancellationToken)
    {
        var (totalItems, currentPage, pageSize, clients) = await _repository.GetPagedAsync(
            request.No,
            request.Ncont,
            request.Nome,
            request.Telefone,
            request.Morada,
            request.Estab,
            request.Page,
            request.PageSize,
            cancellationToken);

        // Resolve user field definitions for this tenant (cached)
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

        // Batch-fetch user field values by table (single query per table)
        Dictionary<string, Dictionary<string, object?>> clUserFieldValues = [];
        Dictionary<string, Dictionary<string, object?>> cl2UserFieldValues = [];
        var stamps = clients.Select(x => x.cl.Clstamp).ToList();

        if (clFieldDefs.Count > 0)
        {
            var clColumns = clFieldDefs.Select(f => f.ColumnName).ToList();
            clUserFieldValues = await _repository.GetUserFieldValuesBatchAsync(stamps, clColumns, cancellationToken);
        }

        if (cl2FieldDefs.Count > 0)
        {
            var cl2Columns = cl2FieldDefs.Select(f => f.ColumnName).ToList();
            cl2UserFieldValues = await _repository.GetUserFieldValuesFromCl2BatchAsync(stamps, cl2Columns, cancellationToken);
        }

        var items = clients
            .Select(x => MapToDTO(x.cl, x.cl2, clFieldDefs, cl2FieldDefs, clUserFieldValues, cl2UserFieldValues))
            .ToList();

        return new GetAllClientsResultDTO(totalItems, currentPage, pageSize, items);
    }

    private static ClientOutputDTO MapToDTO(
        Domain.Entities.Cl cl,
        Domain.Entities.Cl2 cl2,
        IReadOnlyList<UserFieldDefinition> clFieldDefs,
        IReadOnlyList<UserFieldDefinition> cl2FieldDefs,
        Dictionary<string, Dictionary<string, object?>> clUserFieldValues,
        Dictionary<string, Dictionary<string, object?>> cl2UserFieldValues)
    {
        var dto = new ClientOutputDTO
        {
            Nome = cl.Nome,
            Ncont = cl.Ncont,
            Telefone = cl.Telefone,
            Morada = cl.Morada,
            Email = cl.Email,
            No = cl.No,
            Estab = cl.Estab,
            Inactivo = cl.Inactivo,
        };

        if (clFieldDefs.Count > 0 || cl2FieldDefs.Count > 0)
        {
            var stampKey = cl.Clstamp.Trim();
            clUserFieldValues.TryGetValue(stampKey, out var clRawValues);
            cl2UserFieldValues.TryGetValue(stampKey, out var cl2RawValues);

            var userFields = new Dictionary<string, object?>();

            foreach (var field in clFieldDefs)
            {
                if (!userFields.ContainsKey(field.Alias))
                    userFields[field.Alias] = UserFieldValueFormatter.FormatForOutput(clRawValues?.GetValueOrDefault(field.ColumnName), field.FieldType);
            }

            foreach (var field in cl2FieldDefs)
            {
                if (!userFields.ContainsKey(field.Alias))
                    userFields[field.Alias] = UserFieldValueFormatter.FormatForOutput(cl2RawValues?.GetValueOrDefault(field.ColumnName), field.FieldType);
            }

            dto.AddFields = userFields;
        }

        return dto;
    }
}
