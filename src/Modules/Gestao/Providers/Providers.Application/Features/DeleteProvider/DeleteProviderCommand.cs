using MediatR;

namespace Providers.Application.Features.DeleteProvider;

public record DeleteProviderCommand(
    string UProviderStamp
) : IRequest<bool>;
