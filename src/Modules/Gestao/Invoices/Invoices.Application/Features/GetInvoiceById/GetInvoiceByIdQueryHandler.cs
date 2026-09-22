using Invoices.Application.DTOs;
using Invoices.Application.Mappings;
using Invoices.Domain.Repositories;
using MediatR;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;

namespace Invoices.Application.Features.GetInvoiceById;

/// <summary>
/// Handler para obter fatura por chave composta.
/// </summary>
public class GetInvoiceByIdQueryHandler : IRequestHandler<GetInvoiceByIdQuery, InvoiceOutputDTO?>
{
    private readonly IFaturaRepository _repository;
    private readonly IUserFieldService _userFieldService;
    private readonly ITenantContext _tenantContext;

    public GetInvoiceByIdQueryHandler(
        IFaturaRepository repository,
        IUserFieldService userFieldService,
        ITenantContext tenantContext)
    {
        _repository = repository;
        _userFieldService = userFieldService;
        _tenantContext = tenantContext;
    }

    public async Task<InvoiceOutputDTO?> Handle(GetInvoiceByIdQuery request, CancellationToken cancellationToken)
    {
        var aggregate = await _repository.GetAggregateAsync(request.Ndoc, request.InvoiceNumber, request.Year);
        if (aggregate is null)
            return null;

        var dto = aggregate.Fatura.ToOutput(aggregate.Linhas);

        if (_tenantContext.AppLicenseStamp is null)
            return dto;

        var tenant = _tenantContext.AppLicenseStamp;
        var ftDefs  = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "ft",  cancellationToken);
        var ft2Defs = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "ft2", cancellationToken);
        var ft3Defs = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "ft3", cancellationToken);
        var fiDefs  = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "fi",  cancellationToken);
        var fi2Defs = await _userFieldService.GetFieldsAsync(tenant, "Invoices", "fi2", cancellationToken);

        dto.AddFields = await BuildHeaderAddFieldsAsync(
            aggregate.Fatura.FtStamp,
            aggregate.DadosSecundarios.Ft2Stamp,
            aggregate.DadosAdicionais.Ft3Stamp,
            ftDefs, ft2Defs, ft3Defs,
            cancellationToken);

        if (dto.Linhas.Count > 0 && aggregate.Linhas.Count > 0)
        {
            for (var i = 0; i < dto.Linhas.Count && i < aggregate.Linhas.Count; i++)
            {
                var fiStamp  = aggregate.Linhas[i].Fistamp;
                var fi2Stamp = i < aggregate.LinhasAdicionais.Count
                    ? aggregate.LinhasAdicionais[i].Fi2Stamp
                    : fiStamp;

                dto.Linhas[i].AddFields = await BuildLineAddFieldsAsync(
                    fiStamp, fi2Stamp, fiDefs, fi2Defs, cancellationToken);
            }
        }

        return dto;
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
