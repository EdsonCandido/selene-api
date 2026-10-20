using MinimalApiOrg.Configurations;
using Asp.Versioning;
using MinimalApiOrg.Services;

namespace MinimalApiOrg.Extensions;

public static class BuilderExtensions
{
    public static WebApplicationBuilder AddArchitectures(this WebApplicationBuilder builder)
    {
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();


        return builder;
    }

    public static WebApplicationBuilder AddServices(this WebApplicationBuilder builder)
    {
        builder.Services
            .Configure<AppSettings>(
                builder.Configuration.GetSection("AppSettings"));

        builder.Services
            .AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ReportApiVersions = true;
                options.AssumeDefaultVersionWhenUnspecified = true;

                options.ApiVersionReader =
                    new HeaderApiVersionReader("api-version");
            });

        builder.Services.AddSingleton<TodoStore>();

        return builder;
    }
}
