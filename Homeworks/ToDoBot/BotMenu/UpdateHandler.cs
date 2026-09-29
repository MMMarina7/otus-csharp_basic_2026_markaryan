using Otus.ToDoList.ConsoleBot;
using Otus.ToDoList.ConsoleBot.Types;

public class UpdateHandler : IUpdateHandler
{
    private readonly IUserService _userService;
    private readonly IToDoService _toDoService;

    public UpdateHandler(IUserService userService, IToDoService toDoService)
    {
        _userService = userService;
        _toDoService = toDoService;
    }

    public void HandleUpdateAsync(
        ITelegramBotClient botClient,
        Update update)
    {
        try
        {
            string command = update.Message.Text.Trim();
            long telegramUserId = update.Message.From.Id;

            ToDoUser? user = _userService.GetUser(telegramUserId);

            switch (command.ToLowerInvariant())
            {
                case "/start":
                    HandleStart(botClient, update, user);
                    return;

                case "/help":
                    HandleHelp(botClient, update, user);
                    return;

                case "/info":
                    HandleInfo(botClient, update);
                    return;
            }

            if (user == null)
            {
                botClient.SendMessage(
                    update.Message.Chat,
                    "Сначала зарегистрируйтесь с помощью команды /start.");

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

                ToDoItem task = _toDoService.Add(user, taskName);

                botClient.SendMessage(
                    update.Message.Chat,
                    $"Задача \"{task.Name}\" добавлена. Id: {task.Id}");

                return;
            }

            if (command.Equals(
                "/showtasks",
                StringComparison.OrdinalIgnoreCase))
            {
                HandleShowTasks(botClient, update, user);
                return;
            }

            if (command.Equals(
                "/showalltasks",
                StringComparison.OrdinalIgnoreCase))
            {
                HandleShowAllTasks(botClient, update, user);
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
                HandleCompleteTask(botClient, update, command);
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
                HandleRemoveTask(botClient, update, command);
                return;
            }

            botClient.SendMessage(update.Message.Chat, "Неизвестная команда. Используйте /help для получения справки.");
        }
        catch (Exception exception)
        {
            botClient.SendMessage(update.Message.Chat, exception.Message);
        }
    }

    private void HandleStart(ITelegramBotClient botClient, Update update, ToDoUser? user)
    {
        if (user == null)
        {
            long telegramUserId = update.Message.From.Id;

            string telegramUserName =
                update.Message.From.Username ?? $"User_{telegramUserId}";

            user = _userService.RegisterUser(telegramUserId, telegramUserName);

            botClient.SendMessage(update.Message.Chat, $"Привет, {user.TelegramUserName}! Вы зарегистрированы.");

            return;
        }

        botClient.SendMessage(update.Message.Chat, $"{user.TelegramUserName}, вы уже зарегистрированы.");
    }

    private void HandleHelp(ITelegramBotClient botClient, Update update, ToDoUser? user)
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
                "/addtask <название> - Добавить задачу",
                "/showtasks - Показать активные задачи",
                "/showalltasks - Показать все задачи",
                "/completetask <Id> - Завершить задачу",
                "/removetask <Id> - Удалить задачу");
        }

        botClient.SendMessage(update.Message.Chat, message);
    }

    private void HandleInfo(
        ITelegramBotClient botClient,
        Update update)
    {
        botClient.SendMessage(update.Message.Chat, "Версия программы: 1.0. Дата создания: 18.07.2026.");
    }

    private void HandleShowTasks(ITelegramBotClient botClient, Update update, ToDoUser user)
    {
        IReadOnlyList<ToDoItem> tasks = _toDoService.GetActiveByUserId(user.UserId);

        if (!tasks.Any())
        {
            botClient.SendMessage(update.Message.Chat, "Список активных задач пуст.");

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

        botClient.SendMessage(update.Message.Chat, message);
    }

    private void HandleShowAllTasks(ITelegramBotClient botClient, Update update, ToDoUser user)
    {
        IReadOnlyList<ToDoItem> tasks = _toDoService.GetAllByUserId(user.UserId);

        if (!tasks.Any())
        {
            botClient.SendMessage(
                update.Message.Chat,
                "Список задач пуст.");

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

        botClient.SendMessage(update.Message.Chat, message);
    }

    private void HandleCompleteTask(ITelegramBotClient botClient, Update update, string command)
    {
        const string completeTaskCommand = "/completetask";

        string idText = command[completeTaskCommand.Length..].Trim();

        if (!Guid.TryParse(idText, out Guid taskId))
        {
            throw new ArgumentException("Неверный формат идентификатора задачи.");
        }

        _toDoService.MarkCompleted(taskId);

        botClient.SendMessage(update.Message.Chat, "Задача отмечена как выполненная.");
    }

    private void HandleRemoveTask(ITelegramBotClient botClient, Update update, string command)
    {
        const string removeTaskCommand = "/removetask";

        string idText = command[removeTaskCommand.Length..].Trim();

        if (!Guid.TryParse(idText, out Guid taskId))
        {
            throw new ArgumentException("Неверный формат идентификатора задачи.");
        }

        _toDoService.Delete(taskId);

        botClient.SendMessage(update.Message.Chat, "Задача удалена.");
    }
}