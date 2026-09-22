using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shared.Abstractions.Privacy.Pseudonymization;
using Shared.Infrastructure.Privacy.Pseudonymization;
using Shared.Infrastructure.Privacy.Pseudonymization.Crypto;
using Shared.Infrastructure.Privacy.Pseudonymization.Detection;
using Shared.Infrastructure.Privacy.Pseudonymization.Detoken;
using Shared.Infrastructure.Privacy.Pseudonymization.Leak;
using Shared.Infrastructure.Privacy.Pseudonymization.Options;
using Shared.Infrastructure.Privacy.Pseudonymization.Pipeline;
using Shared.Infrastructure.Privacy.Pseudonymization.Store;
using Xunit;

namespace Shared.Infrastructure.Tests.Privacy.Pseudonymization;

public class DocumentPseudonymizerTests
{
    [Fact]
    public async Task Roundtrip_PseudonymizeThenDetokenize_RestoresOriginals()
    {
        var sut = CreateSut(requireSidecar: false);
        var session = Guid.NewGuid().ToString("N");
        var doc = "doc-1";
        var plain =
            "Contact joao.silva@example.com or +351 912 345 678. Empresa ExampleEmployer SA. Skills: Java, React, SQL.";

        var result = await sut.PseudonymizeAsync(new PseudonymizationRequest(
            session,
            doc,
            plain,
            PseudonymizationMode.RedactedScoring,
            new PseudonymizationPolicy()));

        result.FailureReason.Should().BeNull();
        result.PseudonymizedText.Should().NotContain("joao.silva@example.com");
        result.PseudonymizedText.Should().Contain("Java");
        result.PseudonymizedText.Should().Contain("React");
        result.PseudonymizedText.Should().Contain("SQL");
        result.Entities.Should().NotBeEmpty();

        // Build a synthetic model response using issued tokens
        var emailToken = result.Entities.First(e => e.EntityType == EntityTypes.EmailAddress).Token;
        var modelResponse = $"Candidate email {emailToken} knows Java.";

        var detok = await sut.DetokenizeAsync(new DetokenizationRequest(session, doc, modelResponse));
        detok.Success.Should().BeTrue();
        detok.FinalText.Should().Contain("joao.silva@example.com");
        detok.FinalText.Should().Contain("Java");
        detok.RejectedTokens.Should().BeEmpty();
    }

    [Fact]
    public async Task LeakChecker_BlocksEgress_WhenPlaintextRemains()
    {
        var store = new InMemoryTokenMapStore();
        var protector = new AesGcmTokenValueProtector(Options.Create(new PseudonymizationOptions()));
        var leak = new IndependentLeakChecker();

        var entry = new TokenMapEntry(
            "s1",
            "d1",
            "{{EMAIL_ADDRESS_abcd1234}}",
            EntityTypes.EmailAddress,
            protector.Protect("secret@example.com"),
            protector.Protect("secret@example.com"),
            protector.ComputeHmac("secret@example.com"),
            0.9f,
            "test",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddHours(1));

        await store.UpsertAsync(entry);

        var check = await leak.CheckAsync(
            "Please email secret@example.com for details",
            new PseudonymizationPolicy(),
            new[] { entry },
            e => protector.Unprotect(e.OriginalValueCipher));

        check.Passed.Should().BeFalse();
        check.Findings.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SkillsAllowlist_PreservesTechnologies()
    {
        var sut = CreateSut(requireSidecar: false);
        var plain = "Strong in .NET, Azure, Docker and Kubernetes at ExampleEmployer SA.";

        var result = await sut.PseudonymizeAsync(new PseudonymizationRequest(
            Guid.NewGuid().ToString("N"),
            "doc-skills",
            plain,
            PseudonymizationMode.RedactedScoring,
            new PseudonymizationPolicy(PreserveProfessionalSkills: true)));

        result.PseudonymizedText.Should().Contain(".NET");
        result.PseudonymizedText.Should().Contain("Azure");
        result.PseudonymizedText.Should().Contain("Docker");
        result.PseudonymizedText.Should().Contain("Kubernetes");
        result.PseudonymizedText.Should().NotContain("ExampleEmployer SA");
    }

    [Fact]
    public async Task CloudEgress_WhenSidecarDown_Refuses_FailClosed()
    {
        var sut = CreateSut(requireSidecar: true, sidecarHealthy: false);
        var result = await sut.PseudonymizeAsync(new PseudonymizationRequest(
            "s",
            "d",
            "email test@example.com",
            PseudonymizationMode.CloudEgress,
            new PseudonymizationPolicy()));

        result.EgressAllowed.Should().BeFalse();
        result.FailureReason.Should().Contain("sidecar");
    }

    [Fact]
    public async Task ModeOff_DoesNotRequireSidecar()
    {
        var sut = CreateSut(requireSidecar: true, sidecarHealthy: false);
        var result = await sut.PseudonymizeAsync(new PseudonymizationRequest(
            "s",
            "d",
            "plain",
            PseudonymizationMode.Off,
            new PseudonymizationPolicy()));

        result.EgressAllowed.Should().BeFalse();
        result.PseudonymizedText.Should().Be("plain");
        result.FailureReason.Should().Be("Mode=Off");
    }

    [Fact]
    public async Task SafeDetoken_RejectsUnknownToken_Strict()
    {
        var sut = CreateSut(requireSidecar: false);
        var session = Guid.NewGuid().ToString("N");
        await sut.PseudonymizeAsync(new PseudonymizationRequest(
            session,
            "d",
            "mail a@b.com",
            PseudonymizationMode.RedactedScoring,
            new PseudonymizationPolicy()));

        var detok = await sut.DetokenizeAsync(new DetokenizationRequest(
            session,
            "d",
            "Injected {{EMPLOYER_deadbeef}} token"));

        detok.Success.Should().BeFalse();
        detok.RejectedTokens.Should().Contain("{{EMPLOYER_deadbeef}}");
    }

    private static DocumentPseudonymizer CreateSut(bool requireSidecar, bool sidecarHealthy = true)
    {
        var options = Options.Create(new PseudonymizationOptions
        {
            RequireSidecarForCloudEgress = requireSidecar,
            EnableCloudLlm = true,
            PreserveProfessionalSkills = true,
            StrictDetoken = true
        });

        var protector = new AesGcmTokenValueProtector(options);
        var store = new InMemoryTokenMapStore();
        var access = new LoggingTokenMapAccessLogger(NullLogger<LoggingTokenMapAccessLogger>.Instance);
        var local = new RegexDictContextEntityDetector();
        var presidio = new StubPresidioClient(sidecarHealthy);
        var detector = new EnsembleEntityDetector(local, presidio, tryPresidio: false);
        var resolver = new SimpleEntityResolver();
        var leak = new IndependentLeakChecker();
        var detoken = new SafeDetokenizer(access);

        IPseudonymizationStep[] steps =
        {
            new NormalizeTextStep(),
            new DetectEntitiesStep(detector),
            new ClassifyAndScoreStep(),
            new ResolveEntitiesStep(resolver),
            new TokenizeAndPersistStep(store, protector),
            new LeakCheckStep(leak, protector)
        };

        return new DocumentPseudonymizer(
            steps,
            detoken,
            store,
            protector,
            presidio,
            options,
            NullLogger<DocumentPseudonymizer>.Instance);
    }

    private sealed class StubPresidioClient : IPresidioAnalyzerClient
    {
        private readonly bool _healthy;

        public StubPresidioClient(bool healthy) => _healthy = healthy;

        public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_healthy);

        public Task<IReadOnlyList<RawEntitySpan>> AnalyzeAsync(
            string text,
            string language,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<RawEntitySpan>>(Array.Empty<RawEntitySpan>());
    }
}
