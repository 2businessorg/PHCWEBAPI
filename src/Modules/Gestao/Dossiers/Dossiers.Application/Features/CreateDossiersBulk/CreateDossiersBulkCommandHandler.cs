using Dossiers.Application.DTOs;
using Dossiers.Application.Errors;
using Dossiers.Application.Features.CreateDossier;
using FluentValidation;
using MediatR;
using Shared.Kernel.Responses;

namespace Dossiers.Application.Features.CreateDossiersBulk;

/// <summary>
/// Handler para criação em lote de dossiers (best-effort)
/// </summary>
public class CreateDossiersBulkCommandHandler : IRequestHandler<CreateDossiersBulkCommand, CreateDossiersBulkResponseDTO>
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateDossierCommand> _singleValidator;

    public CreateDossiersBulkCommandHandler(IMediator mediator, IValidator<CreateDossierCommand> singleValidator)
    {
        _mediator = mediator;
        _singleValidator = singleValidator;
    }

    public async Task<CreateDossiersBulkResponseDTO> Handle(CreateDossiersBulkCommand request, CancellationToken cancellationToken)
    {
        var response = new CreateDossiersBulkResponseDTO();
        var results = new List<Shared.Kernel.Responses.BulkItemResultDTO>();

        // Validar lote vazio
        if (request.Dto.Items == null || request.Dto.Items.Count == 0)
        {
            response.Code = DossiersErrorCatalog.BulkEmpty.Code;
            response.Message = DossiersErrorCatalog.BulkEmpty.Description;
            response.Items = results;
            response.SuccessCount = 0;
            response.FailureCount = 0;
            return response;
        }

        // Validar tamanho máximo do lote
        if (request.Dto.Items.Count > 100)
        {
            response.Code = DossiersErrorCatalog.BulkMaxItemsExceeded.Code;
            response.Message = $"Lote não pode conter mais de 100 itens";
            response.Items = results;
            response.SuccessCount = 0;
            response.FailureCount = request.Dto.Items.Count;
            return response;
        }

        // Processar cada item: best-effort (insere os que consegue, captura erros dos que não consegue)
        for (int i = 0; i < request.Dto.Items.Count; i++)
        {
            var itemResult = new Shared.Kernel.Responses.BulkItemResultDTO { Index = i };
            var dto = request.Dto.Items[i];

            try
            {
                // Validar item individual
                var command = new CreateDossierCommand(dto, request.CreatedBy);
                var validationResult = await _singleValidator.ValidateAsync(command, cancellationToken);

                if (!validationResult.IsValid)
                {
                    var first = validationResult.Errors.FirstOrDefault();
                    itemResult.Success = false;
                    itemResult.Data = null;
                    itemResult.Error = new BulkErrorDTO
                    {
                        Code = first?.ErrorCode ?? DossiersErrorCatalog.ValidationError.Code,
                        Message = first?.ErrorMessage ?? "Erro de validação"
                    };
                    response.FailureCount++;
                }
                else
                {
                    // Criar dossier se passou validação
                    var created = await _mediator.Send(command, cancellationToken);
                    itemResult.Success = true;
                    itemResult.Data = created;
                    itemResult.Error = null;
                    response.SuccessCount++;
                }
            }
            catch (DossiersModuleException ex)
            {
                itemResult.Success = false;
                itemResult.Data = null;
                itemResult.Error = new BulkErrorDTO
                {
                    Code = ex.Code,
                    Message = ex.Message
                };
                response.FailureCount++;
            }
            catch (ValidationException ex)
            {
                var first = ex.Errors.FirstOrDefault();
                itemResult.Success = false;
                itemResult.Data = null;
                itemResult.Error = new BulkErrorDTO
                {
                    Code = first?.ErrorCode ?? DossiersErrorCatalog.ValidationError.Code,
                    Message = first?.ErrorMessage ?? ex.Message
                };
                response.FailureCount++;
            }
            catch (Exception ex)
            {
                itemResult.Success = false;
                itemResult.Data = null;
                itemResult.Error = new BulkErrorDTO
                {
                    Code = DossiersErrorCatalog.DatabaseUpdateError.Code,
                    Message = ex.Message
                };
                response.FailureCount++;
            }

            results.Add(itemResult);
        }

        response.Code = response.FailureCount > 0 && response.SuccessCount == 0 
            ? DossiersErrorCatalog.BulkPartialSuccess.Code 
            : DossiersErrorCatalog.Success.Code;

        response.Message = response.FailureCount > 0
            ? $"Lote processado: {response.SuccessCount} sucessos, {response.FailureCount} falhas"
            : $"{response.SuccessCount} dossier(s) criado(s) com sucesso";

        response.Items = results;
        return response;
    }
}
