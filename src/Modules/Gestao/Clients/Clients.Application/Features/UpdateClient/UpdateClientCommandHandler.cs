using MediatR;
using Clients.Application.DTOs;
using Clients.Domain.Repositories;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Clients.Application.Features.UpdateClient;

/// <summary>
/// Handler para atualizar um cliente existente
/// </summary>
public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, ClientOutputDTO?>
{
    private readonly IClientRepository _repository;
    private readonly IUserFieldService _userFieldService;
    private readonly ITenantContext _tenantContext;

    public UpdateClientCommandHandler(
        IClientRepository repository,
        IUserFieldService userFieldService,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    public async Task<ClientOutputDTO?> Handle(
        UpdateClientCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Buscar cliente existente
        var (cl, cl2) = await _repository.GetByNoAndEstabAsync(request.No, request.Estab, cancellationToken);

        if (cl == null || cl2 == null)
        {
            return null;
        }

        // 2. Atualizar campos (apenas se fornecidos)
        if (!string.IsNullOrEmpty(request.Dto.Nome))
        {
            cl.Nome = request.Dto.Nome;
        }

        if (!string.IsNullOrEmpty(request.Dto.Telefone))
        {
            cl.Telefone = request.Dto.Telefone;
        }

        if (!string.IsNullOrEmpty(request.Dto.Morada))
        {
            cl.Morada = request.Dto.Morada;
        }

        if (!string.IsNullOrEmpty(request.Dto.Email))
        {
            cl.Email = request.Dto.Email;
        }

        if (request.Dto.Inactivo.HasValue)
        {
            cl.Inactivo = request.Dto.Inactivo.Value;
        }

        // 3. Atualizar auditoria
        cl.Usrinis = request.UpdatedBy ?? "PHCAPI";
        cl.Usrdata = DateTime.Now.Date;
        cl.Usrhora = DateTime.Now.ToString("HH:mm:ss");

        cl2.Usrinis = request.UpdatedBy ?? "PHCAPI";
        cl2.Usrdata = DateTime.Now.Date;
        cl2.Usrhora = DateTime.Now.ToString("HH:mm:ss");

        // 4. Persistir
        await _repository.UpdateAsync(cl, cl2, cancellationToken);

        // 5. Retornar DTO
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
}
