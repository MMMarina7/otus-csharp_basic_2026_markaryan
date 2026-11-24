using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using Telegram.Bot.Types.Enums;

public class UpdateHandler : IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;
    private readonly IToDoReportService _toDoReportService;

    public UpdateHandler(
        IUserService userService,
        IToDoService toDoService,
        IToDoReportService toDoReportService)
    {
        _userService = userService;
        _toDoService = toDoService;
        _toDoReportService = toDoReportService;
    }

    public async Task HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update,
        CancellationToken ct)
    {
        Message? message = update.Message;

        if (message?.Text == null || message.From == null)
        {
            return;
        }

        try
        {
            string command = message.Text.Trim();
            long telegramUserId = message.From.Id;
            ToDoUser? user = await _userService.GetUserAsync(telegramUserId, ct);

            switch (command.ToLowerInvariant())
            {
                case "/start":
                    await HandleStartAsync(botClient, message, user, ct);
                    return;
                case "/help":
                    await HandleHelpAsync(botClient, message, user, ct);
                    return;
                case "/info":
                    await HandleInfoAsync(botClient, message, ct);
                    return;
                case "/hidekeyboard":
                    await HandleHideKeyboardAsync(botClient, message, ct);
                    return;
            }

            if (user == null)
            {
                await botClient.SendMessage(
                    message.Chat,
                    "Сначала зарегистрируйтесь с помощью команды /start.",
                    replyMarkup: CreateStartKeyboard(),
                    cancellationToken: ct);

                return;
            }

            const string findCommand = "/find";
            if (command.Equals(findCommand, StringComparison.OrdinalIgnoreCase) ||
                command.StartsWith(findCommand + " ", StringComparison.OrdinalIgnoreCase))
            {
                await HandleFindAsync(botClient, message, user, command, ct);
                return;
            }

            if (command.Equals("/report", StringComparison.OrdinalIgnoreCase))
            {
                await HandleReportAsync(botClient, message, user, ct);
                return;
            }

            const string addTaskCommand = "/addtask";
            if (command.Equals(addTaskCommand, StringComparison.OrdinalIgnoreCase) ||
                command.StartsWith(addTaskCommand + " ", StringComparison.OrdinalIgnoreCase))
            {
                string taskName = command[addTaskCommand.Length..].Trim();
                ToDoItem task = await _toDoService.AddAsync(user, taskName, ct);

                await botClient.SendMessage(
                    message.Chat,
                    $"Задача \"{task.Name}\" добавлена. Id: {task.Id}",
                    cancellationToken: ct);
                return;
            }

            if (command.Equals("/showtasks", StringComparison.OrdinalIgnoreCase))
            {
                await HandleShowTasksAsync(botClient, message, user, ct);
                return;
            }

            if (command.Equals("/showalltasks", StringComparison.OrdinalIgnoreCase))
            {
                await HandleShowAllTasksAsync(botClient, message, user, ct);
                return;
            }

            const string completeTaskCommand = "/completetask";
            if (command.Equals(completeTaskCommand, StringComparison.OrdinalIgnoreCase) ||
                command.StartsWith(completeTaskCommand + " ", StringComparison.OrdinalIgnoreCase))
            {
                await HandleCompleteTaskAsync(botClient, message, command, ct);
                return;
            }

            const string removeTaskCommand = "/removetask";
            if (command.Equals(removeTaskCommand, StringComparison.OrdinalIgnoreCase) ||
                command.StartsWith(removeTaskCommand + " ", StringComparison.OrdinalIgnoreCase))
            {
                await HandleRemoveTaskAsync(botClient, message, command, ct);
                return;
            }

            await botClient.SendMessage(
                message.Chat,
                "Неизвестная команда. Используйте /help для получения справки.",
                cancellationToken: ct);
        }
        catch (Exception exception)
        {
            await botClient.SendMessage(
                message.Chat,
                exception.Message,
                cancellationToken: ct);
        }
    }

    public Task HandleErrorAsync(
        ITelegramBotClient botClient,
        Exception exception,
        HandleErrorSource source,
        CancellationToken ct)
    {
        Console.WriteLine($"Ошибка Telegram ({source}): {exception.Message}");
        return Task.CompletedTask;
    }

    private async Task HandleStartAsync(
        ITelegramBotClient botClient,
        Message message,
        ToDoUser? user,
        CancellationToken ct)
    {
        if (user == null)
        {
            long telegramUserId = message.From!.Id;
            string telegramUserName = message.From.Username ?? $"User_{telegramUserId}";
            user = await _userService.RegisterUserAsync(telegramUserId, telegramUserName, ct);

           await botClient.SendMessage(
                message.Chat,
                $"Привет, {user.TelegramUserName}! Вы зарегистрированы.",
                replyMarkup: CreateMainKeyboard(),
                cancellationToken: ct);
            return;
        }

        await botClient.SendMessage(
            message.Chat,
            $"{user.TelegramUserName}, вы уже зарегистрированы.",
            replyMarkup: CreateMainKeyboard(),
            cancellationToken: ct);
    }

    private async Task HandleHelpAsync(
        ITelegramBotClient botClient,
        Message message,
        ToDoUser? user,
        CancellationToken ct)
    {
        string responseText;

        if (user == null)
        {
            responseText = string.Join(
                Environment.NewLine,
                "/start - Начало работы",
                "/help - Помощь",
                "/info - Информация");
        }
        else
        {
            responseText = string.Join(
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

        ReplyKeyboardMarkup keyboard = user == null
            ? CreateStartKeyboard()
            : CreateMainKeyboard();

        await botClient.SendMessage(
            message.Chat,
            responseText,
            replyMarkup: keyboard,
            cancellationToken: ct);
    }

    private async Task HandleInfoAsync(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken ct)
    {
        await botClient.SendMessage(
            message.Chat,
            "Версия программы: 1.0. Дата создания: 18.07.2026.",
            cancellationToken: ct);
    }

    private async Task HandleShowTasksAsync(
        ITelegramBotClient botClient,
        Message message,
        ToDoUser user,
        CancellationToken ct)
    {
        IReadOnlyList<ToDoItem> tasks = await _toDoService.GetActiveByUserIdAsync(user.UserId, ct);

        if (!tasks.Any())
        {
            await botClient.SendMessage(
                message.Chat,
                "Список активных задач пуст.",
                cancellationToken: ct);
            return;
        }

        string responseText = "Активные задачи:";
        foreach (ToDoItem task in tasks)
        {
            responseText +=
                $"\n{task.Name} — " +
                $"{task.CreatedAt:dd.MM.yyyy HH:mm:ss} — " +
                $"`{task.Id}`";
        }

        await botClient.SendMessage(
            message.Chat,
            responseText,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleShowAllTasksAsync(
        ITelegramBotClient botClient,
        Message message,
        ToDoUser user,
        CancellationToken ct)
    {
        IReadOnlyList<ToDoItem> tasks = await _toDoService.GetAllByUserIdAsync(user.UserId, ct);

        if (!tasks.Any())
        {
            await botClient.SendMessage(
                message.Chat,
                "Список задач пуст.",
                cancellationToken: ct);
            return;
        }

        string responseText = "Все задачи:";
        foreach (ToDoItem task in tasks)
        {
            string state = task.State == ToDoItemState.Active ? "Активна" : "Выполнена";
            responseText +=
                $"\n{task.Name} — " +
                $"{state} — " +
                $"{task.CreatedAt:dd.MM.yyyy HH:mm:ss} — " +
                $"`{task.Id}`";
        }

        await botClient.SendMessage(
            message.Chat,
            responseText,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleCompleteTaskAsync(
        ITelegramBotClient botClient,
        Message message,
        string command,
        CancellationToken ct)
    {
        const string completeTaskCommand = "/completetask";
        string idText = command[completeTaskCommand.Length..].Trim();

        if (!Guid.TryParse(idText, out Guid taskId))
        {
            throw new ArgumentException("Неверный формат идентификатора задачи.");
        }

        await _toDoService.MarkCompletedAsync(taskId, ct);
        await botClient.SendMessage(
            message.Chat,
            "Задача отмечена как выполненная.",
            cancellationToken: ct);
    }

    private async Task HandleRemoveTaskAsync(
        ITelegramBotClient botClient,
        Message message,
        string command,
        CancellationToken ct)
    {
        const string removeTaskCommand = "/removetask";
        string idText = command[removeTaskCommand.Length..].Trim();

        if (!Guid.TryParse(idText, out Guid taskId))
        {
            throw new ArgumentException("Неверный формат идентификатора задачи.");
        }

        await _toDoService.DeleteAsync(taskId, ct);
        await botClient.SendMessage(
            message.Chat,
            "Задача удалена.",
            cancellationToken: ct);
    }

    private async Task HandleFindAsync(
        ITelegramBotClient botClient,
        Message message,
        ToDoUser user,
        string command,
        CancellationToken ct)
    {
        const string findCommand = "/find";
        string namePrefix = command[findCommand.Length..].Trim();
        IReadOnlyList<ToDoItem> tasks = await _toDoService.FindAsync(user, namePrefix, ct);

        if (!tasks.Any())
        {
            await botClient.SendMessage(
                message.Chat,
                "Подходящие задачи не найдены.",
                cancellationToken: ct);
            return;
        }

        string responseText = "Найденные задачи:";
        foreach (ToDoItem task in tasks)
        {
            responseText +=
                $"\n{task.Name} — " +
                $"{task.CreatedAt:dd.MM.yyyy HH:mm:ss} — " +
                $"`{task.Id}`";
        }

        await botClient.SendMessage(
            message.Chat,
            responseText,
            parseMode: ParseMode.Markdown,
            cancellationToken: ct);
    }

    private async Task HandleReportAsync(
        ITelegramBotClient botClient,
        Message message,
        ToDoUser user,
        CancellationToken ct)
    {
        (
            int total,
            int completed,
            int active,
            DateTime generatedAt
        ) stats = await _toDoReportService.GetUserStatsAsync(user.UserId, ct);

        string responseText =
            "Статистика по задачам на " +
            $"{stats.generatedAt:dd.MM.yyyy HH:mm:ss}. " +
            $"Всего: {stats.total}; " +
            $"Завершенных: {stats.completed}; " +
            $"Активных: {stats.active};";

        await botClient.SendMessage(
            message.Chat,
            responseText,
            cancellationToken: ct);
    }

    private async Task HandleHideKeyboardAsync(
        ITelegramBotClient botClient,
        Message message,
        CancellationToken ct)
    {
        await botClient.SendMessage(
            message.Chat,
            "Клавиатура скрыта. Чтобы вернуть её, используйте /start.",
            replyMarkup: new ReplyKeyboardRemove(),
            cancellationToken: ct);
    }

    private static ReplyKeyboardMarkup CreateStartKeyboard()
    {
        return new ReplyKeyboardMarkup(
            new[]
            {
                new[]
                {
                    new KeyboardButton("/start")
                }
            })
        {
            ResizeKeyboard = true
        };
    }

    private static ReplyKeyboardMarkup CreateMainKeyboard()
    {
        return new ReplyKeyboardMarkup(
            new[]
            {
                new[]
                {
                    new KeyboardButton("/showalltasks")
                },
                new[]
                {
                    new KeyboardButton("/showtasks"),
                    new KeyboardButton("/report")
                }
            })
        {
            ResizeKeyboard = true
        };
    }
}
