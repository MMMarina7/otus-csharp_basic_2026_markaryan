using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

public class UpdateHandler : IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private readonly IToDoReportService _toDoReportService;

    public UpdateHandler(IUserService userService, IToDoService toDoService, IToDoReportService toDoReportService)
    {
        _userService = userService;
        _toDoService = toDoService;
        _toDoReportService = toDoReportService;
    }

    public async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        try
        {
            string command = update.Message.Text.Trim();
            long telegramUserId = update.Message.From.Id;

            ToDoUser? user = await _userService.GetUserAsync(telegramUserId, ct);

            switch (command.ToLowerInvariant())
            {
                case "/start":
                    await HandleStartAsync(botClient, update, user, ct);
                    return;

                case "/help":
                    await HandleHelpAsync(botClient, update, user, ct);
                    return;

                case "/info":
                    await HandleInfoAsync(botClient, update, ct);
                    return;
            }

            if (user == null)
            {
                await botClient.SendMessage(
                update.Message.Chat,
                "Сначала зарегистрируйтесь с помощью команды /start.",
                ct);

                return;
            }

            const string findCommand = "/find";

            if (command.Equals(
                    findCommand,
                    StringComparison.OrdinalIgnoreCase) ||
                command.StartsWith(
                    findCommand + " ",
                    StringComparison.OrdinalIgnoreCase))
            {
                await HandleFindAsync(botClient, update, user, command, ct);
                return;
            }

            if (command.Equals(
                "/report",
                StringComparison.OrdinalIgnoreCase))
            {
                await HandleReportAsync(botClient, update, user, ct);
                return;
            }

            const string addTaskCommand = "/addtask";

            if (command.Equals(
                    addTaskCommand,
                    StringComparison.OrdinalIgnoreCase) ||
                command.StartsWith(
                    addTaskCommand + " ",
                    StringComparison.OrdinalIgnoreCase))
            {
                string taskName = command[addTaskCommand.Length..].Trim();

                ToDoItem task = await _toDoService.AddAsync(user, taskName, ct);

                await botClient.SendMessage(
                update.Message.Chat,
                $"Задача \"{task.Name}\" добавлена. Id: {task.Id}",
                ct);

                return;
            }

            if (command.Equals(
                "/showtasks",
                StringComparison.OrdinalIgnoreCase))
            {
                await HandleShowTasksAsync(botClient, update, user, ct);

                return;
            }

            if (command.Equals(
                "/showalltasks",
                StringComparison.OrdinalIgnoreCase))
            {
                await HandleShowAllTasksAsync(botClient, update, user, ct);

                return;
            }

            const string completeTaskCommand = "/completetask";

            if (command.Equals(
                    completeTaskCommand,
                    StringComparison.OrdinalIgnoreCase) ||
                command.StartsWith(
                    completeTaskCommand + " ",
                    StringComparison.OrdinalIgnoreCase))
            {
                await HandleCompleteTaskAsync(botClient, update, command, ct);
                return;
            }

            const string removeTaskCommand = "/removetask";

            if (command.Equals(
                    removeTaskCommand,
                    StringComparison.OrdinalIgnoreCase) ||
                command.StartsWith(
                    removeTaskCommand + " ",
                    StringComparison.OrdinalIgnoreCase))
            {
                await HandleRemoveTaskAsync(botClient, update, command, ct);
                return;
            }

            await botClient.SendMessage(
            update.Message.Chat,
            "Неизвестная команда. Используйте /help для получения справки.",
            ct);
        }
        catch (Exception exception)
        {
            await botClient.SendMessage(update.Message.Chat, exception.Message, ct);
        }
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken ct)
    {
        Console.WriteLine($"Ошибка при обработке сообщения: {exception.Message}");

        return Task.CompletedTask;
    }

    private async Task HandleStartAsync(ITelegramBotClient botClient, Update update, ToDoUser? user, CancellationToken ct)
    {
        if (user == null)
        {
            long telegramUserId = update.Message.From.Id;

            string telegramUserName =
                update.Message.From.Username ?? $"User_{telegramUserId}";

            user = await _userService.RegisterUserAsync(telegramUserId, telegramUserName, ct);

            await botClient.SendMessage(
                update.Message.Chat,
                $"Привет, {user.TelegramUserName}! Вы зарегистрированы.",
                ct);

            return;
        }

        await botClient.SendMessage(
            update.Message.Chat,
            $"{user.TelegramUserName}, вы уже зарегистрированы.",
            ct);
    }   

    private async Task HandleHelpAsync(ITelegramBotClient botClient, Update update, ToDoUser? user, CancellationToken ct)
    {
        string message;

        if (user == null)
        {
            message = string.Join(
                Environment.NewLine,
                "/start - Начало работы",
                "/help - Помощь",
                "/info - Информация");
        }
        else
        {
            message = string.Join(
                Environment.NewLine,
                "/help - Помощь",
                "/info - Информация",
                "/find <начало названия> - Найти задачи",
                "/report - Показать статистику по задачам",
                "/addtask <название> - Добавить задачу",
                "/showtasks - Показать активные задачи",
                "/showalltasks - Показать все задачи",
                "/completetask <Id> - Завершить задачу",
                "/removetask <Id> - Удалить задачу");
        }

        await botClient.SendMessage(update.Message.Chat, message, ct);
    }

   private async Task HandleInfoAsync(ITelegramBotClient botClient, Update update, CancellationToken ct)
    {
        await botClient.SendMessage(update.Message.Chat, "Версия программы: 1.0. Дата создания: 18.07.2026.", ct);
    }

    private async Task HandleShowTasksAsync(ITelegramBotClient botClient, Update update, ToDoUser user, CancellationToken ct)
    {
        IReadOnlyList<ToDoItem> tasks = await _toDoService.GetActiveByUserIdAsync(user.UserId, ct);

        if (!tasks.Any())
        {
            await botClient.SendMessage(update.Message.Chat, "Список активных задач пуст.", ct);

            return;
        }

        string message = "Активные задачи:";

        foreach (ToDoItem task in tasks)
        {
            message +=
                $"\n{task.Name} — " +
                $"{task.CreatedAt:dd.MM.yyyy HH:mm:ss} — " +
                $"{task.Id}";
        }

        await botClient.SendMessage(update.Message.Chat, message, ct);
    }

    private async Task HandleShowAllTasksAsync(ITelegramBotClient botClient, Update update, ToDoUser user, CancellationToken ct)
    {
        IReadOnlyList<ToDoItem> tasks = await _toDoService.GetAllByUserIdAsync(user.UserId, ct);

        if (!tasks.Any())
        {
            await botClient.SendMessage(
            update.Message.Chat,
            "Список задач пуст.",
            ct);

            return;
        }

        string message = "Все задачи:";

        foreach (ToDoItem task in tasks)
        {
            string state;

            if (task.State == ToDoItemState.Active)
            {
                state = "Активна";
            }
            else
            {
                state = "Выполнена";
            }

            message +=
                $"\n{task.Name} — " +
                $"{state} — " +
                $"{task.CreatedAt:dd.MM.yyyy HH:mm:ss} — " +
                $"{task.Id}";
        }

        await botClient.SendMessage(update.Message.Chat, message, ct);
    }

    private async Task HandleCompleteTaskAsync(ITelegramBotClient botClient, Update update, string command, CancellationToken ct)
    {
        const string completeTaskCommand = "/completetask";

        string idText = command[completeTaskCommand.Length..].Trim();

        if (!Guid.TryParse(idText, out Guid taskId))
        {
            throw new ArgumentException("Неверный формат идентификатора задачи.");
        }

        await _toDoService.MarkCompletedAsync(taskId, ct);

        await botClient.SendMessage(update.Message.Chat, "Задача отмечена как выполненная.", ct);
    }

    private async Task HandleRemoveTaskAsync(ITelegramBotClient botClient, Update update, string command, CancellationToken ct)
    {
        const string removeTaskCommand = "/removetask";

        string idText = command[removeTaskCommand.Length..].Trim();

        if (!Guid.TryParse(idText, out Guid taskId))
        {
            throw new ArgumentException("Неверный формат идентификатора задачи.");
        }

        await _toDoService.DeleteAsync(taskId, ct);

        await botClient.SendMessage(update.Message.Chat, "Задача удалена.", ct);
    }

    private async Task HandleFindAsync(ITelegramBotClient botClient, Update update, ToDoUser user, string command, CancellationToken ct)
    {
        const string findCommand = "/find";

        string namePrefix = command[findCommand.Length..].Trim();

        IReadOnlyList<ToDoItem> tasks = await _toDoService.FindAsync(user, namePrefix, ct);

        if (!tasks.Any())
        {
            await botClient.SendMessage(update.Message.Chat, "Подходящие задачи не найдены.", ct);

            return;
        }

        string message = "Найденные задачи:";

        foreach (ToDoItem task in tasks)
        {
            message +=
                $"\n{task.Name} — " +
                $"{task.CreatedAt:dd.MM.yyyy HH:mm:ss} — " +
                $"{task.Id}";
        }

        await botClient.SendMessage(update.Message.Chat, message, ct);
    }

    private async Task HandleReportAsync(ITelegramBotClient botClient, Update update, ToDoUser user, CancellationToken ct)
    {
        (
            int total,
            int completed,
            int active,
            DateTime generatedAt
        ) stats = await _toDoReportService.GetUserStatsAsync(user.UserId, ct);

        string message =
            $"Статистика по задачам на " +
            $"{stats.generatedAt:dd.MM.yyyy HH:mm:ss}. " +
            $"Всего: {stats.total}; " +
            $"Завершенных: {stats.completed}; " +
            $"Активных: {stats.active};";

        await botClient.SendMessage(update.Message.Chat, message, ct);
    }
}