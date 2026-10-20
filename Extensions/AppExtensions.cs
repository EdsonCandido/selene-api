namespace MinimalApiOrg.Extensions;

using Scalar.AspNetCore;

public static class AppExtensions
{
    public static WebApplication UseArchitectures(this WebApplication app)
    {
        app.MapControllers();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        return app;
    }
}
