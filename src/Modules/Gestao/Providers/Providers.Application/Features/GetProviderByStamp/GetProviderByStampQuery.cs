using MediatR;
using Providers.Application.DTOs.Providers;

namespace Providers.Application.Features.GetProviderByStamp;

public record GetProviderByStampQuery(
    string UProviderStamp
) : IRequest<ProviderOutputDTO?>;
