using Agent.Application.Abstractions;
using MediatR;

namespace Agent.Application.Features.SendAgentChat;

/// <summary>
/// POC command that runs the local treasury agent against a natural-language prompt.
/// </summary>
public record SendAgentChatCommand(string Message) : IRequest<AgentChatResult>;
