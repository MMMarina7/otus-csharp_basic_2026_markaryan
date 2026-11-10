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

    public async Task<IReadOnlyList<ToDoItem>> GetAllByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _toDoRepository.GetAllByUserIdAsync(userId, ct);
    }

    public async Task<IReadOnlyList<ToDoItem>> GetActiveByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _toDoRepository.GetActiveByUserIdAsync(userId, ct);
    }

    public async Task<ToDoItem> AddAsync(ToDoUser user, string name, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(user);

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Название задачи не может быть пустым.",
                nameof(name));
        }

        name = name.Trim();

        int activeTaskCount =
            await _toDoRepository.CountActiveAsync(
                user.UserId,
                ct);

        if (activeTaskCount >= _maxTaskCount)
        {
            throw new TaskCountLimitException(
                _maxTaskCount);
        }

        if (name.Length > _maxTaskLength)
        {
            throw new TaskLengthLimitException(
                name.Length,
                _maxTaskLength);
        }

        bool taskExists =
            await _toDoRepository.ExistsByNameAsync(
                user.UserId,
                name,
                ct);

        if (taskExists)
        {
            throw new DuplicateTaskException(name);
        }

        ToDoItem task = new ToDoItem(name, user);

        await _toDoRepository.AddAsync(task, ct);

        return task;
    }

    public async Task MarkCompletedAsync(Guid id, CancellationToken ct)
    {
        ToDoItem? task =
            await _toDoRepository.GetAsync(id, ct);

        if (task == null)
        {
            throw new ArgumentException(
                $"Задача с идентификатором {id} не найдена.",
                nameof(id));
        }

        task.State = ToDoItemState.Completed;
        task.StateChangedAt = DateTime.UtcNow;

        await _toDoRepository.UpdateAsync(task, ct);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        ToDoItem? task = await _toDoRepository.GetAsync(id, ct);

        if (task == null)
        {
            throw new ArgumentException(
                $"Задача с идентификатором {id} не найдена.",
                nameof(id));
        }

        await _toDoRepository.DeleteAsync(id, ct);
    }

    public async Task<IReadOnlyList<ToDoItem>> FindAsync(ToDoUser user, string namePrefix, CancellationToken ct)
{
    ArgumentNullException.ThrowIfNull(user);

    if (string.IsNullOrWhiteSpace(namePrefix))
    {
        throw new ArgumentException(
            "Начало названия задачи не может быть пустым.",
            nameof(namePrefix));
    }

    namePrefix = namePrefix.Trim();

    return await _toDoRepository.FindAsync(
        user.UserId,
        task => task.Name.StartsWith(
            namePrefix,
            StringComparison.OrdinalIgnoreCase),
        ct);
}
}