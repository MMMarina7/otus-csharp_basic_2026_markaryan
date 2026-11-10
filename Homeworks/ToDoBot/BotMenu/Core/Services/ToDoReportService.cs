public class ToDoReportService : IToDoReportService
{
    private readonly IToDoRepository _toDoRepository;

    public ToDoReportService(IToDoRepository toDoRepository)
    {
        _toDoRepository = toDoRepository;
    }

    public async Task<(
        int total,
        int completed,
        int active,
        DateTime generatedAt
    )> GetUserStatsAsync(Guid userId, CancellationToken ct)
    {
        IReadOnlyList<ToDoItem> tasks =
            await _toDoRepository
                .GetAllByUserIdAsync(userId, ct);

        int total = tasks.Count;

        int completed = tasks.Count(task =>
            task.State == ToDoItemState.Completed);

        int active = tasks.Count(task =>
            task.State == ToDoItemState.Active);

        DateTime generatedAt = DateTime.UtcNow;

        return (
            total,
            completed,
            active,
            generatedAt);
    }
}