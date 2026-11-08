using Otus.ToDoList.ConsoleBot;

try
{
    int maxTaskCount = ReadLimit("Введите максимально допустимое количество задач:");

    int maxTaskLength = ReadLimit("Введите максимально допустимую длину задачи:");

    IUserRepository userRepository = new InMemoryUserRepository();

    IUserService userService = new UserService(userRepository);

    IToDoRepository toDoRepository = new InMemoryToDoRepository();

    IToDoService toDoService = new ToDoService(toDoRepository, maxTaskCount, maxTaskLength);

    IToDoReportService toDoReportService = new ToDoReportService(toDoRepository);
    IUpdateHandler updateHandler = new UpdateHandler(userService, toDoService, toDoReportService);

    ITelegramBotClient botClient = new ConsoleBotClient();

    botClient.StartReceiving(updateHandler);
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