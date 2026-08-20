
int maxTaskCount;   // Максимальное количество задач
int maxTaskLength; // Максимальная длина задачи

while (true)
{
    try
    {
        Console.WriteLine("Введите максимально допустимое количество задач:");
        maxTaskCount = ParseAndValidateInt(Console.ReadLine(), 1, 100);
        break;
    }
    catch (ArgumentException exception)
    {
        Console.WriteLine(exception.Message);
    }
    catch (Exception exception)
    {
        PrintUnexpectedException(exception);
    }
}

while (true)
{
    try
    {
        Console.WriteLine("Введите максимально допустимую длину задачи:");
        maxTaskLength = ParseAndValidateInt(Console.ReadLine(), 1, 100);
        break;
    }
    catch (ArgumentException exception)
    {
        Console.WriteLine(exception.Message);
    }
    catch (Exception exception)
    {
        PrintUnexpectedException(exception);
    }
}

Console.WriteLine("Добро пожаловать. Эта программа поддерживает следующие команды:");
ShowCommands();
string? userName = null;
bool isRunning = true;
List<string> tasks = new List<string>();

while (isRunning)
{
    try 
    {
        Console.WriteLine("Введите команду: ");
        string command = Console.ReadLine()?.Trim() ?? "";

        ValidateString(command);

        if (command.StartsWith("/echo", StringComparison.OrdinalIgnoreCase))
        {
        HandleEchoCommand(command, userName);
            continue;
        }

        switch (command.ToLower())
        {
            case "/start":
                userName = HandleStartCommand();
                break;
            case "/help":
                HandleHelpCommand(userName);
                break;
            case "/info":
                HandleInfoCommand(userName);
                break;
            case "/addtask":
                AddTask(tasks, maxTaskCount, maxTaskLength);
                break;
            case "/showtasks":
                ShowTasks(tasks);
                break;
            case "/removetask":
                RemoveTask(tasks);
                break;
            case "/exit":
                HandleExitCommand(userName, ref isRunning);
                break;
            default:
                Console.WriteLine("Неизвестная команда. Используйте /help для получения справки.");
                break;
        }
    }
    catch (ArgumentException exception)
    {
        Console.WriteLine(exception.Message);
    }
    catch (TaskCountLimitException exception)
    {
        Console.WriteLine(exception.Message);
    }
    catch (TaskLengthLimitException exception)
    {
        Console.WriteLine(exception.Message);
    }
    catch (DuplicateTaskException exception)
    {
        Console.WriteLine(exception.Message);
    }
    catch (Exception exception)
    {
        PrintUnexpectedException(exception);
    }
}

static void HandleEchoCommand(string command, string? userName)
{
    if (string.IsNullOrWhiteSpace(userName))
        {
            Console.WriteLine("Команда /echo станет доступна после команды /start.");
            return;
        }
        string echoText = command["/echo".Length..].Trim();
        ValidateString(echoText);
        
        Console.WriteLine($"{userName}, {echoText}");
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

static void ValidateString(string? command)
{
    if (string.IsNullOrWhiteSpace(command)) 
    {
        throw new ArgumentException("Строка не может быть пустой или состоять только из пробелов.");
    }
}

static string HandleStartCommand()
{
    Console.WriteLine("Введите ваше имя:");
    string? enteredUserName = Console.ReadLine()?.Trim() ?? "";

    ValidateString(enteredUserName);

    enteredUserName = enteredUserName!.Trim();
    
    string greetingMessage = $"Привет, {enteredUserName}! " + 
            "Теперь вам доступна команда /echo. " +
            "Чем могу помочь?";
    Console.WriteLine(greetingMessage);

    return enteredUserName;
}

static void HandleHelpCommand(string? userName)
{
    Console.WriteLine(string.IsNullOrWhiteSpace(userName) ? "Доступные команды:" : $"{userName}, доступные команды:");
    ShowCommands();
    if (!(string.IsNullOrWhiteSpace(userName))) 
    {
        Console.WriteLine("/echo <текст> — Повтор введённого текста");
    }
}

static void HandleInfoCommand(string? userName)
{
     string information = "Версия программы: 1.0. " + "Дата создания: 18.07.2026.";
     Console.WriteLine(string.IsNullOrWhiteSpace(userName) ? information : $"{userName}, {information}");
            
}

static void AddTask(List<string> tasks, int maxTaskCount, int maxTaskLength)
{
    if (tasks.Count >= maxTaskCount)
    {
        throw new TaskCountLimitException(maxTaskCount);
    }

    Console.Write("Пожалуйста, введите описание задачи: ");
    string? taskDescription = Console.ReadLine();

    ValidateString(taskDescription);

    taskDescription = taskDescription!.Trim();
    
    if (taskDescription.Length > maxTaskLength)
    {
        throw new TaskLengthLimitException(taskDescription.Length, maxTaskLength);
    }
    if (tasks.Contains(taskDescription))
    {
        throw new DuplicateTaskException(taskDescription);
    }

    tasks.Add(taskDescription);
    Console.WriteLine($"Задача \"{taskDescription}\" добавлена.");
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

static void HandleExitCommand(string? userName, ref bool isRunning)
{
    string farewellMessage = string.IsNullOrWhiteSpace(userName) ? "До свидания!" : $"{userName}, до свидания!";
    Console.WriteLine(farewellMessage);
    isRunning = false;
}

static int ParseAndValidateInt(string? str, int min, int max)
{
    if (!int.TryParse(str, out int number))
    {
        throw new ArgumentException("Введенное значение должно быть целым числом.");
    }

    if (number < min || number > max)
    {
        throw new ArgumentException($"Число должно находиться в диапазоне от {min} до {max}.");
    }

    return number;
}

static void PrintUnexpectedException(Exception exception)
{
    Console.WriteLine("Произошла непредвиденная ошибка:");
    Console.WriteLine($"Type: {exception.GetType()}");
    Console.WriteLine($"Message: {exception.Message}");
    Console.WriteLine($"StackTrace: {exception.StackTrace}");
    Console.WriteLine($"InnerException: {exception.InnerException}");
}
