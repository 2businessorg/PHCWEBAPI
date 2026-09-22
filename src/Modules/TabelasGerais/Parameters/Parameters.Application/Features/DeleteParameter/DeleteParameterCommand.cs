using MediatR;

namespace Parameters.Application.Features.DeleteParameter;


public record DeleteParameterCommand(string Para1Stamp) : IRequest<bool>;
