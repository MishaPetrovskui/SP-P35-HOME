using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;

class Program
{
    static bool stopRequested = false;

    public static void ShowProcessesWithInterval(int seconds)
    {
        stopRequested = false;

        Thread worker = new Thread(() =>
        {
            while (!stopRequested)
            {
                Console.Clear();
                List<Process> processes = Process.GetProcesses().OrderBy(p => p.ProcessName).ToList();
                Console.WriteLine("Список процесів (натисніть Enter для виходу):\n");
                foreach (var process in processes)
                {
                    Console.WriteLine($"{process.ProcessName,-30} ID: {process.Id}");
                }

                for (int i = 0; i < seconds * 10; i++)
                {
                    if (stopRequested) return;
                    Thread.Sleep(100);
                }
            }
        });

        worker.Start();

        Console.ReadLine();
        stopRequested = true;
        worker.Join();
    }

    public static void ShowProcessDetails()
    {
        Console.Write("Введіть ID процесу: ");
        string input = Console.ReadLine();
        int pid;
        if (int.TryParse(input, out pid))
        {
            try
            {
                Process proc = Process.GetProcessById(pid);
                int count = Process.GetProcessesByName(proc.ProcessName).Length;

                Console.WriteLine($"\nІдентифікатор: {proc.Id}");
                Console.WriteLine($"Час старту: {proc.StartTime}");
                Console.WriteLine($"Процесорний час: {proc.TotalProcessorTime}");
                Console.WriteLine($"Кількість потоків: {proc.Threads.Count}");
                Console.WriteLine($"Кількість копій: {count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Помилка: " + ex.Message);
            }
        }
        else
        {
            Console.WriteLine("Невірний ID.");
        }
    }

    public static void KillProcess()
    {
        Console.Write("Введіть ID процесу для завершення: ");
        string input = Console.ReadLine();
        int pid;
        if (int.TryParse(input, out pid))
        {
            try
            {
                Process.GetProcessById(pid).Kill();
                Console.WriteLine("Процес завершено.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Помилка: " + ex.Message);
            }
        }
        else
        {
            Console.WriteLine("Невірний ID.");
        }
    }

    public static void StartApplication()
    {
        Console.WriteLine("Оберіть програму:");
        Console.WriteLine("1. Notepad");
        Console.WriteLine("2. Calculator");
        Console.WriteLine("3. Paint");
        Console.WriteLine("4. Власна програма");
        Console.Write("Ваш вибір: ");
        string input = Console.ReadLine();

        try
        {
            switch (input)
            {
                case "1":
                    Process.Start("notepad.exe");
                    break;
                case "2":
                    Process.Start("calc.exe");
                    break;
                case "3":
                    Process.Start("mspaint.exe");
                    break;
                case "4":
                    Console.Write("Введіть повний шлях: ");
                    string path = Console.ReadLine();
                    Process.Start(path);
                    break;
                default:
                    Console.WriteLine("Невірний вибір.");
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Помилка запуску: " + ex.Message);
        }
    }

    static void Main(string[] args)
    {
        Console.InputEncoding = UTF8Encoding.UTF8;
        Console.OutputEncoding = UTF8Encoding.UTF8;

        while (true)
        {
            Console.Clear();
            Console.WriteLine("=== МЕНЮ ===");
            Console.WriteLine("1. Вивести список процесів (оновлення)");
            Console.WriteLine("2. Показати деталі процесу");
            Console.WriteLine("3. Завершити процес");
            Console.WriteLine("4. Запустити програму");
            Console.WriteLine("5. Вийти");
            Console.Write("\nВаш вибір: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1":
                    Console.Write("Введіть інтервал оновлення (сек): ");
                    if (int.TryParse(Console.ReadLine(), out int interval))
                        ShowProcessesWithInterval(interval);
                    else
                        Console.WriteLine("Невірний інтервал.");
                    break;

                case "2":
                    ShowProcessDetails();
                    break;

                case "3":
                    KillProcess();
                    break;

                case "4":
                    StartApplication();
                    break;

                case "5":
                    return;

                default:
                    Console.WriteLine("Невірна команда.");
                    break;
            }

            Console.WriteLine("\nНатисніть Enter для продовження...");
            Console.ReadLine();
        }
    }
}
