using System;
using System.Collections.Generic;
using System.Diagnostics;
using Serilog;
using Serilog.Events;

namespace TaskManager
{
    class Program
    {
        // Список задач
        static List<string> tasks = new List<string>();

        // Трассировка (System.Diagnostics — осталась из предыдущей версии)
        static TraceSource traceSource = new TraceSource("TaskManager", SourceLevels.All);

        static void Main(string[] args)
        {
            // --- Настройка трассировки (System.Diagnostics) ---
            var fileListener = new TextWriterTraceListener("app-trace.log");
            fileListener.TraceOutputOptions = TraceOptions.DateTime;
            Trace.Listeners.Add(fileListener);
            Trace.AutoFlush = true;
            traceSource.Listeners.Clear();
            traceSource.Listeners.Add(fileListener);

            // --- Настройка Serilog: структурированное логирование ---
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                // Текстовые логи в консоль (для удобства чтения)
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                // Текстовые логи в файл (как было раньше)
                .WriteTo.File("logs\\taskmanager-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                // НОВОЕ: структурированные логи в JSON-файл
                .WriteTo.File(
                    new Serilog.Formatting.Json.JsonFormatter(),
                    "logs\\taskmanager-structured-.json",
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // --- Старт приложения ---
            Log.Information("Приложение TaskManager запущено");
            Trace.WriteLine("[TRACE] Инициализация системы хранения задач...");

            try
            {
                if (tasks == null)
                {
                    Log.Fatal("Не удалось инициализировать компонент хранения задач. Завершение приложения.");
                    return;
                }

                Trace.WriteLine("[TRACE] Система хранения задач инициализирована успешно.");
                Console.WriteLine("=== Менеджер задач ===");
                Console.WriteLine("Команды: add, remove, list, exit");
                Console.WriteLine();

                // --- Главный цикл ---
                bool running = true;
                while (running)
                {
                    Console.Write("Введите команду: ");
                    string input = Console.ReadLine()?.Trim().ToLower();

                    switch (input)
                    {
                        case "add":
                            AddTask();
                            break;
                        case "remove":
                            RemoveTask();
                            break;
                        case "list":
                            ListTasks();
                            break;
                        case "exit":
                            running = false;
                            break;
                        default:
                            Log.Warning("Неизвестная команда: {Command}", input);
                            Console.WriteLine("Неизвестная команда. Доступные: add, remove, list, exit");
                            break;
                    }

                    Console.WriteLine();
                }

                Log.Information("Пользователь завершил работу командой exit");
                Trace.WriteLine("[TRACE] Приложение корректно завершено.");
                Console.WriteLine("До свидания!");
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Необработанная ошибка. Завершение приложения.");
            }
            finally
            {
                traceSource.Close();
                Trace.Close();
                Log.CloseAndFlush();
            }
        }

        static void AddTask()
        {
            Trace.WriteLine("[TRACE] Начало операции: добавление задачи.");

            Console.Write("Введите название задачи: ");
            string taskName = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(taskName))
            {
                // Структурированный лог — свойство Operation попадёт в JSON
                Log.Warning("Пустое название задачи. Операция {Operation} не выполнена.", "add");
                Console.WriteLine("Ошибка: название задачи не может быть пустым.");
                Trace.WriteLine("[TRACE] Конец операции: добавление задачи (неуспешно).");
                return;
            }

            tasks.Add(taskName);

            // Структурированный лог — TaskName и TaskCount попадут как отдельные поля в JSON
            Log.Information("Задача {TaskName} успешно добавлена. Всего задач: {TaskCount}",
                taskName, tasks.Count);
            Console.WriteLine($"Задача \"{taskName}\" добавлена.");

            Trace.WriteLine($"[TRACE] Конец операции: добавление задачи (успешно). Количество: {tasks.Count}.");
        }

        static void RemoveTask()
        {
            Trace.WriteLine("[TRACE] Начало операции: удаление задачи.");

            Console.Write("Введите название задачи для удаления: ");
            string taskName = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(taskName))
            {
                Log.Warning("Пустое название задачи. Операция {Operation} не выполнена.", "remove");
                Console.WriteLine("Ошибка: название задачи не может быть пустым.");
                Trace.WriteLine("[TRACE] Конец операции: удаление задачи (неуспешно).");
                return;
            }

            bool removed = tasks.Remove(taskName);

            if (removed)
            {
                Log.Information("Задача {TaskName} успешно удалена. Осталось задач: {TaskCount}",
                    taskName, tasks.Count);
                Console.WriteLine($"Задача \"{taskName}\" удалена.");
            }
            else
            {
                Log.Error("Задача {TaskName} не найдена для удаления. Текущее количество: {TaskCount}",
                    taskName, tasks.Count);
                Console.WriteLine($"Ошибка: задача \"{taskName}\" не найдена.");
            }

            Trace.WriteLine($"[TRACE] Конец операции: удаление (результат: {(removed ? "успешно" : "неуспешно")}).");
        }

        static void ListTasks()
        {
            Trace.WriteLine("[TRACE] Начало операции: вывод списка задач.");

            if (tasks.Count == 0)
            {
                Log.Information("Вывод списка: список задач пуст. Количество: {TaskCount}", tasks.Count);
                Console.WriteLine("Список задач пуст.");
                Trace.WriteLine("[TRACE] Конец операции: вывод списка (пуст).");
                return;
            }

            Console.WriteLine("--- Текущие задачи ---");
            for (int i = 0; i < tasks.Count; i++)
            {
                Console.WriteLine($"  {i + 1}. {tasks[i]}");
            }
            Console.WriteLine("----------------------");

            // Структурированный лог — список задач как массив в JSON
            Log.Information("Выведен список задач. Количество: {TaskCount}. Задачи: {TaskList}",
                tasks.Count, tasks);

            Trace.WriteLine($"[TRACE] Конец операции: вывод списка. Количество: {tasks.Count}.");
        }
    }
}