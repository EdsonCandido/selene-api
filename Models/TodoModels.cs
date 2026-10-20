namespace MinimalApiOrg.Models;

public sealed class TodoModels
{
    public Guid Id { get; init; }
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; } = false;
    public DateTime CreatedAd { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed record CreatedTodoRequest(string Title);

public sealed record UpdatedTodoRequest(string Title, bool IsDone);
