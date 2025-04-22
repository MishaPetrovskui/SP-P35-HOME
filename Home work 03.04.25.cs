using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Game
{
    class Program
    {
        static readonly object lockObj = new object();
        static Random rnd = new Random();
        static int sum1 = 0;
        static int sum2 = 0;

        static int Sum(List<int> numbers)
        {
            int total = 0;
            foreach (int number in numbers)
            {
                total += number;
                Thread.Sleep(5);
            }
            return total;
        }

        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;

            List<int> allNumbers = new List<int>();
            for (int i = 0; i < 10; i++)
            {
                allNumbers.Add(rnd.Next(1, 101));
            }

            Console.WriteLine("Масив: [" + string.Join(", ", allNumbers) + "]");

            List<int> part1 = allNumbers.GetRange(0, 5);
            List<int> part2 = allNumbers.GetRange(5, 5);

            Parallel.Invoke(
                () =>
                {
                    int s = Sum(part1);
                    lock (lockObj)
                    {
                        sum1 = s;
                    }
                    Console.WriteLine($"[Потік {Thread.CurrentThread.ManagedThreadId}] Сума першої частини: {sum1}");
                },
                () =>
                {
                    int s = Sum(part2);
                    lock (lockObj)
                    {
                        sum2 = s;
                    }
                    Console.WriteLine($"[Потік {Thread.CurrentThread.ManagedThreadId}] Сума другої частини: {sum2}");
                }
            );

            Console.WriteLine($"\nЗагальна сума масиву: {sum1 + sum2}");
        }
    }
}
