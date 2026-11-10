public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<ToDoUser> RegisterUserAsync(long telegramUserId, string telegramUserName, CancellationToken ct)
    {
        ToDoUser user = new ToDoUser(telegramUserId, telegramUserName);

        await _userRepository.AddAsync(user, ct);

        return user;
    }

    public async Task<ToDoUser?> GetUserAsync(long telegramUserId, CancellationToken ct)
    {
        return await _userRepository.GetUserByTelegramUserIdAsync(telegramUserId, ct);
    }
}