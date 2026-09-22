using MediatR;
using Shared.Kernel.Responses;
using Stocks.Application.DTOs;
using Stocks.Application.Errors;
using Stocks.Application.Mappers;
using Stocks.Domain.Entities;
using Stocks.Domain.Repositories;
using Shared.Kernel.Extensions;

namespace Stocks.Application.Features.CreateStocksBulk;

public sealed class CreateStocksBulkCommandHandler : IRequestHandler<CreateStocksBulkCommand, CreateStocksBulkResponseDTO>
{
    private readonly IStockRepository _repository;

    public CreateStocksBulkCommandHandler(IStockRepository repository)
    {
        _repository = repository;
    }

    public async Task<CreateStocksBulkResponseDTO> Handle(CreateStocksBulkCommand request, CancellationToken cancellationToken)
    {
        var items = request.Dto.Items;
        var response = new CreateStocksBulkResponseDTO();
        var validEntities = new List<St>();
        var results = new List<Shared.Kernel.Responses.BulkItemResultDTO>();

        var duplicatedInBatch = items
            .GroupBy(x => StockMapper.NormalizeString(x.Referencia), StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(g => !string.IsNullOrWhiteSpace(g.Key) && g.Count() > 1);

        if (duplicatedInBatch is not null)
        {
            throw new StocksModuleException(
                StocksErrorCatalog.BulkDuplicateReference.Code,
                string.Format(StocksErrorCatalog.BulkDuplicateReference.Description, duplicatedInBatch.Key));
        }

        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];

            if (string.IsNullOrWhiteSpace(item.Referencia) || item.Referencia.Length > 18)
            {
                results.Add(new Shared.Kernel.Responses.BulkItemResultDTO
                {
                    Index = i,
                    Success = false,
                    Data = null,
                    Error = new BulkErrorDTO
                    {
                        Code = StocksErrorCatalog.BulkItemValidationFailed.Code,
                        Message = string.Format(StocksErrorCatalog.BulkItemValidationFailed.Description, i, "Referência inválida")
                    }
                });
                continue;
            }

            var normalizedRef = StockMapper.NormalizeString(item.Referencia);
            var normalizedDesc = StockMapper.NormalizeString(item.Descricao);

            if (string.IsNullOrWhiteSpace(normalizedDesc) || normalizedDesc.Length > 60)
            {
                results.Add(new Shared.Kernel.Responses.BulkItemResultDTO
                {
                    Index = i,
                    Success = false,
                    Data = null,
                    Error = new BulkErrorDTO
                    {
                        Code = StocksErrorCatalog.BulkItemValidationFailed.Code,
                        Message = string.Format(StocksErrorCatalog.BulkItemValidationFailed.Description, i, "Descrição inválida")
                    }
                });
                continue;
            }

            var alreadyExists = await _repository.ExistsByRefAsync(normalizedRef, cancellationToken);
            if (alreadyExists)
            {
                results.Add(new Shared.Kernel.Responses.BulkItemResultDTO
                {
                    Index = i,
                    Success = false,
                    Data = null,
                    Error = new BulkErrorDTO
                    {
                        Code = StocksErrorCatalog.StockRefAlreadyExists.Code,
                        Message = string.Format(StocksErrorCatalog.StockRefAlreadyExists.Description, normalizedRef)
                    }
                });
                continue;
            }

            var now = DateTime.Now;
            var normalizedFamilia = StockMapper.NormalizeString(item.FamiliaRef);

            // Converter array de preços para campos individuais
            var (pv1, iva1, pv2, iva2, pv3, iva3, pv4, iva4, pv5, iva5) = 
                StockMapper.ConvertPrecosArrayToFields(item.Precos);

            var entity = new St
            {
                Ststamp = StampExtensions.GenerateStamp(),
                Ref = normalizedRef,
                Design = normalizedDesc,
                Stns = item.Eservico,
                Familia = normalizedFamilia,
                Pv1 = pv1,
                Epv1 = pv1,
                Iva1incl = iva1,
                Pv2 = pv2,
                Epv2 = pv2,
                Iva2incl = iva2,
                Pv3 = pv3,
                Epv3 = pv3,
                Iva3incl = iva3,
                Pv4 = pv4,
                Epv4 = pv4,
                Iva4incl = iva4,
                Pv5 = pv5,
                Epv5 = pv5,
                Iva5incl = iva5,
                Tabiva = (int)item.TabIva,
                Obs = StockMapper.NormalizeString(item.Obs),
                Inactivo = item.Inactivo,
                Ousrinis = request.CreatedBy ?? "PHCAPI",
                Ousrdata = now.Date,
                Ousrhora = now.ToString("HH:mm:ss"),
                Usrinis = request.CreatedBy ?? "PHCAPI",
                Usrdata = now.Date,
                Usrhora = now.ToString("HH:mm:ss")
            };

            validEntities.Add(entity);
            
            // Converter campos individuais de volta para array de preços no retorno
            var precosArray = StockMapper.ConvertFieldsToPrecosArray(
                pv1, iva1, pv2, iva2, pv3, iva3, pv4, iva4, pv5, iva5);

            results.Add(new Shared.Kernel.Responses.BulkItemResultDTO
            {
                Index = i,
                Success = true,
                Data = new StockOutputDTO
                {
                    Referencia = entity.Ref,
                    Descricao = entity.Design,
                    Eservico = entity.Stns,
                    Precos = precosArray,
                    Stock = entity.Stock,
                    TabIva = (int)entity.Tabiva,
                    FamiliaRef = entity.Familia,
                    FamiliaNome = entity.Faminome,
                    Obs = entity.Obs,
                    Inactivo = entity.Inactivo
                },
                Error = null
            });
        }

        if (validEntities.Count > 0)
        {
            await _repository.AddRangeAsync(validEntities, cancellationToken);
        }

        response.Items = results;
        response.SuccessCount = results.Count(x => x.Success == true);
        response.FailureCount = results.Count(x => x.Success == false);

        if (response.FailureCount == 0)
        {
            response.Code = StocksErrorCatalog.Success.Code;
            response.Message = $"{response.SuccessCount} stock(s) criado(s) com sucesso";
        }
        else
        {
            response.Code = StocksErrorCatalog.BulkProcessed.Code;
            response.Message = string.Format(
                StocksErrorCatalog.BulkProcessed.Description,
                response.SuccessCount,
                response.FailureCount);
        }

        return response;
    }
}
