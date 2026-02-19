using FluentValidation;
using Gateway.Application.Features;
using Gateway.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Application.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IDocumentService, DocumentService>();
        
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        
        return services;
    }
}
