using FluentValidation;
using Invoices.Application.DTOs;
using Invoices.Application.ExternalServices;
using Invoices.Application.Features.CreateFatura;
using Invoices.Domain.Repositories;
using Invoices.Infrastructure.ExternalServices;
using Invoices.Infrastructure.Persistence;
using Invoices.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Kernel.MultiTenancy;
using Shared.Kernel.UserFields;
using System;

namespace Invoices.Presentation
{
    /// <summary>
    /// Extensões de injeção de dependências para o módulo de Faturas
    /// </summary>
    public static class DIInvoices
    {
        /// <summary>
        /// Registra todos os serviços do módulo de Faturas no contentor de DI
        /// </summary>
        public static IServiceCollection AddInvoicesPresentation(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // ===================================
            // 1. DbContext
            // ===================================
            var connectionString = configuration.GetConnectionString("DBconnect");
            services.AddDbContext<FaturasDbContext>((sp, options) =>
            {
                options.UseSqlServer(connectionString);
            });

            // ===================================
            // 2. Repositories
            // ===================================
            services.AddScoped<IFaturaRepository, FaturaRepository>();
            services.AddScoped<ITDRepository, TDRepositoryEFCore>();
            services.AddScoped<IClientRepository, ClientRepository>();
            services.AddScoped<IStockRepository, StockRepository>();

            // ===================================
            // 3. MediatR
            // ===================================
            services.AddMediatR(config => 
            {
                config.RegisterServicesFromAssembly(typeof(Invoices.Application.Features.CreateFatura.CreateFaturaCommand).Assembly);
            });

            // ===================================
            // 4. Validators
            // ===================================
            // Register concrete validators first (needed as dependencies)
            services.AddScoped<Invoices.Application.Features.CreateFatura.CreateInvoiceLineInputDTOValidator>();
            services.AddScoped<Invoices.Application.Features.CreateFatura.CreateInvoiceInputDTOValidator>();
            
            // Register validators by interface
            services.AddScoped<IValidator<CreateInvoiceLineInputDTO>>(sp => 
                sp.GetRequiredService<Invoices.Application.Features.CreateFatura.CreateInvoiceLineInputDTOValidator>());
            services.AddScoped<IValidator<CreateInvoiceInputDTO>>(sp => 
                sp.GetRequiredService<Invoices.Application.Features.CreateFatura.CreateInvoiceInputDTOValidator>());
            services.AddScoped<IValidator<CreateFaturaCommand>, Invoices.Application.Features.CreateFatura.CreateFaturaCommandValidator>();

            // ===================================
            // 5. External Services
            // ===================================
            services.AddScoped<IFtService, FtService>();
            services.AddScoped<Invoices.Domain.ExternalServices.IPhcWebService, Invoices.Infrastructure.ExternalServices.PhcWebService>();

            // IUserFieldService e ITenantContext são registados globalmente (Auth/Host),
            // mas injectados como opcionais no handler — não precisam de registo adicional aqui.

            return services;
        }
    }
}
