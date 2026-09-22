using MediatR;
using Parameters.Domain.Repositories;
using Parameters.Application.Mappings;
using Parameters.Application.DTOs;
using Parameters.Application.DTOs.Parameters;

namespace Parameters.Application.Features.UpdateParameter;

public class UpdateParameterCommandHandler : IRequestHandler<UpdateParameterCommand, ParameterOutputDTO>
{
    private readonly IPara1Repository _para1Repository;

    public UpdateParameterCommandHandler(IPara1Repository para1Repository)
    {
        _para1Repository = para1Repository;
    }

    public async Task<ParameterOutputDTO> Handle(UpdateParameterCommand request, CancellationToken cancellationToken)
    {
        var para1 = await _para1Repository.GetByStampAsync(request.Para1Stamp, cancellationToken)
            ?? throw new KeyNotFoundException($"Parameter with stamp {request.Para1Stamp} not found");

        para1.UpdateEntity(request.Dto, request.AtualizadoPor);

        await _para1Repository.UpdateAsync(para1, cancellationToken);

        return para1.ToDto<ParameterOutputDTO>();
    }
}
