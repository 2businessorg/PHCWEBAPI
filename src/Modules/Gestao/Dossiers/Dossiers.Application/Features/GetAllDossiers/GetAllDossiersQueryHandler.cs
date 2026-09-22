using Dossiers.Application.DTOs;
using Dossiers.Application.Mappings;
using Dossiers.Domain.Repositories;
using MediatR;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Dossiers.Application.Features.GetAllDossiers;

/// <summary>
/// Handler para listar dossiers paginados
/// </summary>
public class GetAllDossiersQueryHandler : IRequestHandler<GetAllDossiersQuery, GetAllDossiersResultDTO>
{
    private readonly IDossierRepository _repository;
    private readonly IUserFieldService _userFieldService;
    private readonly ITenantContext _tenantContext;

    public GetAllDossiersQueryHandler(
        IDossierRepository repository,
        IUserFieldService userFieldService,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    public async Task<GetAllDossiersResultDTO> Handle(
        GetAllDossiersQuery request,
        CancellationToken cancellationToken)
    {
        var result = await _repository.GetPagedAsync(
            request.Ndos,
            request.Nmdos,
            request.Obrano,
            request.Boano,
            request.No,
            request.Estab,
            request.Nome,
            request.Page,
            request.PageSize,
            request.IncludeLinhas,
            cancellationToken);

        var items = result.Items
            .Select(x => x.ToOutput(includeLines: request.IncludeLinhas))
            .ToList();

        if (_tenantContext.AppLicenseStamp is not null && items.Count > 0)
        {
            var tenant = _tenantContext.AppLicenseStamp;
            var boDefs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bo", cancellationToken);
            var bo2Defs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bo2", cancellationToken);
            var bo3Defs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bo3", cancellationToken);
            var biDefs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bi", cancellationToken);
            var bi2Defs = await _userFieldService.GetFieldsAsync(tenant, "Dossiers", "bi2", cancellationToken);

            var boStamps = result.Items.Select(x => x.Bo.Bostamp).Distinct().ToList();

            var boRawBatch = boDefs.Count > 0
                ? await _repository.GetUserFieldValuesBatchAsync("bo", "bostamp", boStamps, boDefs.Select(f => f.ColumnName).ToList(), cancellationToken)
                : new Dictionary<string, Dictionary<string, object?>>();

            var bo2RawBatch = bo2Defs.Count > 0
                ? await _repository.GetUserFieldValuesBatchAsync("bo2", "bo2stamp", boStamps, bo2Defs.Select(f => f.ColumnName).ToList(), cancellationToken)
                : new Dictionary<string, Dictionary<string, object?>>();

            var bo3RawBatch = bo3Defs.Count > 0
                ? await _repository.GetUserFieldValuesBatchAsync("bo3", "bo3stamp", boStamps, bo3Defs.Select(f => f.ColumnName).ToList(), cancellationToken)
                : new Dictionary<string, Dictionary<string, object?>>();

            Dictionary<string, Dictionary<string, object?>> biRawBatch = [];
            Dictionary<string, Dictionary<string, object?>> bi2RawBatch = [];

            if (request.IncludeLinhas)
            {
                var biStamps = result.Items
                    .SelectMany(x => x.Lines)
                    .Select(l => l.Bistamp)
                    .Distinct()
                    .ToList();

                if (biDefs.Count > 0 && biStamps.Count > 0)
                {
                    biRawBatch = await _repository.GetUserFieldValuesBatchAsync(
                        "bi", "bistamp", biStamps, biDefs.Select(f => f.ColumnName).ToList(), cancellationToken);
                }

                if (bi2Defs.Count > 0 && biStamps.Count > 0)
                {
                    bi2RawBatch = await _repository.GetUserFieldValuesBatchAsync(
                        "bi2", "bi2stamp", biStamps, bi2Defs.Select(f => f.ColumnName).ToList(), cancellationToken);
                }
            }

            for (var i = 0; i < result.Items.Count && i < items.Count; i++)
            {
                var aggregate = result.Items[i];
                var dto = items[i];
                var stamp = aggregate.Bo.Bostamp?.Trim() ?? string.Empty;

                var headerFields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

                if (boDefs.Count > 0 && boRawBatch.TryGetValue(stamp, out var boRaw))
                {
                    foreach (var def in boDefs)
                        headerFields[def.Alias] = UserFieldValueFormatter.FormatForOutput(boRaw.GetValueOrDefault(def.ColumnName), def.FieldType);
                }

                if (bo2Defs.Count > 0 && bo2RawBatch.TryGetValue(stamp, out var bo2Raw))
                {
                    foreach (var def in bo2Defs)
                        if (!headerFields.ContainsKey(def.Alias))
                            headerFields[def.Alias] = UserFieldValueFormatter.FormatForOutput(bo2Raw.GetValueOrDefault(def.ColumnName), def.FieldType);
                }

                if (bo3Defs.Count > 0 && bo3RawBatch.TryGetValue(stamp, out var bo3Raw))
                {
                    foreach (var def in bo3Defs)
                        if (!headerFields.ContainsKey(def.Alias))
                            headerFields[def.Alias] = UserFieldValueFormatter.FormatForOutput(bo3Raw.GetValueOrDefault(def.ColumnName), def.FieldType);
                }

                dto.AddFields = headerFields.Count > 0 ? headerFields : null;

                if (!request.IncludeLinhas)
                    continue;

                for (var lineIndex = 0; lineIndex < aggregate.Lines.Count && lineIndex < dto.Linhas.Count; lineIndex++)
                {
                    var biStamp = aggregate.Lines[lineIndex].Bistamp?.Trim() ?? string.Empty;
                    var lineFields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

                    if (biDefs.Count > 0 && biRawBatch.TryGetValue(biStamp, out var biRaw))
                    {
                        foreach (var def in biDefs)
                            lineFields[def.Alias] = UserFieldValueFormatter.FormatForOutput(biRaw.GetValueOrDefault(def.ColumnName), def.FieldType);
                    }

                    if (bi2Defs.Count > 0 && bi2RawBatch.TryGetValue(biStamp, out var bi2Raw))
                    {
                        foreach (var def in bi2Defs)
                            if (!lineFields.ContainsKey(def.Alias))
                                lineFields[def.Alias] = UserFieldValueFormatter.FormatForOutput(bi2Raw.GetValueOrDefault(def.ColumnName), def.FieldType);
                    }

                    dto.Linhas[lineIndex].AddFields = lineFields.Count > 0 ? lineFields : null;
                }
            }
        }

        return new GetAllDossiersResultDTO(
            result.TotalItems,
            result.CurrentPage,
            result.PageSize,
            items);
    }
}
