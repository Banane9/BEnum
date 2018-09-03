using System;
using System.Linq;

namespace BEnum.Example
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Days of the week:");
            foreach (var day in Weekdays.GetNames())
                Console.WriteLine($" - {day}");

            Console.WriteLine();
            Console.WriteLine("Days that are part of the weekend:");
            foreach (var weekendDay in Weekdays.GetNames().Where(name => Weekdays.Parse(name).IsWeekend))
                Console.WriteLine($" - {weekendDay}");

            Console.WriteLine();
            Console.WriteLine("Days in order are:");
            foreach (var day in Weekdays.GetValues().OrderBy(d => d.Number))
                Console.WriteLine($" {day.Number + 1}. {day}");

            Console.WriteLine();
            Console.WriteLine($"Wednesday has the value: {(long)Weekdays.Wednesday}");
            Console.WriteLine($"The value 4 means: {(Weekdays)4L}");
            Console.WriteLine($"It can also be parsed back: Sunday {(((Weekdays)"Sunday").IsWeekend ? "is" : "isn't")} part of the weekend.");

            Console.WriteLine();
            Console.WriteLine($"Multiple days print like this: {Weekdays.Monday | Weekdays.Wednesday}");
            Console.WriteLine($"And can also be parsed back: {string.Join(", ", ((Weekdays)"[Monday, Wednesday]").GetFlags())}");
            Console.WriteLine($"Their combined value is: {(long)(Weekdays.Monday | Weekdays.Wednesday)}");
            Console.WriteLine($"And 5 turns into: {(Weekdays)5L}");

            Console.ReadLine();
        }
    }
}