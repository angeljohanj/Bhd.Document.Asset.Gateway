using Gateway.Application.Interfaces;
using Gateway.Infrastructure.BackgroundWorkers;
using Gateway.Infrastructure.ExternalServices;
using Gateway.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gateway.Infrastructure.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=gateway.db";

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<IDocumentRepository, DocumentRepository>();

        services.AddScoped<IDocumentPublisher, DocumentPublisher>();

        services.AddSingleton<IUploadJobQueue, UploadJobQueue>();

        services.AddHostedService<DocumentUploadWorker>();

        return services;
    }
}
