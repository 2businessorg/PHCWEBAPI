using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Shared.Abstractions.Privacy.Pseudonymization;
using Shared.Infrastructure.Privacy.Pseudonymization.Crypto;
using Shared.Infrastructure.Privacy.Pseudonymization.Detection;
using Shared.Infrastructure.Privacy.Pseudonymization.Detoken;
using Shared.Infrastructure.Privacy.Pseudonymization.Leak;
using Shared.Infrastructure.Privacy.Pseudonymization.Options;
using Shared.Infrastructure.Privacy.Pseudonymization.Pipeline;
using Shared.Infrastructure.Privacy.Pseudonymization.Presidio;
using Shared.Infrastructure.Privacy.Pseudonymization.Store;

namespace Shared.Infrastructure.Privacy.Pseudonymization;

/// <summary>
/// Registers shared privacy pseudonymization (Presidio sidecar client + map + leak + detoken).
/// Ownership: Shared/Platform. Recruitment is first consumer only.
/// </summary>
public static class PseudonymizationDependencyInjection
{
    public static IServiceCollection AddPseudonymization(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<PseudonymizationOptions>(
            configuration.GetSection(PseudonymizationOptions.SectionName));

        services.AddSingleton<ITokenValueProtector, AesGcmTokenValueProtector>();
        services.AddSingleton<ITokenMapAccessLogger, LoggingTokenMapAccessLogger>();
        services.AddSingleton<ITokenMapStore, InMemoryTokenMapStore>();
        services.AddSingleton<ILeakChecker, IndependentLeakChecker>();
        services.AddSingleton<IDetokenizer, SafeDetokenizer>();
        services.AddSingleton<IEntityResolver, SimpleEntityResolver>();
        services.AddSingleton<RegexDictContextEntityDetector>();

        services.AddHttpClient<IPresidioAnalyzerClient, PresidioHttpAnalyzerClient>();

        services.AddSingleton<IEntityDetector>(sp =>
        {
            var local = sp.GetRequiredService<RegexDictContextEntityDetector>();
            var presidio = sp.GetRequiredService<IPresidioAnalyzerClient>();
            return new EnsembleEntityDetector(local, presidio, tryPresidio: true);
        });

        services.AddSingleton<IPseudonymizationStep, NormalizeTextStep>();
        services.AddSingleton<IPseudonymizationStep, DetectEntitiesStep>();
        services.AddSingleton<IPseudonymizationStep, ClassifyAndScoreStep>();
        services.AddSingleton<IPseudonymizationStep, ResolveEntitiesStep>();
        services.AddSingleton<IPseudonymizationStep, TokenizeAndPersistStep>();
        services.AddSingleton<IPseudonymizationStep, LeakCheckStep>();

        services.AddSingleton<IDocumentPseudonymizer, DocumentPseudonymizer>();

        return services;
    }
}
