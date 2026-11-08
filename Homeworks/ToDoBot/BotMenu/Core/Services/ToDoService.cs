public class ToDoService : IToDoService
{
    private readonly IToDoRepository _toDoRepository;
    private readonly int _maxTaskCount;
    private readonly int _maxTaskLength;

    public ToDoService(
        IToDoRepository toDoRepository,
        int maxTaskCount,
        int maxTaskLength)
    {
        _toDoRepository = toDoRepository;
        _maxTaskCount = maxTaskCount;
        _maxTaskLength = maxTaskLength;
    }

    public IReadOnlyList<ToDoItem> GetAllByUserId(Guid userId)
    {
        return _toDoRepository.GetAllByUserId(userId);
    }

    public IReadOnlyList<ToDoItem> GetActiveByUserId(Guid userId)
    {
        return _toDoRepository.GetActiveByUserId(userId);
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

        if (_toDoRepository.CountActive(user.UserId) >= _maxTaskCount)
        {
            throw new TaskCountLimitException(_maxTaskCount);
        }

        if (name.Length > _maxTaskLength)
        {
            throw new TaskLengthLimitException(
                name.Length,
                _maxTaskLength);
        }

        if (_toDoRepository.ExistsByName(user.UserId, name))
        {
            throw new DuplicateTaskException(name);
        }

        ToDoItem task = new ToDoItem(name, user);
        _toDoRepository.Add(task);

        return task;
    }

    public void MarkCompleted(Guid id)
    {
        ToDoItem? task = _toDoRepository.Get(id);

        if (task == null)
        {
            throw new ArgumentException(
                $"Задача с идентификатором {id} не найдена.",
                nameof(id));
        }

        task.State = ToDoItemState.Completed;
        task.StateChangedAt = DateTime.UtcNow;

        _toDoRepository.Update(task);
    }

    public void Delete(Guid id)
    {
        ToDoItem? task = _toDoRepository.Get(id);

        if (task == null)
        {
            throw new ArgumentException(
                $"Задача с идентификатором {id} не найдена.",
                nameof(id));
        }

        _toDoRepository.Delete(id);
    }

    public IReadOnlyList<ToDoItem> Find(ToDoUser user, string namePrefix)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(namePrefix))
        {
            throw new ArgumentException(
                "Начало названия задачи не может быть пустым.",
                nameof(namePrefix));
        }

        namePrefix = namePrefix.Trim();

        return _toDoRepository.Find(
            user.UserId,
            task => task.Name.StartsWith(
                namePrefix,
                StringComparison.OrdinalIgnoreCase));
    }
}