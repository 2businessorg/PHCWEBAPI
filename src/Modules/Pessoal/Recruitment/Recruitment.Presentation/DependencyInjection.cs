using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Recruitment.Application;
using Recruitment.Infrastructure;

namespace Recruitment.Presentation;

public static class DependencyInjection
{
    public static IServiceCollection AddRecruitmentPresentation(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRecruitmentApplication();
        services.AddRecruitmentInfrastructure(configuration);
        return services;
    }
}
