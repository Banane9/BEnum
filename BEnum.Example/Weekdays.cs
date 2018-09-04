using System;
using System.Linq;
using System.Collections.Generic;

namespace BEnum.Example
{
    public sealed class Weekdays : BEnum<Weekdays>
    {
        public static readonly Weekdays Friday = new Weekdays(16, 4, false);
        public static readonly Weekdays Monday = new Weekdays(1, 0, false);
        public static readonly Weekdays Saturday = new Weekdays(32, 5, true);
        public static readonly Weekdays Sunday = new Weekdays(64, 6, true);
        public static readonly Weekdays Thursday = new Weekdays(8, 3, false);
        public static readonly Weekdays Tuesday = new Weekdays(2, 1, false);
        public static readonly Weekdays Wednesday = new Weekdays(4, 2, false);
        public static readonly Weekdays Weekend = new Weekdays(32 | 64, -1, true);

        public bool IsWeekend { get; }

        public int Number { get; }

        private Weekdays(ulong value)
                    : base(value)
        {
            IsWeekend = GetFlags(false).All(day => day.IsWeekend);
        }

        private Weekdays(ulong value, int number, bool isWeekend)
            : base(value)
        {
            Number = number;
            IsWeekend = isWeekend;
        }
    }
}