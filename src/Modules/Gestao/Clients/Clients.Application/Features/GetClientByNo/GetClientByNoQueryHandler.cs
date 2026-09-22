using MediatR;
using Clients.Application.DTOs;
using Clients.Domain.Repositories;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Clients.Application.Features.GetClientByNo;

/// <summary>
/// Handler para buscar cliente pelo No + Estab
/// </summary>
public class GetClientByNoQueryHandler : IRequestHandler<GetClientByNoQuery, ClientOutputDTO?>
{
    private readonly IClientRepository _repository;
    private readonly IUserFieldService _userFieldService;
    private readonly ITenantContext _tenantContext;

    public GetClientByNoQueryHandler(
        IClientRepository repository,
        IUserFieldService userFieldService,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    public async Task<ClientOutputDTO?> Handle(
        GetClientByNoQuery request,
        CancellationToken cancellationToken)
    {
        var (cl, cl2) = await _repository.GetByNoAndEstabAsync(request.No, request.Estab, cancellationToken);

        if (cl == null || cl2 == null)
            return null;

        var dto = MapToDTO(cl, cl2);

        // Enrich with user fields if the tenant has any configured
        if (_tenantContext.AppLicenseStamp is not null)
        {
            var clFieldDefs = await _userFieldService.GetFieldsAsync(
                _tenantContext.AppLicenseStamp,
                "Clients",
                "cl",
                cancellationToken);

            var cl2FieldDefs = await _userFieldService.GetFieldsAsync(
                _tenantContext.AppLicenseStamp,
                "Clients",
                "cl2",
                cancellationToken);

            if (clFieldDefs.Count > 0 || cl2FieldDefs.Count > 0)
            {
                Dictionary<string, object?> clRawValues = [];
                Dictionary<string, object?> cl2RawValues = [];

                if (clFieldDefs.Count > 0)
                {
                    var clColumns = clFieldDefs.Select(f => f.ColumnName).ToList();
                    clRawValues = await _repository.GetUserFieldValuesAsync(cl.Clstamp, clColumns, cancellationToken);
                }

                if (cl2FieldDefs.Count > 0)
                {
                    var cl2Columns = cl2FieldDefs.Select(f => f.ColumnName).ToList();
                    cl2RawValues = await _repository.GetUserFieldValuesFromCl2Async(cl.Clstamp, cl2Columns, cancellationToken);
                }

                var userFields = new Dictionary<string, object?>();

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

                dto.AddFields = userFields;
            }
        }

        return dto;
    }

    private static ClientOutputDTO MapToDTO(Domain.Entities.Cl cl, Domain.Entities.Cl2 cl2)
    {
        return new ClientOutputDTO
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
    }
}

