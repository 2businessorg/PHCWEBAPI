using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Recruitment.Application.Features.EnqueueAnalysis;
using Recruitment.Application.Options;
using Recruitment.Application.Services;
using Recruitment.Domain.Repositories;
using Recruitment.Application.Scoring;
using Recruitment.Infrastructure.Notifications;
using Recruitment.Infrastructure.Options;
using Recruitment.Infrastructure.Persistence;
using Recruitment.Infrastructure.Repositories;
using Recruitment.Infrastructure.Scoring;

namespace Recruitment.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRecruitmentInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RecruitmentIaOptions>(configuration.GetSection(RecruitmentIaOptions.SectionName));
        services.Configure<RecruitmentSchemaOptions>(
            configuration.GetSection(RecruitmentSchemaOptions.SectionName));
        services.Configure<RecruitmentLocalAiOptions>(
            configuration.GetSection(RecruitmentLocalAiOptions.SectionName));

        services.AddSingleton<IRecruitmentSqlConnectionFactory, RecruitmentSqlConnectionFactory>();
        services.AddScoped<IRctVacancyRepository, RctVacancyRepository>();
        services.AddScoped<IRctCriteriaRepository, RctCriteriaRepository>();
        services.AddScoped<IRctIntervenienteRepository, RctIntervenienteRepository>();
        services.AddScoped<ICvAnexoRepository, CvAnexoRepository>();
        services.AddScoped<ICveEstadoIaRepository, CveEstadoIaRepository>();
        services.AddScoped<ISrtScoreRepository, SrtScoreRepository>();
        services.AddScoped<IRecruitmentOutboxRepository, RecruitmentOutboxRepository>();
        services.AddScoped<IPhcAvisoService, PhcAvisoServiceStub>();
        services.AddScoped<IRecruitmentJobScheduler, HangfireRecruitmentJobScheduler>();
        services.AddScoped<IRecruitmentLlmClient, LocalChatRecruitmentLlmClient>();

        return services;
    }
}
