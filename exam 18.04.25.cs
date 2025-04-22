using System.Text;
using System.Threading;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Game
{
    class BackupDirectory
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public BackupMode Mode { get; set; }
        public FileSystemWatcher Watcher { get; set; }
        public Timer Timer { get; set; }
        public int IntervalMs { get; set; }
    }

    enum BackupMode
    {
        Regular,
        OnChange
    }

    class SmartBackup
    {
        private List<BackupDirectory> directories = new();
        private List<string> logs = new();
        private SemaphoreSlim copySemaphore;
        private int maxConcurrentCopies = 2;
        private const int LogStartColumn = 50;
        private const int LogMaxLines = 20;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetStdHandle(int nStdHandle);

        [StructLayout(LayoutKind.Sequential)]
        public struct COORD
        {
            public short X;
            public short Y;
            public COORD(short x, short y)
            {
                X = x;
                Y = y;
            }
        }

        const int STD_OUTPUT_HANDLE = -11;
        IntPtr consoleHandle = GetStdHandle(STD_OUTPUT_HANDLE);

        public SmartBackup()
        {
            copySemaphore = new SemaphoreSlim(maxConcurrentCopies);
        }

        private void Log(string message)
        {
            lock (logs)
            {
                if (logs.Count >= LogMaxLines)
                    logs.RemoveAt(0);
                logs.Add($"{DateTime.Now:T} - {message}");
                RedrawLog();
            }
        }

        private void RedrawLog()
        {
            int maxLogWidth = Console.WindowWidth - LogStartColumn - 1;

            for (int i = 0; i < logs.Count; i++)
            {
                Console.SetCursorPosition(LogStartColumn, i);
                string line = logs[i];
                if (line.Length > maxLogWidth)
                    line = line.Substring(0, maxLogWidth);
                Console.Write(line.PadRight(maxLogWidth));
            }
            Console.SetCursorPosition(0, Console.WindowHeight - 2);
        }


        private void ClearLeftSide()
        {
            for (int i = 0; i < Console.WindowHeight; i++)
            {
                Console.SetCursorPosition(0, i);
                Console.Write(new string(' ', LogStartColumn - 1));
            }
        }

        public void AddDirectory(string source, string destination, BackupMode mode, int intervalSeconds = 60)
        {
            var dir = new BackupDirectory
            {
                SourcePath = source,
                DestinationPath = destination,
                Mode = mode,
                IntervalMs = intervalSeconds * 1000
            };

            if (mode == BackupMode.OnChange)
            {
                if (Directory.Exists(source))
                {
                    var watcher = new FileSystemWatcher(source);
                    watcher.IncludeSubdirectories = true;
                    watcher.EnableRaisingEvents = true;
                    watcher.Changed += (s, e) => StartCopy(e.FullPath, destination);
                    watcher.Created += (s, e) => StartCopy(e.FullPath, destination);
                    watcher.Renamed += (s, e) => StartCopy(e.FullPath, destination);
                    dir.Watcher = watcher;
                    Log($"Watching '{source}' for changes.");
                }
                else
                {
                    Log($"Помилка: Директорія {source} не існує.");
                    return;
                }
            }
            else
            {
                Timer timer = new Timer(_ => StartDirectoryCopy(source, destination), null, 0, dir.IntervalMs);
                dir.Timer = timer;
                Log($"Регулярне копіювання '{source}' кожні {intervalSeconds} сек.");
            }

            directories.Add(dir);
        }

        public bool DeleteDirectory(int index)
        {
            if (index >= directories.Count || index < 0)
                return false;

            if (directories[index].Mode == BackupMode.OnChange)
                directories[index].Watcher.Dispose();
            else
                directories[index].Timer.Dispose();

            Log($"Директорія '{directories[index].SourcePath}' видалена.");
            directories.RemoveAt(index);
            return true;
        }

        private void StartCopy(string filePath, string destinationDir)
        {
            copySemaphore.Wait();
            try
            {
                if (!File.Exists(filePath)) return;
                string relativePath = Path.GetRelativePath(Path.GetDirectoryName(filePath), filePath);
                string destPath = Path.Combine(destinationDir, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destPath));
                File.Copy(filePath, destPath, true);
                Log($"Скопійовано: {filePath} -> {destPath}");
            }
            catch (Exception ex)
            {
                Log($"Помилка копіювання {filePath}: {ex.Message}");
            }
            finally
            {
                copySemaphore.Release();
            }
        }

        private void StartDirectoryCopy(string source, string destination)
        {
            foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
            {
                StartCopy(file, destination);
            }
        }

        public void ShowMenu()
        {
            while (true)
            {
                ClearLeftSide();
                Console.SetCursorPosition(0, 0);
                Console.WriteLine("=== SmartBackup Menu ===");
                Console.WriteLine("1. Додати директорію");
                Console.WriteLine("2. Переглянути директорії");
                Console.WriteLine("3. Видалити директорію");
                Console.WriteLine("4. Очищення помилок");
                Console.WriteLine("5. Вийти");
                Console.Write("Оберіть опцію: ");
                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        ClearLeftSide();
                        Console.SetCursorPosition(0, 0);
                        Console.Write("Шлях до директорії: ");
                        string src = Console.ReadLine();
                        Console.Write("Куди зберігати копії: ");
                        string dst = Console.ReadLine();
                        Console.Write("Режим (1 - регулярний, 2 - при зміні): ");
                        int mode = int.Parse(Console.ReadLine());
                        int interval = 60;
                        if (mode == 1)
                        {
                            Console.Write("Інтервал у секундах: ");
                            interval = int.Parse(Console.ReadLine());
                        }
                        AddDirectory(src, dst, (BackupMode)(mode - 1), interval);
                        break;
                    case "2":
                        ClearLeftSide();
                        Console.SetCursorPosition(0, 0);
                        int i = 0;
                        foreach (var d in directories)
                        {
                            Console.WriteLine($"{i++}. {d.SourcePath} -> {d.DestinationPath} | Mode: {d.Mode}");
                        }
                        Console.WriteLine("Натисніть Enter для повернення в меню...");
                        Console.ReadLine();
                        break;
                    case "3":
                        ClearLeftSide();
                        Console.SetCursorPosition(0, 0);
                        Console.WriteLine("Яку директорію видалити?");
                        Console.Write(">> ");
                        if (int.TryParse(Console.ReadLine(), out int numb))
                        {
                            if (!DeleteDirectory(numb))
                                Log("Невірна опція для видалення.");
                        }
                        else
                        {
                            Log("Введено не число.");
                        }
                        break;
                    case "4":
                        logs.Clear();
                        RedrawLog();
                        break;
                    case "5":
                        return;
                    default:
                        Log("Невірна опція в меню.");
                        break;
                }
                Console.SetCursorPosition(0, Console.WindowHeight - 2);
            }
        }

        static void Main()
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            Console.Clear();
            SmartBackup app = new SmartBackup();
            app.ShowMenu();
        }
    }
}
