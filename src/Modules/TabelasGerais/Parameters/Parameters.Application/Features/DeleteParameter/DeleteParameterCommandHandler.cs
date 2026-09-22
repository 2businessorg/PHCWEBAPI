using MediatR;
using Parameters.Domain.Repositories;

namespace Parameters.Application.Features.DeleteParameter;

public class DeleteParameterCommandHandler : IRequestHandler<DeleteParameterCommand, bool>
{
    private readonly IPara1Repository _para1Repository;

    public DeleteParameterCommandHandler(IPara1Repository para1Repository)
    {
        _para1Repository = para1Repository;
    }

    public async Task<bool> Handle(DeleteParameterCommand request, CancellationToken cancellationToken)
    {
        // Get Para1
        var para1 = await _para1Repository.GetByStampAsync(request.Para1Stamp, cancellationToken);
        if (para1 == null)
            return false;

        // Delete Para1
        await _para1Repository.DeleteAsync(para1, cancellationToken);

        return true;
    }
}
