using System.Linq;

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
ToDoUser? user = null;
bool isRunning = true;
List<ToDoItem> tasks = new List<ToDoItem>();

while (isRunning)
{
    try 
    {
        Console.WriteLine("Введите команду: ");
        string command = Console.ReadLine()?.Trim() ?? "";

        ValidateString(command);

        if (command.StartsWith("/echo", StringComparison.OrdinalIgnoreCase))
        {
            HandleEchoCommand(command, user);
            continue;
        }

        if (command.StartsWith("/completetask", StringComparison.OrdinalIgnoreCase))
        {
            HandleCompleteTaskCommand(command, tasks);
            continue;
        }

        switch (command.ToLower())
        {
            case "/start":
                user = HandleStartCommand();
                break;
            case "/help":
                HandleHelpCommand(user);
                break;
            case "/info":
                HandleInfoCommand(user);
                break;
            case "/addtask":
                HandleAddTaskCommand(tasks, user, maxTaskCount, maxTaskLength);
                break;
            case "/showtasks":
                HandleShowTasksCommand(tasks);
                break;
            case "/removetask":
                HandleRemoveTaskCommand(tasks);
                break;
            case "/exit":
                HandleExitCommand(user, ref isRunning);
                break;
            case "/showalltasks":
                HandleShowAllTasksCommand(tasks);
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

static void HandleEchoCommand(string command, ToDoUser? user)
{
    if (user == null)
        {
            Console.WriteLine("Команда /echo станет доступна после команды /start.");
            return;
        }
        string echoText = command["/echo".Length..].Trim();
        ValidateString(echoText);
        
        Console.WriteLine($"{user.TelegramUserName}, {echoText}");
}

static void ShowCommands()
{
    Console.WriteLine("/start - Начало работы");
    Console.WriteLine("/help - Помощь");
    Console.WriteLine("/info - Информация");
    Console.WriteLine("/exit - Выход из программы");
    Console.WriteLine("/addtask - Добавить задачу");
    Console.WriteLine("/showtasks - Показать активные задачи");      
    Console.WriteLine("/removetask <Id> - Удалить задачу");
    Console.WriteLine("/completetask <Id> - Отметить задачу как выполненную");
    Console.WriteLine("/showalltasks - Показать все задачи (включая выполненные)");
}

static void ValidateString(string? command)
{
    if (string.IsNullOrWhiteSpace(command)) 
    {
        throw new ArgumentException("Строка не может быть пустой или состоять только из пробелов.");
    }
}

static ToDoUser HandleStartCommand()
{
    Console.WriteLine("Введите ваше имя:");
    string? telegramUserName = Console.ReadLine()?.Trim() ?? "";

    ValidateString(telegramUserName);

    ToDoUser user = new ToDoUser(telegramUserName);
    
    string greetingMessage = $"Привет, {user.TelegramUserName}! " + 
            "Чем могу помочь?";
    Console.WriteLine(greetingMessage);

    return user;
}

static void HandleHelpCommand(ToDoUser? user)
{
    Console.WriteLine(user == null ? "Доступные команды:" : $"{user.TelegramUserName}, доступные команды:");
    ShowCommands();
    if (user != null) 
    {
        Console.WriteLine("/echo <текст> — Повтор введённого текста");
    }
}

static void HandleInfoCommand(ToDoUser? user)
{
     string information = "Версия программы: 1.0. " + "Дата создания: 18.07.2026.";
     Console.WriteLine(user == null ? information : $"{user.TelegramUserName}, {information}");
            
}

static void HandleAddTaskCommand(List<ToDoItem> tasks, ToDoUser? user, int maxTaskCount, int maxTaskLength)
{
    if (user == null)
    {
        Console.WriteLine("Команда /addtask станет доступна после команды /start.");
        return;
    }

    if (tasks.Count >= maxTaskCount)
    {
        throw new TaskCountLimitException(maxTaskCount);
    }

    Console.Write("Пожалуйста, введите описание задачи: ");
    string? taskName = Console.ReadLine();

    ValidateString(taskName);

    taskName = taskName!.Trim();
    
    if (taskName.Length > maxTaskLength)
    {
        throw new TaskLengthLimitException(taskName.Length, maxTaskLength);
    }

    bool taskExists = tasks.Any(t => t.Name.Equals(taskName, StringComparison.OrdinalIgnoreCase));

    if (taskExists)
    {
        throw new DuplicateTaskException(taskName);
    }

    ToDoItem newTask = new ToDoItem(taskName, user);
    tasks.Add(newTask);
    Console.WriteLine($"Задача \"{taskName}\" добавлена.");
}

static void HandleShowTasksCommand(List<ToDoItem> tasks)
{
    List<ToDoItem> activeTasks = tasks.Where(t => t.State == ToDoItemState.Active).ToList();

    if (!activeTasks.Any())
    {
        Console.WriteLine("Список активных задач пуст.");
        return;
    }

    foreach (ToDoItem task in activeTasks)
    {
        Console.WriteLine($"{task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}");
    }
}

static void HandleRemoveTaskCommand(List<ToDoItem> tasks)
{
    if (!tasks.Any())
    {
        Console.WriteLine("Список задач пуст. Нет задач для удаления.");
        return;
    }

    Console.WriteLine("Вот ваш список задач:");
    HandleShowTasksCommand(tasks);

    Console.Write("Пожалуйста, введите идентификатор задачи для удаления: ");

    string? input = Console.ReadLine();

    if (!Guid.TryParse(input?.Trim(), out Guid taskId))
    {
        Console.WriteLine("Неверный формат идентификатора задачи. Пожалуйста, введите корректный GUID.");
        return;
    } 

    ToDoItem? task = FindTaskById(tasks, taskId);
    
    if (task == null)
    {
        Console.WriteLine($"Задача с идентификатором {taskId} не найдена.");
        return;
    }   

    tasks.Remove(task);
    Console.WriteLine($"Задача \"{task.Name}\" удалена.");
}

static void HandleExitCommand(ToDoUser? user, ref bool isRunning)
{
    string farewellMessage = user == null ? "До свидания!" : $"{user.TelegramUserName}, до свидания!";
    Console.WriteLine(farewellMessage);
    isRunning = false;
}

static void HandleCompleteTaskCommand(string command, List<ToDoItem> tasks)
{
    if (!tasks.Any())
    {
        Console.WriteLine("Список задач пуст. Нет задач для выполнения.");
        return;
    }
    
    string idText = command["/completetask".Length..].Trim();
    if (!Guid.TryParse(idText, out Guid taskId))
    {
        Console.WriteLine("Неверный формат идентификатора задачи. Пожалуйста, введите корректный GUID.");
        return;
    }   

    ToDoItem? task = FindTaskById(tasks, taskId);

    if (task == null)
    {
        Console.WriteLine($"Задача с идентификатором {taskId} не найдена.");
        return;
    }

    if (task.State == ToDoItemState.Completed)
    {
        Console.WriteLine($"Задача \"{task.Name}\" уже выполнена.");
        return;
    }

    task.State = ToDoItemState.Completed;
    task.StateChangedAt = DateTime.Now;
    Console.WriteLine($"Задача \"{task.Name}\" выполнена.");
}

static void HandleShowAllTasksCommand(List<ToDoItem> tasks)
{
    if (!tasks.Any())
    {
        Console.WriteLine("Список задач пуст.");
        return;
    }

    foreach (ToDoItem task in tasks)
    {
        string state = task.State == ToDoItemState.Active ? "Активна" : "Выполнена";
        Console.WriteLine($"({task.State}) {task.Name} - {task.CreatedAt:dd.MM.yyyy HH:mm:ss} - {task.Id}");
    }
}

static ToDoItem? FindTaskById(List<ToDoItem> tasks, Guid taskId)
{
    return tasks.FirstOrDefault(task => task.Id == taskId);
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
