using Asp.Versioning;

namespace MinimalApiOrg.Endpoints;

public static class CategoryEndpoints
{
    public static WebApplication MapCategoryEndpoints(this WebApplication app, string path)
    {

        var categories = app
                    .NewVersionedApi("Categories");

        var v1 = categories.MapGroup(path)
                   .HasApiVersion(new ApiVersion(1, 0));

        var v2 = categories.MapGroup(path)
            .HasApiVersion(new ApiVersion(2, 0));
        v1.MapGet("/", GetCategoriesV1);
        v2.MapGet("/", GetCategoriesV2);

        return app;
    }

    private static IResult GetCategoriesV1()
    {
        return Results.Ok(new
        {
            version = "v1",
            categories = new[] { "Eletrônicos", "Livros" }
        });
    }

    private static IResult GetCategoriesV2()
    {
        return Results.Ok(new
        {
            version = "v2",
            categories = new[]
            {
                    new { id = 1, name = "Eletrônicos" },
                    new { id = 2, name = "Livros" }
                }
        });
    }

}
