using System.Collections.Concurrent;
using MinimalApiOrg.Models;

namespace MinimalApiOrg.Services;

public class TodoStore
{
    private readonly ConcurrentDictionary<Guid, TodoModels> _todos = new();

    public IReadOnlyCollection<TodoModels> GetAll() =>
    _todos.Values
    .OrderBy(t => t.CreatedAd)
    .ToList();

    public TodoModels? GetById(Guid id) =>
        _todos.TryGetValue(id, out var todo) ? todo : null;

    public TodoModels TodoCreate(string title)
    {
        var payload = new TodoModels
        {
            Id = Guid.NewGuid(),
            Title = title.Trim(),
        };

        _todos.TryAdd(payload.Id, payload);

        return payload;
    }

    public TodoModels? TodoUpdate(Guid id, string title, bool isDone)
    {

        if (!_todos.TryGetValue(id, out var todo))
        {
            return null;
        }

        todo.Title = title.Trim();
        todo.IsDone = isDone;
        todo.UpdatedAt = DateTime.UtcNow;

        return todo;
    }

    public TodoModels? TodoComplete(Guid id)
    {
        if (!_todos.TryGetValue(id, out var todo))
        {
            return null;
        }

        todo.IsDone = true;

        return todo;
    }

    public bool DeleteTodo(Guid id)
    {
        return _todos.TryRemove(id, out _);
    }

}
