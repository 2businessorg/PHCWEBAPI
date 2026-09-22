using MediatR;
using Parameters.Application.DTOs.Parameters;
using Parameters.Application.Mappings;
using Parameters.Domain.Repositories;
using System.Diagnostics;

namespace Parameters.Application.Features.GetAllParameters;

public class GetAllParametersQueryHandler : IRequestHandler<GetAllParametersQuery, IEnumerable<ParameterOutputDTO>>
{
    private readonly IPara1Repository _para1Repository;

    public GetAllParametersQueryHandler(IPara1Repository para1Repository)
    {
        _para1Repository = para1Repository;
    }

    public async Task<IEnumerable<ParameterOutputDTO>> Handle(GetAllParametersQuery request, CancellationToken cancellationToken)
    {
        Debug.Print("HANDLING GET ALL PARAMETERS QUERY");
        var para1List = await _para1Repository.GetAllAsync(request.IncludeInactive, cancellationToken);

        return para1List.ToDtos<ParameterOutputDTO>();
    }
}

