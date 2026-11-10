public class InMemoryToDoRepository : IToDoRepository
{
    private readonly List<ToDoItem> _tasks = new();

public Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
{
    ct.ThrowIfCancellationRequested();

    IReadOnlyList<ToDoItem> tasks = _tasks
        .Where(task => task.User.UserId == userId)
        .ToList();

    return Task.FromResult(tasks);
}

    public Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        IReadOnlyList<ToDoItem> tasks = _tasks
            .Where(task =>
                task.User.UserId == userId &&
                task.State == ToDoItemState.Active)
            .ToList();

        return Task.FromResult(tasks);
    }

    public Task<ToDoItem?> GetAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        ToDoItem? task = _tasks.FirstOrDefault(task => task.Id == id);

        return Task.FromResult(task);
    }

    public Task AddAsync(ToDoItem item, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _tasks.Add(item);

        return Task.CompletedTask;
    }

    public Task UpdateAsync(ToDoItem item, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        int index = _tasks.FindIndex(task => task.Id == item.Id);

        if (index >= 0)
        {
            _tasks[index] = item;
        }

        return Task.CompletedTask;
    }

    public Task DeleteAsync(Guid id, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        ToDoItem? task = _tasks.FirstOrDefault(task => task.Id == id);

        if (task != null)
        {
            _tasks.Remove(task);
        }

        return Task.CompletedTask;
    }

    public Task<bool> ExistsByNameAsync(Guid userId, string name, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        bool exists = _tasks.Any(task =>
            task.User.UserId == userId &&
            task.Name.Equals(
                name,
                StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(exists);
    }

    public Task<int> CountActiveAsync(Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        int count = _tasks.Count(task =>
            task.User.UserId == userId &&
            task.State == ToDoItemState.Active);

        return Task.FromResult(count);
    }

    public Task<IReadOnlyList<ToDoItem>> FindAsync(Guid userId, Func<ToDoItem, bool> predicate, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        IReadOnlyList<ToDoItem> tasks = _tasks
            .Where(task =>
                task.User.UserId == userId &&
                predicate(task))
            .ToList();

        return Task.FromResult(tasks);
    }
}