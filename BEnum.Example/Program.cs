using System;
using System.Linq;

namespace BEnum.Example
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(Weekdays.Monday);

            Console.WriteLine("Days of the week:");
            foreach (var day in Weekdays.GetValues().Where(value => !value.IsComposite))
                Console.WriteLine($" - {day}");

            Console.WriteLine();
            Console.WriteLine("Days that are part of the weekend:");
            foreach (var weekendDay in Weekdays.GetNames().Select(name => Weekdays.Parse(name)).Where(day => day.IsWeekend && !day.IsComposite))
                Console.WriteLine($" - {weekendDay}");

            Console.WriteLine();
            Console.WriteLine("Days in order are:");
            foreach (var day in Weekdays.GetValues().Where(d => !d.IsComposite).OrderBy(d => d.Number))
                Console.WriteLine($" {day.Number + 1}. {day}");

            Console.WriteLine();
            Console.WriteLine($"Wednesday has the value: {(ulong)Weekdays.Wednesday}");
            Console.WriteLine($"The value 4 means: {(Weekdays)4L}");
            Console.WriteLine($"It can also be parsed back: Sunday {(((Weekdays)"Sunday").IsWeekend ? "is" : "isn't")} part of the weekend.");

            Console.WriteLine();
            Console.WriteLine($"Multiple days print like this: {Weekdays.Monday | Weekdays.Wednesday}");
            Console.WriteLine($"And can also be parsed back: {string.Join(", ", ((Weekdays)"[Monday, Wednesday]").GetFlags(includeCompositeMembers: false))}");
            Console.WriteLine($"Their combined value is: {(ulong)(Weekdays.Monday | Weekdays.Wednesday)}");
            Console.WriteLine($"And 5 turns into: {(Weekdays)5L}");

            Console.WriteLine();
            Console.WriteLine($"The {Weekdays.Weekend} is made from:");
            foreach (var weekendDay in Weekdays.Weekend.GetFlags(includeCompositeMembers: false))
                Console.WriteLine($" - {weekendDay}");

            Console.ReadLine();
        }
    }
}