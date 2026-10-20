using MinimalApiOrg.Endpoints;

namespace MinimalApiOrg.Extensions;


public static class EndpointExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapHealthEndpoints("/");
        app.MapCategoryEndpoints("/api/categories");
        app.MapTodoEndpoints("/api/todos");
        return app;
    }
}
