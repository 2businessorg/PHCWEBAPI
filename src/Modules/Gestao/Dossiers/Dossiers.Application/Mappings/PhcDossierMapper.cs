using Dossiers.Application.DTOs;
using Dossiers.Application.Features.CreateDossier;
using Dossiers.Domain.Models;

namespace Dossiers.Application.Mappings;

/// <summary>
/// Mapper para converter entre DTOs de Dossier e modelos PHC
/// </summary>
public static class PhcDossierMapper
{
    /// <summary>
    /// Converte CreateDossierCommand para requisição PHC
    /// Inclui campos opcionais: Boano, Estab, Nome, Data, Moeda
    /// Linhas também incluem: Design, PrecoUnitario, TabIva, IvaIncl
    /// </summary>
    public static PhcInsertBoRequest ToPhcRequest(this CreateDossierCommand command)
    {
        var dto = command.Dto;
        var request = new PhcInsertBoRequest
        {
            Ndos = (int)dto.Ndos,
            No = (int)dto.No,
            Boano = dto.Boano.HasValue ? (int)dto.Boano.Value : null,
            Estab = dto.Estab.HasValue ? (int)dto.Estab.Value : null,
            Nome = string.IsNullOrEmpty(dto.Nome) ? null : dto.Nome,
            Data = dto.Data.HasValue ? dto.Data.Value.ToString("yyyy-MM-dd") : null,
            Moeda = string.IsNullOrEmpty(dto.Moeda) ? null : dto.Moeda,
            CreatedBy = command.CreatedBy,
            LstBi = dto.Linhas.Select(line => new PhcLinhaModel
            {
                Ref = line.Referencia,
                Qtt = line.Quantidade,
                Design = string.IsNullOrEmpty(line.Descricao) ? null : line.Descricao,
                PrecoUnitario = line.PrecoUnitario,
                TabIva = line.TabIva.HasValue ? (int)line.TabIva.Value : null,
                IvaIncl = line.IvaIncl
            }).ToList()
        };

        return request;
    }

    /// <summary>
    /// Converte resposta PHC para DossierOutputDTO
    /// </summary>
    public static DossierOutputDTO ToOutputDTO(this PhcInsertBoResponse phcResponse)
    {
        if (!int.TryParse(phcResponse.Ndos, out var docTypeId))
            docTypeId = 0;

        if (!int.TryParse(phcResponse.Obrano, out var docNumber))
            docNumber = 0;

        if (!int.TryParse(phcResponse.Boano, out var year))
            year = DateTime.Now.Year;

        if (!decimal.TryParse(phcResponse.No, out var entityId))
            entityId = 0;

        if (!decimal.TryParse(phcResponse.Estab, out var entityBranch))
            entityBranch = 0;

        // Parsear data
        var date = DateTime.TryParse(phcResponse.Data, out var parsedDate)
            ? parsedDate
            : DateTime.Now;

        // Converter linhas
        var lines = phcResponse.Linhas.Select(linha => new DossierLineOutputDTO
        {
            Ref = linha.Ref,
            Design= linha.Design,
            Qtt = linha.Qtt,
            tabIva = int.TryParse(linha.Tabiva, out var vatCode) ? vatCode : 0,
            iva = linha.Iva,
            IvaIncl = linha.Ivaincl,
            PrecoUnitario = linha.Ttdeb > 0 && linha.Qtt > 0 ? linha.Ttdeb / linha.Qtt : 0,
            Total = linha.Ttdeb
        }).ToList();

        return new DossierOutputDTO
        {
            Ndos = docTypeId,
            Nmdos = phcResponse.Nmdos,
            Obrano = docNumber,
            Boano = year,
            No = (int)entityId,
            Estab = (int)entityBranch,
            Nome = phcResponse.Nome,
            Data = date,
            Moeda = phcResponse.Moeda,
            Total = phcResponse.Total,
            Linhas = lines
        };
    }
}
