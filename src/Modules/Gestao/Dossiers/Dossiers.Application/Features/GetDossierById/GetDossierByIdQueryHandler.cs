using Dossiers.Application.DTOs;
using Dossiers.Application.Mappings;
using Dossiers.Domain.Repositories;
using MediatR;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Dossiers.Application.Features.GetDossierById;

/// <summary>
/// Handler para obter dossier por chave composta
/// </summary>
public class GetDossierByIdQueryHandler : IRequestHandler<GetDossierByIdQuery, DossierOutputDTO?>
{
    private readonly IDossierRepository _repository;
    private readonly IUserFieldService _userFieldService;
    private readonly ITenantContext _tenantContext;

    public GetDossierByIdQueryHandler(
        IDossierRepository repository,
        IUserFieldService userFieldService,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    public async Task<DossierOutputDTO?> Handle(
        GetDossierByIdQuery request,
        CancellationToken cancellationToken)
    {
        var aggregate = await _repository.GetByKeyAsync(request.Ndos, request.Obrano, request.Boano, cancellationToken);
        if (aggregate is null)
            return null;

        var dto = aggregate.ToOutput(includeLines: true);

        if (_tenantContext.AppLicenseStamp is null)
            return dto;

        var tenant = _tenantContext.AppLicenseStamp;
        var boDefs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bo", cancellationToken);
        var bo2Defs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bo2", cancellationToken);
        var bo3Defs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bo3", cancellationToken);
        var biDefs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bi", cancellationToken);
        var bi2Defs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bi2", cancellationToken);

        dto.AddFields = await BuildHeaderAddFieldsAsync(aggregate.Bo.Bostamp, boDefs, bo2Defs, bo3Defs, cancellationToken);

        if (dto.Linhas.Count > 0 && aggregate.Lines.Count > 0)
        {
            for (var i = 0; i < dto.Linhas.Count && i < aggregate.Lines.Count; i++)
            {
                var biStamp = aggregate.Lines[i].Bistamp;
                var bi2Stamp = i < aggregate.Lines2.Count ? aggregate.Lines2[i].Bi2stamp : biStamp;
                dto.Linhas[i].AddFields = await BuildLineAddFieldsAsync(biStamp, bi2Stamp, biDefs, bi2Defs, cancellationToken);
            }
        }

        return dto;
    }

    private async Task<Dictionary<string, object?>?> BuildHeaderAddFieldsAsync(
        string boStamp,
        IReadOnlyList<UserFieldDefinition> boDefs,
        IReadOnlyList<UserFieldDefinition> bo2Defs,
        IReadOnlyList<UserFieldDefinition> bo3Defs,
        CancellationToken cancellationToken)
    {
        var fields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (boDefs.Count > 0)
        {
            var raw = await _repository.GetUserFieldValuesAsync("bo", "bostamp", boStamp, boDefs.Select(f => f.ColumnName).ToList(), cancellationToken);
            foreach (var def in boDefs)
                fields[def.Alias] = UserFieldValueFormatter.FormatForOutput(raw.GetValueOrDefault(def.ColumnName), def.FieldType);
        }

        if (bo2Defs.Count > 0)
        {
            var raw = await _repository.GetUserFieldValuesAsync("bo2", "bo2stamp", boStamp, bo2Defs.Select(f => f.ColumnName).ToList(), cancellationToken);
            foreach (var def in bo2Defs)
                if (!fields.ContainsKey(def.Alias))
                    fields[def.Alias] = UserFieldValueFormatter.FormatForOutput(raw.GetValueOrDefault(def.ColumnName), def.FieldType);
        }

        if (bo3Defs.Count > 0)
        {
            var raw = await _repository.GetUserFieldValuesAsync("bo3", "bo3stamp", boStamp, bo3Defs.Select(f => f.ColumnName).ToList(), cancellationToken);
            foreach (var def in bo3Defs)
                if (!fields.ContainsKey(def.Alias))
                    fields[def.Alias] = UserFieldValueFormatter.FormatForOutput(raw.GetValueOrDefault(def.ColumnName), def.FieldType);
        }

        return fields.Count > 0 ? fields : null;
    }

    private async Task<Dictionary<string, object?>?> BuildLineAddFieldsAsync(
        string biStamp,
        string bi2Stamp,
        IReadOnlyList<UserFieldDefinition> biDefs,
        IReadOnlyList<UserFieldDefinition> bi2Defs,
        CancellationToken cancellationToken)
    {
        var fields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (biDefs.Count > 0)
        {
            var raw = await _repository.GetUserFieldValuesAsync("bi", "bistamp", biStamp, biDefs.Select(f => f.ColumnName).ToList(), cancellationToken);
            foreach (var def in biDefs)
                fields[def.Alias] = UserFieldValueFormatter.FormatForOutput(raw.GetValueOrDefault(def.ColumnName), def.FieldType);
        }

        if (bi2Defs.Count > 0)
        {
            var raw = await _repository.GetUserFieldValuesAsync("bi2", "bi2stamp", bi2Stamp, bi2Defs.Select(f => f.ColumnName).ToList(), cancellationToken);
            foreach (var def in bi2Defs)
                if (!fields.ContainsKey(def.Alias))
                    fields[def.Alias] = UserFieldValueFormatter.FormatForOutput(raw.GetValueOrDefault(def.ColumnName), def.FieldType);
        }

        return fields.Count > 0 ? fields : null;
    }
}
