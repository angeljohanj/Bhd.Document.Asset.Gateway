using FluentValidation.AspNetCore;
using Gateway.Application.DependencyInjection;
using Gateway.Infrastructure.DependencyInjection;
using Gateway.Infrastructure.Persistence;
using Gateway.Api.Middleware;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;

namespace Gateway.Api;

public partial class Program { 


    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Directory.CreateDirectory("data");

        // Add services to the container.
        builder.Services.AddControllers()
            .AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Gateway.Application.DTOs.DocumentUploadRequestDto>())
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
            {
                Title = "BHD Document Asset Gateway",
                Version = "1.0.0",
                Description = "API for managing document uploads and searching metadata."
            });

            c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
            {
                Description = "API Key needed to access the endpoints. X-Api-Key: My_API_Key",
                In = ParameterLocation.Header,
                Name = "X-Api-Key",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "ApiKeyScheme"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "ApiKey"
                        },
                        In = ParameterLocation.Header
                    },
                    new List<string>()
                }
            });
        });

        builder.Services.AddApplication();
        builder.Services.AddInfrastructure(builder.Configuration);

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        
        app.UseMiddleware<ApiKeyMiddleware>();

        app.MapControllers();

        app.Run();
    }
}




