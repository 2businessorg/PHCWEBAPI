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
    public async Task CloudEgress_AnonymizesEmailAndMzPhone_LeakCheckPasses()
    {
        var sut = CreateSut(requireSidecar: true, sidecarHealthy: true);
        var plain = "Contacto maria.lab@example.co.mz ou +258 84 123 4567. Skills: Java, SQL.";

        var result = await sut.PseudonymizeAsync(new PseudonymizationRequest(
            "s-leak-pass",
            "d-leak-pass",
            plain,
            PseudonymizationMode.CloudEgress,
            new PseudonymizationPolicy()));

        result.EgressAllowed.Should().BeTrue();
        result.FailureReason.Should().BeNull();
        result.LeakCheck.Passed.Should().BeTrue();
        result.LeakCheck.Findings.Should().BeEmpty();
        result.PseudonymizedText.Should().NotContain("maria.lab@example.co.mz");
        result.PseudonymizedText.Should().NotContain("+258");
        result.PseudonymizedText.Should().NotContain("84 123 4567");
        result.PseudonymizedText.Should().Contain("Java");
        result.PseudonymizedText.Should().Contain("SQL");
    }

    [Fact]
    public async Task CloudEgress_OverlappingPresidioSpan_StillRedactsEmail()
    {
        var sut = CreateSut(
            requireSidecar: true,
            sidecarHealthy: true,
            tryPresidio: true,
            presidioSpans: text =>
            {
                const string email = "maria.lab@example.co.mz";
                var at = text.IndexOf(email, StringComparison.Ordinal);
                var start = Math.Max(0, at - 3);
                var end = Math.Min(text.Length, start + 6);
                return
                [
                    new RawEntitySpan(
                        EntityTypes.Person,
                        text.Substring(start, end - start),
                        start,
                        end,
                        0.8f,
                        "presidio.overlap")
                ];
            });

        var result = await sut.PseudonymizeAsync(new PseudonymizationRequest(
            "s-overlap",
            "d-overlap",
            "Contacto maria.lab@example.co.mz ou +258 84 123 4567.",
            PseudonymizationMode.CloudEgress,
            new PseudonymizationPolicy()));

        result.EgressAllowed.Should().BeTrue();
        result.LeakCheck.Passed.Should().BeTrue();
        result.PseudonymizedText.Should().NotContain("example.co.mz");
        result.PseudonymizedText.Should().NotContain("@");
    }

    [Fact]
    public async Task CloudEgress_OcrDuplicateAndShortName_ScrubsResidualBeforeLeakCheck()
    {
        var sut = CreateSut(
            requireSidecar: true,
            sidecarHealthy: true,
            tryPresidio: true,
            presidioSpans: text =>
            {
                var at = text.IndexOf("Ana", StringComparison.Ordinal);
                return
                [
                    new RawEntitySpan(EntityTypes.Person, "Ana", at, at + 3, 0.9f, "presidio.person")
                ];
            });

        var plain =
            "Ana CV. Mail ana.lab@example.co.mz tel +258 84 123 4567. " +
            "OCR copia Ana outra vez ana.lab@example.co.mz e +258  84  123  4567.";

        var result = await sut.PseudonymizeAsync(new PseudonymizationRequest(
            "s-ocr",
            "d-ocr",
            plain,
            PseudonymizationMode.CloudEgress,
            new PseudonymizationPolicy()));

        result.EgressAllowed.Should().BeTrue();
        result.LeakCheck.Passed.Should().BeTrue(string.Join(",", result.LeakCheck.Findings.Select(f => f.Kind)));
        result.PseudonymizedText.Should().NotContain("ana.lab@example.co.mz");
        result.PseudonymizedText.Should().NotContain("+258");
        result.PseudonymizedText.Should().NotContain("84 123 4567");
        result.LeakCheck.Findings.Should().NotContain(f => f.Kind == "session.plaintext_residual");
        result.LeakCheck.Findings.Should().NotContain(f => f.Kind == "regex.email");
        result.LeakCheck.Findings.Should().NotContain(f => f.Kind == "regex.phone");
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

    private static DocumentPseudonymizer CreateSut(
        bool requireSidecar,
        bool sidecarHealthy = true,
        bool tryPresidio = false,
        Func<string, IReadOnlyList<RawEntitySpan>>? presidioSpans = null)
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
        var presidio = new StubPresidioClient(sidecarHealthy, presidioSpans);
        var detector = new EnsembleEntityDetector(local, presidio, tryPresidio);
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
        private readonly Func<string, IReadOnlyList<RawEntitySpan>>? _spans;

        public StubPresidioClient(bool healthy, Func<string, IReadOnlyList<RawEntitySpan>>? spans = null)
        {
            _healthy = healthy;
            _spans = spans;
        }

        public Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(_healthy);

        public Task<IReadOnlyList<RawEntitySpan>> AnalyzeAsync(
            string text,
            string language,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(_spans?.Invoke(text) ?? (IReadOnlyList<RawEntitySpan>)Array.Empty<RawEntitySpan>());
    }
}
