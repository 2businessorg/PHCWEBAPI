using Microsoft.Extensions.DependencyInjection;

namespace Parameters.Presentation.GraphQL;

/// <summary>
/// Extension methods para configurar GraphQL API do módulo Parameters
/// </summary>
public static class GraphQLDependencyInjection
{
    public static IServiceCollection AddParametersGraphQL(this IServiceCollection services)
    {
        // TODO: Implementar quando adicionar HotChocolate
        // services
        //     .AddGraphQLServer()
        //     .AddQueryType<ParametersQueries>()
        //     .AddMutationType<ParametersMutations>()
        //     .AddType<ParameterType>();
        
        return services;
    }
}
