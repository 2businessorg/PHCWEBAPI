using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Recruitment.Application.Features.EnqueueAnalysis;
using Recruitment.Application.Options;
using Recruitment.Application.Services;
using Recruitment.Domain.Repositories;
using Recruitment.Infrastructure.Notifications;
using Recruitment.Infrastructure.Options;
using Recruitment.Infrastructure.Persistence;
using Recruitment.Infrastructure.Repositories;

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

        return services;
    }
}
