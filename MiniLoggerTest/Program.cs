using System;
using Serilog;

namespace MiniLoggerTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // Настройка Serilog: консоль + файл с дневной ротацией
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs\\minilogger-.log",
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            // 1. Debug — перед запуском основной логики
            Log.Debug("Инициализация приложения. Подготовка к запуску основной логики.");

            // 2. Information — приложение запущено
            Log.Information("Приложение MiniLoggerTest запущено.");

            // 3. Основная логика — запрашиваем возраст пользователя
            Console.Write("Введите ваш возраст: ");
            string input = Console.ReadLine();

            if (!int.TryParse(input, out int age) || age < 0 || age > 150)
            {
                // Warning — некорректный ввод, применяем значение по умолчанию
                Log.Warning("Некорректный ввод возраста: \"{Input}\". Применяется значение по умолчанию: 25.", input);
                age = 25;
            }
            else
            {
                Log.Information("Пользователь указал возраст: {Age}.", age);
            }

            // 4. Error — эмуляция ошибки
            try
            {
                Log.Debug("Попытка выполнить деление...");
                int result = 100 / (age - age); // намеренное деление на ноль
                Console.WriteLine(result);
            }
            catch (DivideByZeroException ex)
            {
                Log.Error(ex, "Произошла ошибка при вычислении: деление на ноль.");
            }

            Log.Information("Приложение MiniLoggerTest завершает работу.");

            // Корректное завершение логирования
            Log.CloseAndFlush();

            Console.WriteLine("\nРабота завершена. Проверьте папку logs/ для просмотра лог-файла.");
            Console.WriteLine("Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }
    }
}