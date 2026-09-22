using MediatR;
using Audit.Application.DTOs;
using Audit.Domain.Repositories;
using Audit.Application.Mappings;
using Microsoft.Extensions.Logging;

namespace Audit.Application.Features.GetAuditLogsByCorrelationId;

public sealed class GetAuditLogsByCorrelationIdQueryHandler 
    : IRequestHandler<GetAuditLogsByCorrelationIdQuery, IEnumerable<AuditLogOutputDTO>>
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<GetAuditLogsByCorrelationIdQueryHandler> _logger;

    public GetAuditLogsByCorrelationIdQueryHandler(
        IAuditLogRepository repository,
        ILogger<GetAuditLogsByCorrelationIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<IEnumerable<AuditLogOutputDTO>> Handle(
        GetAuditLogsByCorrelationIdQuery request,
        CancellationToken cancellationToken)
    {

        var logs = await _repository.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);
        var result = logs.ToOutputDtos().ToList();
        return result;
    }
}
