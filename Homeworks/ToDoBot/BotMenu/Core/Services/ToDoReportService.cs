public class ToDoReportService : IToDoReportService
{
    private readonly IToDoRepository _toDoRepository;

    public ToDoReportService(IToDoRepository toDoRepository)
    {
        _toDoRepository = toDoRepository;
    }

    public (int total,  int completed, int active, DateTime generatedAt) GetUserStats(Guid userId)
    {
        IReadOnlyList<ToDoItem> tasks =
            _toDoRepository.GetAllByUserId(userId);

        int total = tasks.Count;

        int completed = tasks.Count(task =>
            task.State == ToDoItemState.Completed);

        int active = tasks.Count(task =>
            task.State == ToDoItemState.Active);

        DateTime generatedAt = DateTime.UtcNow;

        return (total, completed, active, generatedAt);
    }
}