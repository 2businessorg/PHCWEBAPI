using MediatR;
using Audit.Application.DTOs;
using Audit.Domain.Repositories;
using Audit.Application.Mappings;
using Microsoft.Extensions.Logging;

namespace Audit.Application.Features.GetAllAuditLogs;


public sealed class GetAllAuditLogsQueryHandler : IRequestHandler<GetAllAuditLogsQuery, PagedAuditLogsResult>
{
    private readonly IAuditLogRepository _repository;
    private readonly ILogger<GetAllAuditLogsQueryHandler> _logger;

    public GetAllAuditLogsQueryHandler(
        IAuditLogRepository repository,
        ILogger<GetAllAuditLogsQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<PagedAuditLogsResult> Handle(
        GetAllAuditLogsQuery request,
        CancellationToken cancellationToken)
    {
       

        var (logs, totalCount) = await _repository.GetPagedAsync(
            startDate: request.StartDate,
            endDate: request.EndDate,
            correlationId: request.CorrelationId,
            operation: request.Operation,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            cancellationToken);

  
        var dtos = logs.ToOutputDtos().ToList();

  
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var hasNextPage = request.PageNumber < totalPages;
        var hasPreviousPage = request.PageNumber > 1;

     
        return new PagedAuditLogsResult
        {
            Items = dtos,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
            TotalPages = totalPages,
            HasNextPage = hasNextPage,
            HasPreviousPage = hasPreviousPage
        };
    }
}
