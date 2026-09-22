using MediatR;
using Audit.Application.DTOs;

namespace Audit.Application.Features.GetAllAuditLogs;


public sealed record GetAllAuditLogsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    string? CorrelationId = null,
    string? Operation = null,
    int PageNumber = 1,
    int PageSize = 50) : IRequest<PagedAuditLogsResult>; // ✅ Mudado para retornar resultado paginado
