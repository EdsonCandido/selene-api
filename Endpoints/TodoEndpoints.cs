using Asp.Versioning;
using MinimalApiOrg.Models;
using MinimalApiOrg.Services;

namespace MinimalApiOrg.Endpoints;


public static class TodoEndpoints
{

    public static WebApplication MapTodoEndpoints(this WebApplication app, string path)
    {
        var todos = app.NewVersionedApi("Todos");

        var v1 = todos.MapGroup(path)
            .HasApiVersion(new ApiVersion(1, 0));

        v1.MapGet("/", GetTodos);
        v1.MapGet("/{id:guid}", GetTodoById);
        v1.MapPost("/", CreateTodo);
        v1.MapPost("/{id:guid}/complete", CompleteTodo);
        v1.MapPut("/{id:guid}", UpdateTodo);
        v1.MapDelete("/{id:guid}", DeleteTodo);

        return app;
    }

    private static IResult GetTodos(TodoStore store) => Results.Ok(store.GetAll());
    private static IResult GetTodoById(Guid id, TodoStore store)
    {
        var todo = store.GetById(id);

        return todo is null ? Results.NotFound() : Results.Ok(todo);
    }
    private static IResult CreateTodo(CreatedTodoRequest request, TodoStore store)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Results.BadRequest(new { error = "Title is required" });
        }

        var todo = store.TodoCreate(request.Title);
        return Results.Created($"/{todo.Id}", todo);
    }
    private static IResult CompleteTodo(Guid id, TodoStore store)
    {
        var todo = store.TodoComplete(id);

        return todo is null ? Results.NotFound() : Results.Ok(todo);
    }
    private static IResult DeleteTodo(Guid id, TodoStore store)
    {
        var todo = store.DeleteTodo(id);

        return !todo  ? Results.NotFound() : Results.Ok(todo);
    }
    private static IResult UpdateTodo(Guid id, UpdatedTodoRequest request, TodoStore store)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Results.BadRequest(new { error = "Title is required." });
        }

        var todo = store.TodoUpdate(id, request.Title, request.IsDone);
        return todo is null ? Results.NotFound() : Results.Ok(todo);
    }
}
