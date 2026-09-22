using MediatR;
using Audit.Application.DTOs;

namespace Audit.Application.Features.GetAuditLogByStamp;


public sealed record GetAuditLogByStampQuery(string ULogsstamp) : IRequest<AuditLogOutputDTO?>;
