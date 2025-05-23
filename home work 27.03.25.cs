using System;
using System.Runtime.InteropServices;
using System.Threading;

class Task
{
    const uint MB_OK = 0x00000000;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern int MessageBox(IntPtr hWnd, string text, string caption, uint type);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    public static extern bool Beep(int frequency, int duration);

    static void Main()
    {
        int[,] melody = {
            { 659, 100 }, { 659, 100 }, { 0, 100 }, { 659, 100 }, { 0, 100 }, { 523, 100 }, { 659, 100 }, { 0, 100 }, { 784, 100 },
            { 0, 300 }, { 392, 100 }, { 0, 300 },
            { 523, 100 }, { 0, 300 }, { 392, 100 }, { 0, 300 }, { 330, 100 }, { 0, 300 },
            { 440, 100 }, { 0, 100 }, { 494, 100 }, { 0, 100 }, { 466, 100 }, { 440, 100 }, { 0, 100 }, { 392, 100 }, { 659, 100 },
            { 784, 100 }, { 880, 100 }, { 698, 100 }, { 784, 100 }, { 659, 100 }, { 523, 100 }, { 587, 100 }, { 494, 100 }
        };

        MessageBox(IntPtr.Zero, "Мене звати Михайло Петровський.\nЯ студент ІТ-спеціальності.", "GAME", MB_OK);
        Thread musicThread = new Thread(() =>
        {
            for (int i = 0; i < melody.GetLength(0); i++)
            {
                if (melody[i, 0] > 0)
                    Beep(melody[i, 0], melody[i, 1]);
                else
                    Thread.Sleep(melody[i, 1]);
            }
        });
        musicThread.Start();
        MessageBox(IntPtr.Zero, "Я навчаюся програмуванню\nта люблю розробляти застосунки.", "GAME", MB_OK);
        musicThread.Join();
        MessageBox(IntPtr.Zero, "Це приклад програми\nз використанням MessageBox та Beep.", "GAME", MB_OK);
    }
}
