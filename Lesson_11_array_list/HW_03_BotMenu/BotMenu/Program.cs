Console.WriteLine("Добро пожаловать. Эта программа поддерживает следующие команды:");
ShowCommands();
string? userName = null;
bool isRunning = true;
List<string> tasks = new List<string>();

while (isRunning)
{
    Console.WriteLine("Введите команду: ");
    string command = Console.ReadLine()?.Trim() ?? "";

    if (string.IsNullOrWhiteSpace(command)) //(command == "")
    {
        Console.WriteLine("Команда не может быть пустой. Пожалуйста, попробуйте снова.");
        continue;
    }

    if (command.StartsWith("/echo", StringComparison.OrdinalIgnoreCase))
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            Console.WriteLine("Команда /echo станет доступна после команды /start.");
            continue;
        }
        string echoText = command["/echo".Length..].Trim();
        if (string.IsNullOrWhiteSpace(echoText))
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

            if (string.IsNullOrWhiteSpace(userName))
            {
                Console.WriteLine("Имя не может быть пустым. Пожалуйста, попробуйте снова.");
                continue;
            }
            Console.WriteLine($"Привет, {userName}! " + 
                    "Теперь вам доступна команда /echo. " +
                    "Чем могу помочь?");
            break;
        case "/help":
            //Console.WriteLine(GetGreeting(userName) + "Доступные команды:");
            Console.WriteLine(string.IsNullOrWhiteSpace(userName) ? "Доступные команды:" : $"{userName}, доступные команды:");
            ShowCommands();
            if (!(string.IsNullOrWhiteSpace(userName))) //(userName is not null)
            {
                Console.WriteLine("/echo <текст> — Повтор введённого текста");
            }
            break;
        case "/info":
            Console.WriteLine(string.IsNullOrWhiteSpace(userName) ? "Версия программы: 1.0. Дата создания: 18.07.2026." : $"{userName}, версия программы: 1.0. Дата создания: 18.07.2026.");
            break;
         case "/addtask":
            Console.Write("Пожалуйста, введите описание задачи: ");
            string taskDescription = Console.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(taskDescription)) //(command == "")
            {
                Console.WriteLine("Описание задачи не может быть пустым. Пожалуйста, попробуйте снова.");
                continue;
            }
            AddTask(taskDescription, tasks);
            break;
         case "/showtasks":
            ShowTasks(tasks);
            break;
        case "/removetask":
            RemoveTask(tasks);
            break;
        case "/exit":
            isRunning = false;
            Console.WriteLine(string.IsNullOrWhiteSpace(userName) ? "До свидания!" : $"{userName}, до свидания!");
            break;
        default:
            Console.WriteLine("Неизвестная команда. Используйте /help для получения справки.");
            break;
    }
}

static void RemoveTask(List<string> tasks)
{
    if (tasks.Count == 0)
    {
        Console.WriteLine("Список задач пуст. Нет задач для удаления.");
        return;
    }

    Console.WriteLine("Вот ваш список задач:");
    ShowTasks(tasks);
    Console.Write("Пожалуйста, введите номер задачи для удаления: ");
    if (int.TryParse(Console.ReadLine()?.Trim(), out int taskIndex) && taskIndex >= 1 && taskIndex <= tasks.Count)
    {               
        string removedTask = tasks[taskIndex - 1];
        tasks.RemoveAt(taskIndex - 1);
        Console.WriteLine($"Задача \"{removedTask}\" удалена.");
    }
    else
    {
        Console.WriteLine("Неверный номер задачи.");
    }
}

static void ShowTasks(List<string> tasks)
{
    if (tasks.Count == 0)
    {
        Console.WriteLine("Список задач пуст.");
        return;
    }

    for (int i = 0; i < tasks.Count; i++)
    {
        Console.WriteLine($"{i + 1}. {tasks[i]}");
    }
}

static void AddTask(string taskDescription, List<string> tasks)
{
    tasks.Add(taskDescription);
    Console.WriteLine($"Задача \"{taskDescription}\" добавлена.");
}

static void ShowCommands()
{
    Console.WriteLine("/start - Начало работы");
    Console.WriteLine("/help - Помощь");
    Console.WriteLine("/info - Информация");
    Console.WriteLine("/exit - Выход из программы");
    Console.WriteLine("/addtask - Добавить задачу");
    Console.WriteLine("/showtasks - Показать все задачи");      
    Console.WriteLine("/removetask - Удалить задачу");
}

// static string GetGreeting(string? userName)
// {
//     return userName is null ? "" : $"{userName}, ";
// }
