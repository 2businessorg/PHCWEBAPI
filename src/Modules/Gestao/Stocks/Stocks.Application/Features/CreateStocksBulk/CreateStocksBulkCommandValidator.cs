using FluentValidation;
using Stocks.Application.Errors;

namespace Stocks.Application.Features.CreateStocksBulk;

public sealed class CreateStocksBulkCommandValidator : AbstractValidator<CreateStocksBulkCommand>
{
    public CreateStocksBulkCommandValidator()
    {
        RuleFor(x => x.Dto.Items)
            .NotNull()
            .Must(x => x != null && x.Count > 0)
            .WithMessage(StocksErrorCatalog.BulkEmpty.Description)
            .WithErrorCode(StocksErrorCatalog.BulkEmpty.Code)
            .Must(x => x != null && x.Count <= 100)
            .WithMessage(string.Format(StocksErrorCatalog.BulkMaxItemsExceeded.Description, 100))
            .WithErrorCode(StocksErrorCatalog.BulkMaxItemsExceeded.Code);
    }
}
