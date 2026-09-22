using Agent.Application.Abstractions;
using MediatR;

namespace Agent.Application.Features.SendAgentChat;

/// <summary>
/// Delegates the chat turn to the agent host.
/// </summary>
public class SendAgentChatCommandHandler : IRequestHandler<SendAgentChatCommand, AgentChatResult>
{
    private readonly ITreasuryAgent _agent;

    public SendAgentChatCommandHandler(ITreasuryAgent agent)
    {
        _agent = agent;
    }

    public Task<AgentChatResult> Handle(SendAgentChatCommand request, CancellationToken cancellationToken)
        => _agent.ChatAsync(request.Message, cancellationToken);
}
