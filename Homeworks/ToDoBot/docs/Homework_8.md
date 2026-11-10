Домашнее задание
Асинхронность, делегаты и события

Цель:
Расширение функционала приложения, разработанного в предыдущих домашних заданиях:

Добавление асинхронности

Описание/Пошаговая инструкция выполнения домашнего задания:

Подключение асинхронной библиотеки Otus.ToDoList.ConsoleBot
Добавить к себе в решение и в зависимости к своему проекту с ботом проект Otus.ToDoList.ConsoleBot GitHub вместо аналогичного проекта добавленнего в рамках ДЗ "ООП классы и интерфейсы"
Ознакомиться с README.md
Переключиться на использование асинхронных методов из библиотеки Otus.ToDoList.ConsoleBot. Обратите внимание на методы из IUpdateHandler и ITelegramBotClient
Реализовать метод IUpdateHandler.HandleErrorAsync. В нем нужно выводить информацию об ошибке в консоль
В метод ITelegramBotClient.StartReceiving нужно передавать CancellationToken. Его нужно создать с помощью CancellationTokenSource
Код библиотеки Otus.ToDoList.ConsoleBot не нужно изменять
Перевод интерфейсов на асинхронность
Мигрировать синхронные методы всех интерфейсов на асинхронные. Методы должны возвращать Task или Task<> и получать CancellationToken
IUserService IUserRepository IToDoService IToDoRepository и тд