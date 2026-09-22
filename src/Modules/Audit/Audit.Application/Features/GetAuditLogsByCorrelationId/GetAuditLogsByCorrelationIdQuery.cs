using MediatR;
using Audit.Application.DTOs;

namespace Audit.Application.Features.GetAuditLogsByCorrelationId;

/// <summary>
/// Query para buscar todos os logs relacionados a um Correlation ID
/// </summary>
public sealed record GetAuditLogsByCorrelationIdQuery(string CorrelationId) : IRequest<IEnumerable<AuditLogOutputDTO>>;
