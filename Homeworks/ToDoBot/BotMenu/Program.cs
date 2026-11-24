using System.Text.Json;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

try
{
    string settingsFilePath = Path.Combine(
        Environment.GetFolderPath(
            Environment.SpecialFolder.UserProfile),
        "Otus",
        "Secrets",
        "ToDoBot",
        "settings.json");

    if (!File.Exists(settingsFilePath))
    {
        throw new FileNotFoundException(
            $"Не найден файл настроек: {settingsFilePath}");
    }

    string settingsJson =
        File.ReadAllText(settingsFilePath);

    BotConfiguration configuration =
        JsonSerializer.Deserialize<BotConfiguration>(
            settingsJson)
        ?? throw new InvalidOperationException(
            "Не удалось прочитать настройки бота.");

    if (string.IsNullOrWhiteSpace(configuration.BotToken))
    {
        throw new InvalidOperationException(
            "В файле настроек не указан BotToken.");
    }

    int maxTaskCount = ReadLimit("Введите максимально допустимое количество задач:");

    int maxTaskLength = ReadLimit("Введите максимально допустимую длину задачи:");

    IUserRepository userRepository = new InMemoryUserRepository();

    IUserService userService = new UserService(userRepository);

    IToDoRepository toDoRepository = new InMemoryToDoRepository();

    IToDoService toDoService = new ToDoService(toDoRepository, maxTaskCount, maxTaskLength);

    IToDoReportService toDoReportService = new ToDoReportService(toDoRepository);

    IUpdateHandler updateHandler = new UpdateHandler(userService, toDoService, toDoReportService);

    TelegramBotClient botClient = new TelegramBotClient(configuration.BotToken);

    using CancellationTokenSource cts = new CancellationTokenSource();

    ReceiverOptions receiverOptions = new()
    {
        AllowedUpdates = [UpdateType.Message],
        DropPendingUpdates = true
    };

    BotCommand[] botCommands =
    [
        new() { Command = "start", Description = "Начать работу" },
        new() { Command = "help", Description = "Показать справку" },
        new() { Command = "info", Description = "Информация о программе" },
        new() { Command = "addtask", Description = "Добавить задачу" },
        new() { Command = "showtasks", Description = "Показать активные задачи" },
        new() { Command = "showalltasks", Description = "Показать все задачи" },
        new() { Command = "completetask", Description = "Завершить задачу" },
        new() { Command = "removetask", Description = "Удалить задачу" },
        new() { Command = "find", Description = "Найти задачи" },
        new() { Command = "report", Description = "Показать статистику" },
        new() { Command = "hidekeyboard", Description = "Скрыть клавиатуру" }
    ];

    await botClient.SetMyCommands(
        botCommands,
        cancellationToken: cts.Token);

    botClient.StartReceiving(
        updateHandler,
        receiverOptions,
        cts.Token);

    var me = await botClient.GetMe(cancellationToken: cts.Token);

    Console.WriteLine($"{me.FirstName} запущен!");
    Console.WriteLine("Нажмите клавишу A для выхода.");

    while (!cts.IsCancellationRequested)
    {
        ConsoleKeyInfo key = Console.ReadKey(
            intercept: true);

        if (key.Key == ConsoleKey.A)
        {
            cts.Cancel();
            break;
        }

        var botInfo = await botClient.GetMe(
            cancellationToken: cts.Token);

        Console.WriteLine(
            $"Бот: {botInfo.FirstName}; " +
            $"Username: @{botInfo.Username}; " +
            $"Id: {botInfo.Id}");

        Console.WriteLine(
            "Нажмите клавишу A для выхода.");
    }
}
catch (Exception exception)
{
    Console.WriteLine($"Произошла непредвиденная ошибка: {exception.Message}");
}

static int ReadLimit(string message)
{
    while (true)
    {
        Console.WriteLine(message);

        string? input = Console.ReadLine();

        if (int.TryParse(input, out int value) && value >= 1 && value <= 100)
        {
            return value;
        }

        Console.WriteLine("Введите целое число от 1 до 100.");
    }
}
