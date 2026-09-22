using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Recruitment.Application.Options;
using Recruitment.Application.Privacy;
using Shared.Abstractions.Privacy.Pseudonymization;
using Xunit;

namespace Recruitment.Application.Tests.Privacy;

public class RecruitmentCloudEgressGuardTests
{
    [Fact]
    public async Task FlagFalse_DoesNotCallPseudonymizer_AndBlocksEgress()
    {
        var guard = new RecruitmentCloudEgressGuard(
            new ThrowingPseudonymizer(),
            Microsoft.Extensions.Options.Options.Create(new RecruitmentIaOptions { EnableCloudLlm = false }),
            NullLogger<RecruitmentCloudEgressGuard>.Instance);

        var prep = await guard.PrepareAsync("s", "d", "secret@x.com text");

        prep.EnableCloudLlm.Should().BeFalse();
        prep.EgressAllowed.Should().BeFalse();
        prep.FailureReason.Should().Contain("EnableCloudLlm=false");
    }

    private sealed class ThrowingPseudonymizer : IDocumentPseudonymizer
    {
        public Task<PseudonymizationResult> PseudonymizeAsync(
            PseudonymizationRequest request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Sidecar must not be required when EnableCloudLlm=false");

        public Task<DetokenizationResult> DetokenizeAsync(
            DetokenizationRequest request,
            CancellationToken cancellationToken = default) =>
            throw new InvalidOperationException("Should not detokenize when flag is false");
    }
}