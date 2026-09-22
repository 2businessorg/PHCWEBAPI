using FluentValidation;

namespace Agent.Application.Features.SendAgentChat;

/// <summary>
/// Validates <see cref="SendAgentChatCommand"/>.
/// </summary>
public class SendAgentChatCommandValidator : AbstractValidator<SendAgentChatCommand>
{
    public SendAgentChatCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty()
            .MaximumLength(4000);
    }
}
