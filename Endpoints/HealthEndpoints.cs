using Asp.Versioning;
using Microsoft.Extensions.Options;
using MinimalApiOrg.Configurations;

namespace MinimalApiOrg.Endpoints;

public static class HealthEndpoints
{
    public static WebApplication MapHealthEndpoints(this WebApplication app, string path)
    {

        var categories = app
                    .NewVersionedApi("Health");
        var group = categories.MapGroup(path)
        .HasApiVersion(new ApiVersion(1, 0));
        group.MapGet("", (IOptions<AppSettings> options) => Results.Ok(new
        {
            message = "API",
            development = options.Value.ApplicationName,
            environment = app.Environment.EnvironmentName,
            version = options.Value.Version
        }

        ));

        return app;
    }
}
