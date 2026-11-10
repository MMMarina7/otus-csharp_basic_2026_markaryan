public class InMemoryUserRepository : IUserRepository
{
    private readonly List<ToDoUser> _users = new();

    public Task<ToDoUser?> GetUserAsync(Guid userId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        ToDoUser? user = _users.FirstOrDefault(user => user.UserId == userId);

        return Task.FromResult(user);
    }

    public Task<ToDoUser?> GetUserByTelegramUserIdAsync(long telegramUserId, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        ToDoUser? user = _users.FirstOrDefault(user => user.TelegramUserId == telegramUserId);

        return Task.FromResult(user);
    }

    public Task AddAsync(
        ToDoUser user,
        CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        _users.Add(user);

        return Task.CompletedTask;
    }
}