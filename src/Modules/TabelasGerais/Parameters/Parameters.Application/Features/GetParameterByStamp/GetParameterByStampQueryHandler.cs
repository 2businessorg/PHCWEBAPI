using MediatR;
using Parameters.Application.DTOs.Parameters;
using Parameters.Application.Mappings;
using Parameters.Domain.Repositories;

namespace Parameters.Application.Features.GetParameterByStamp;

public class GetParameterByStampQueryHandler : IRequestHandler<GetParameterByStampQuery, ParameterOutputDTO?>
{
    private readonly IPara1Repository _para1Repository;

    public GetParameterByStampQueryHandler(IPara1Repository para1Repository)
    {
        _para1Repository = para1Repository;
    }

    public async Task<ParameterOutputDTO?> Handle(GetParameterByStampQuery request, CancellationToken cancellationToken)
    {
        var para1 = await _para1Repository.GetByStampAsync(request.Para1Stamp, cancellationToken);

        return para1?.ToDto<ParameterOutputDTO>();
    }
}

