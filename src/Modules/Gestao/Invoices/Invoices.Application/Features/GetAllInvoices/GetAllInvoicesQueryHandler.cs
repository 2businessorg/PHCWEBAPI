using Invoices.Application.DTOs;
using Invoices.Application.Mappings;
using Invoices.Domain.Repositories;
using MediatR;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Invoices.Application.Features.GetAllInvoices;

/// <summary>
/// Handler para listar faturas paginadas.
/// </summary>
public class GetAllInvoicesQueryHandler : IRequestHandler<GetAllInvoicesQuery, GetAllInvoicesResultDTO>
{
    private readonly IFaturaRepository _repository;
    private readonly IUserFieldService? _userFieldService;
    private readonly ITenantContext? _tenantContext;

    public GetAllInvoicesQueryHandler(IFaturaRepository repository, IUserFieldService? userFieldService = null, ITenantContext? tenantContext = null)
    {
        _repository = repository;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    /// <summary>
    /// Executa a query de listagem de faturas.
    /// </summary>
    public async Task<GetAllInvoicesResultDTO> Handle(GetAllInvoicesQuery request, CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 20 : request.PageSize;

        IEnumerable<Invoices.Domain.Entities.Ft> faturas = request.ClientNumber.HasValue
            ? await _repository.GetByClientAsync(request.ClientNumber.Value, page, pageSize)
            : await _repository.GetAllAsync(page, pageSize);

        if (request.Ndoc.HasValue)
            faturas = faturas.Where(f => f.Ndoc == request.Ndoc.Value);
        if (request.InvoiceNumber.HasValue)
            faturas = faturas.Where(f => f.Fno == request.InvoiceNumber.Value);
        if (request.Year.HasValue)
            faturas = faturas.Where(f => f.FtAno == request.Year.Value);

        var list = faturas.ToList();

        IReadOnlyList<InvoiceOutputDTO> items;
        if (request.IncludeLines)
        {
            var mapped = await Task.WhenAll(list.Select(async f =>
            {
                var linhas = await _repository.GetLinesAsync(f.FtStamp);
                return f.ToOutput(linhas);
            }));
            items = mapped.ToList();
        }
        else
        {
            items = list.Select(f => f.ToOutput()).ToList();
        }

        // Populate addFields if services are available
        if (_userFieldService != null && _tenantContext != null && _tenantContext.AppLicenseStamp != null)
        {
            var tenant = _tenantContext.AppLicenseStamp;
            var ftDefs  = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "ft",  cancellationToken);
            var ft2Defs = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "ft2", cancellationToken);
            var ft3Defs = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "ft3", cancellationToken);
            var fiDefs  = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "fi",  cancellationToken);
            var fi2Defs = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "fi2", cancellationToken);

            for (int i = 0; i < items.Count && i < list.Count; i++)
            {
                var ft = list[i];
                var aggregate = await _repository.GetAggregateAsync((int)ft.Ndoc, (int)ft.Fno, (int)ft.FtAno);
                if (aggregate != null)
                {
                    items[i].AddFields = await BuildHeaderAddFieldsAsync(
                        ft.FtStamp,
                        aggregate.DadosSecundarios.Ft2Stamp,
                        aggregate.DadosAdicionais.Ft3Stamp,
                        ftDefs, ft2Defs, ft3Defs,
                        cancellationToken);

                    if (request.IncludeLines && items[i].Linhas.Count > 0 && aggregate.Linhas.Count > 0)
                    {
                        for (var j = 0; j < items[i].Linhas.Count && j < aggregate.Linhas.Count; j++)
                        {
                            var fiStamp  = aggregate.Linhas[j].Fistamp;
                            var fi2Stamp = j < aggregate.LinhasAdicionais.Count
                                ? aggregate.LinhasAdicionais[j].Fi2Stamp
                                : fiStamp;

                            items[i].Linhas[j].AddFields = await BuildLineAddFieldsAsync(
                                fiStamp, fi2Stamp, fiDefs, fi2Defs, cancellationToken);
                        }
                    }
                }
            }
        }

        return new GetAllInvoicesResultDTO(
            TotalItems: items.Count,
            CurrentPage: page,
            PageSize: pageSize,
            Items: items);
    }

    private async Task<Dictionary<string, object?>?> BuildHeaderAddFieldsAsync(
        string ftStamp,
        string ft2Stamp,
        string ft3Stamp,
        IReadOnlyList<UserFieldDefinition> ftDefs,
        IReadOnlyList<UserFieldDefinition> ft2Defs,
        IReadOnlyList<UserFieldDefinition> ft3Defs,
        CancellationToken cancellationToken)
    {
        var fields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (ftDefs.Count > 0)
        {
            var raw = await _repository.GetUserFieldValuesAsync("ft", "ftstamp", ftStamp, ftDefs.Select(f => f.ColumnName).ToList(), cancellationToken);
            foreach (var def in ftDefs)
                fields[def.Alias] = UserFieldValueFormatter.FormatForOutput(raw.GetValueOrDefault(def.ColumnName), def.FieldType);
        }

        if (ft2Defs.Count > 0 && !string.IsNullOrWhiteSpace(ft2Stamp))
        {
            var raw = await _repository.GetUserFieldValuesAsync("ft2", "ft2stamp", ft2Stamp, ft2Defs.Select(f => f.ColumnName).ToList(), cancellationToken);
            foreach (var def in ft2Defs)
                if (!fields.ContainsKey(def.Alias))
                    fields[def.Alias] = UserFieldValueFormatter.FormatForOutput(raw.GetValueOrDefault(def.ColumnName), def.FieldType);
        }

        if (ft3Defs.Count > 0 && !string.IsNullOrWhiteSpace(ft3Stamp))
        {
            var raw = await _repository.GetUserFieldValuesAsync("ft3", "ft3stamp", ft3Stamp, ft3Defs.Select(f => f.ColumnName).ToList(), cancellationToken);
            foreach (var def in ft3Defs)
                if (!fields.ContainsKey(def.Alias))
                    fields[def.Alias] = UserFieldValueFormatter.FormatForOutput(raw.GetValueOrDefault(def.ColumnName), def.FieldType);
        }

        return fields.Count > 0 ? fields : null;
    }

    private async Task<Dictionary<string, object?>?> BuildLineAddFieldsAsync(
        string fiStamp,
        string fi2Stamp,
        IReadOnlyList<UserFieldDefinition> fiDefs,
        IReadOnlyList<UserFieldDefinition> fi2Defs,
        CancellationToken cancellationToken)
    {
        var fields = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

        if (fiDefs.Count > 0)
        {
            var raw = await _repository.GetUserFieldValuesAsync("fi", "fistamp", fiStamp, fiDefs.Select(f => f.ColumnName).ToList(), cancellationToken);
            foreach (var def in fiDefs)
                fields[def.Alias] = UserFieldValueFormatter.FormatForOutput(raw.GetValueOrDefault(def.ColumnName), def.FieldType);
        }

        if (fi2Defs.Count > 0 && !string.IsNullOrWhiteSpace(fi2Stamp))
        {
            var raw = await _repository.GetUserFieldValuesAsync("fi2", "fi2stamp", fi2Stamp, fi2Defs.Select(f => f.ColumnName).ToList(), cancellationToken);
            foreach (var def in fi2Defs)
                if (!fields.ContainsKey(def.Alias))
                    fields[def.Alias] = UserFieldValueFormatter.FormatForOutput(raw.GetValueOrDefault(def.ColumnName), def.FieldType);
        }

        return fields.Count > 0 ? fields : null;
    }
}
