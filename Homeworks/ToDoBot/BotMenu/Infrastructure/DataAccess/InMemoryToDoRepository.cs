public class InMemoryToDoRepository : IToDoRepository
{
    private readonly List<ToDoItem> _tasks = new();

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return _tasks
            .Where(task => task.User.UserId == userId)
            .ToList();
    }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return _tasks
            .Where(task =>
                task.User.UserId == userId &&
                task.State == ToDoItemState.Active)
            .ToList();
    }

    public ToDoItem? Get(Guid id)
    {
        return _tasks.FirstOrDefault(task => task.Id == id);
    }

    public void Add(ToDoItem item)
    {
        _tasks.Add(item);
    }

    public void Update(ToDoItem item)
    {
        int index = _tasks.FindIndex(task => task.Id == item.Id);

        if (index >= 0)
        {
            _tasks[index] = item;
        }
    }

    public void Delete(Guid id)
    {
        ToDoItem? task = Get(id);

        if (task != null)
        {
            _tasks.Remove(task);
        }
    }

    public bool ExistsByName(Guid userId, string name)
    {
        return _tasks.Any(task =>
            task.User.UserId == userId &&
            task.Name.Equals(
                name,
                StringComparison.OrdinalIgnoreCase));
    }

    public int CountActive(Guid userId)
    {
        return _tasks.Count(task =>
            task.User.UserId == userId &&
            task.State == ToDoItemState.Active);
    }

    public IReadOnlyList<ToDoItem> Find(Guid userId, Func<ToDoItem, bool> predicate)
    {
        return _tasks.Where(task => task.User.UserId == userId && predicate(task)).ToList();
    }
}