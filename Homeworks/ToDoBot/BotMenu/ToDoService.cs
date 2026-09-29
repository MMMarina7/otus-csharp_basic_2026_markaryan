public class ToDoService : IToDoService
{
    private readonly List<ToDoItem> _tasks = new();
    private readonly int _maxTaskCount;
    private readonly int _maxTaskLength;

    public ToDoService(int maxTaskCount, int maxTaskLength)
    {
        _maxTaskCount = maxTaskCount;
        _maxTaskLength = maxTaskLength;
    }

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

    public ToDoItem Add(ToDoUser user, string name)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Название задачи не может быть пустым.",
                nameof(name));
        }

        name = name.Trim();

        IReadOnlyList<ToDoItem> userTasks = GetAllByUserId(user.UserId);

        if (userTasks.Count >= _maxTaskCount)
        {
            throw new TaskCountLimitException(_maxTaskCount);
        }

        if (name.Length > _maxTaskLength)
        {
            throw new TaskLengthLimitException(name.Length, _maxTaskLength);
        }

        bool taskExists = userTasks.Any(task =>
            task.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        if (taskExists)
        {
            throw new DuplicateTaskException(name);
        }

        ToDoItem task = new ToDoItem(name, user);
        _tasks.Add(task);

        return task;
    }

    public void MarkCompleted(Guid id)
    {
        ToDoItem? task = _tasks.FirstOrDefault(task => task.Id == id);

        if (task == null)
        {
            throw new ArgumentException(
                $"Задача с идентификатором {id} не найдена.",
                nameof(id));
        }

        task.State = ToDoItemState.Completed;
        task.StateChangedAt = DateTime.UtcNow;
    }

    public void Delete(Guid id)
    {
        ToDoItem? task = _tasks.FirstOrDefault(task => task.Id == id);

        if (task == null)
        {
            throw new ArgumentException(
                $"Задача с идентификатором {id} не найдена.",
                nameof(id));
        }

        _tasks.Remove(task);
    }
}