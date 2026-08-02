Console.WriteLine("Добро пожаловать. Эта программа поддерживает следующие команды:");
ShowCommands();
string? userName = null;
bool isRunning = true;

while (isRunning)
{
    Console.WriteLine("Введите команду: ");
    string command = Console.ReadLine()?.Trim() ?? "";

    if (command == "")
    {
        Console.WriteLine("Команда не может быть пустой. Пожалуйста, попробуйте снова.");
        continue;
    }

    if (command.StartsWith("/echo", StringComparison.OrdinalIgnoreCase))
    {
        if (userName is null)
        {
            Console.WriteLine("Команда /echo станет доступна после команды /start.");
            continue;
        }
        string echoText = command["/echo".Length..].Trim();
        if (echoText == "")
        {
            Console.WriteLine("Текст для повторения не может быть пустым. Пожалуйста, попробуйте снова.");
            continue;
        }
        Console.WriteLine($"{userName}, {echoText}");
        continue;
    }

    switch (command.ToLower())
    {
        case "/start":
            Console.WriteLine("Введите ваше имя:");
            userName = Console.ReadLine()?.Trim() ?? "";

            if (userName == "")
            {
                Console.WriteLine("Имя не может быть пустым. Пожалуйста, попробуйте снова.");
                continue;
            }
            Console.WriteLine($"Приятно познакомиться, {userName}! " +
                    "Теперь вам доступна команда /echo.");
            break;
        case "/help":
            //Console.WriteLine(GetGreeting(userName) + "Доступные команды:");
            Console.WriteLine(userName is null ? "Доступные команды:" : $"{userName}, доступные команды:");
            ShowCommands();
            if (userName is not null)
            {
                Console.WriteLine("/echo <текст> — Повтор введённого текста");
            }
            break;
        case "/info":
            Console.WriteLine(userName is null ? "Версия программы: 1.0. Дата создания: 18.07.2026." : $"{userName}, версия программы: 1.0. Дата создания: 18.07.2026.");
            break;
        case "/exit":
            isRunning = false;
            Console.WriteLine(userName is null ? "До свидания!" : $"{userName}, до свидания!");
            break;
        default:
            Console.WriteLine("Неизвестная команда. Используйте /help для получения справки.");
            break;
    }
}

static void ShowCommands()
{
    Console.WriteLine("/start - Начало работы");
    Console.WriteLine("/help - Помощь");
    Console.WriteLine("/info - Информация");
    Console.WriteLine("/exit - Выход из программы");
}

// static string GetGreeting(string? userName)
// {
//     return userName is null ? "" : $"{userName}, ";
// }
