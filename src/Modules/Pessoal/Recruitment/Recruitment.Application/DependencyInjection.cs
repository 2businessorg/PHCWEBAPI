using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Recruitment.Application.Jobs;
using Recruitment.Application.Scoring;
using Recruitment.Application.Privacy;
using Recruitment.Application.Services;

namespace Recruitment.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddRecruitmentApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddSingleton<IRubricEvidenceScorer, RubricEvidenceScorer>();
        services.AddSingleton<RubricCandidateScoreEngine>();
        services.AddScoped<QwenCloudScoreEngine>();
        services.AddScoped<ICandidateScoreEngineSelector, CandidateScoreEngineSelector>();
        services.AddScoped<IRecruitmentEnqueueService, RecruitmentEnqueueService>();
        services.AddScoped<IRecruitmentCloudEgressGuard, RecruitmentCloudEgressGuard>();
        services.AddScoped<AnalyzeCandidateJob>();

        return services;
    }
}

public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults.SelectMany(r => r.Errors).Where(f => f is not null).ToList();
            if (failures.Count > 0)
                throw new ValidationException(failures);
        }

        return await next();
    }
}
