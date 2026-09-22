using MediatR;
using Audit.Application.DTOs;
using Audit.Domain.Repositories;
using Audit.Application.Mappings;
using Microsoft.Extensions.Logging;

namespace Audit.Application.Features.GetAuditLogByStamp;


public sealed class GetAuditLogByStampQueryHandler : IRequestHandler<GetAuditLogByStampQuery, AuditLogOutputDTO?>
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<GetAuditLogByStampQueryHandler> _logger;

    public GetAuditLogByStampQueryHandler(
        IAuditLogRepository repository,
        ILogger<GetAuditLogByStampQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<AuditLogOutputDTO?> Handle(
        GetAuditLogByStampQuery request,
        CancellationToken cancellationToken)
    {

        var log = await _repository.GetByStampAsync(request.ULogsstamp, cancellationToken);

        if (log is null)
        {
            return null;
        }

        return log.ToOutputDto();
    }
}
