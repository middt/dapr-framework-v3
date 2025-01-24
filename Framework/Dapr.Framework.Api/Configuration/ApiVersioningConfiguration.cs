using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.AspNetCore.Builder;

namespace Dapr.Framework.Api.Configuration;

public static class ApiVersioningConfiguration
{
    public static IServiceCollection AddCustomApiVersioning(
        this IServiceCollection services,
        string apiTitle = "API")
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        services.AddVersionedApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        // Explicitly register the ApiVersionDescriptionProvider
        services.AddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();

        // Configure Swagger with versioning support
        services.AddSwaggerGen(options =>
        {
            // Add a swagger document for each discovered API version
            var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, new OpenApiInfo()
                {
                    Title = apiTitle,
                    Version = description.ApiVersion.ToString(),
                    Description = description.IsDeprecated
                        ? "This API version has been deprecated."
                        : "API endpoints"
                });
            }
        });

        return services;
    }

    public static IApplicationBuilder UseCustomApiVersioning(
        this IApplicationBuilder app,
        bool useSwagger = true,
        bool useSwaggerUI = true)
    {
        // Use API Versioning
        app.UseApiVersioning();

        // Configure Swagger and SwaggerUI if enabled
        if (useSwagger)
        {
            app.UseSwagger();
        }

        if (useSwaggerUI)
        {
            app.UseSwaggerUI(options =>
            {
                var apiVersionDescriptionProvider = app.ApplicationServices
                    .GetRequiredService<IApiVersionDescriptionProvider>();

                foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant()
                    );
                }
            });
        }

        return app;
    }
}

