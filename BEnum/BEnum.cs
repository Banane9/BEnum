using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace BEnum
{
    public abstract class BEnum<TEnum>
        where TEnum : BEnum<TEnum>
    {
        private static readonly Type enumType = typeof(TEnum);
        private static readonly Lazy<Dictionary<string, TEnum>> nameIndex = new Lazy<Dictionary<string, TEnum>>(getNameIndex, LazyThreadSafetyMode.ExecutionAndPublication);
        private static readonly Lazy<Dictionary<TEnum, string>> revNameIndex = new Lazy<Dictionary<TEnum, string>>(getRevNameIndex, LazyThreadSafetyMode.ExecutionAndPublication);

        private static readonly Lazy<Dictionary<long, TEnum>> valueIndex = new Lazy<Dictionary<long, TEnum>>(getValueIndex, LazyThreadSafetyMode.ExecutionAndPublication);

        private static Func<long, TEnum> makeInstance;

        private readonly long value;

        public static Func<long, TEnum> InstanceFactory
        {
            get
            {
                return makeInstance
                    ?? throw new NotSupportedException("Use the " + nameof(SetInstanceFactory) + " method, if you would like support for a [Flags]-style enum.");
            }
            set
            {
                if (makeInstance != null)
                    throw new InvalidOperationException("The instance factory for [Flags]-style enum support has already been set.");

                makeInstance = value;
            }
        }

        protected BEnum(long value)
            => this.value = value;

        public static explicit operator BEnum<TEnum>(long value)
            => getInstance(value);

        public static explicit operator BEnum<TEnum>(string name)
            => Parse(name);

        public static explicit operator long(BEnum<TEnum> bEnum)
            => bEnum.value;

        public static string[] GetNames()
            => nameIndex.Value.Keys.ToArray();

        public static TEnum[] GetValues()
            => nameIndex.Value.Values.ToArray();

        public static implicit operator TEnum(BEnum<TEnum> bEnum)
            => (TEnum)bEnum;

        public static bool operator !=(BEnum<TEnum> left, BEnum<TEnum> right)
            => !(left == right);

        public static TEnum operator &(BEnum<TEnum> left, BEnum<TEnum> right)
            => getInstance(left.value & right.value);

        public static TEnum operator ^(BEnum<TEnum> left, BEnum<TEnum> right)
            => getInstance(left.value ^ right.value);

        public static TEnum operator |(BEnum<TEnum> left, BEnum<TEnum> right)
            => getInstance(left.value | right.value);

        public static bool operator ==(BEnum<TEnum> left, BEnum<TEnum> right)
            => !(left is null) && (left.Equals(right) || right is null);

        public static TEnum Parse(string name)
            => TryParse(name, out var instance) ? instance : throw new FormatException("Invalid name of enumeration member.");

        public static bool TryParse(string name, out TEnum instance)
        {
            instance = null;
            var flagsName = name.StartsWith("[") && name.EndsWith("]");

            if (!nameIndex.Value.ContainsKey(name) && !flagsName)
                return false;

            var parts = flagsName ? name.Substring(1, name.Length - 2).Split(new[] { ", " }, StringSplitOptions.None) : new[] { name };
            instance = nameIndex.Value[parts[0]];
            foreach (var part in parts.Skip(1))
            {
                if (!nameIndex.Value.ContainsKey(part))
                    return false;

                instance |= nameIndex.Value[part];
            }

            return true;
        }

        public override bool Equals(object obj)
            => obj is BEnum<TEnum> bEnum && bEnum.value == value;

        public IEnumerable<TEnum> GetFlags()
            => nameIndex.Value.Values.Where(value => HasFlag(value));

        public override int GetHashCode()
            => value.GetHashCode();

        public bool HasFlag(TEnum other)
            => (other.value & value) != 0;

        public override string ToString()
        {
            var flags = GetFlags().ToArray();

            if (flags.Length == 1)
                return revNameIndex.Value[flags[0]];

            return $"[{string.Join(", ", flags.Select(flag => revNameIndex.Value[flag]))}]";
        }

        protected static void SetInstanceFactory(Func<long, TEnum> instanceFactory)
        {
            InstanceFactory = instanceFactory ?? throw new ArgumentNullException(nameof(instanceFactory), "The instance factory can't be set to null!");
        }

        private static IEnumerable<FieldInfo> getFields()
        {
            return enumType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
                           .Where(field => field.IsStatic && field.IsInitOnly && field.FieldType == enumType);
        }

        private static TEnum getInstance(long value)
        {
            if (!valueIndex.Value.ContainsKey(value))
                valueIndex.Value.Add(value, InstanceFactory(value));

            return valueIndex.Value[value];
        }

        private static Dictionary<string, TEnum> getNameIndex()
            => getFields()
                .ToDictionary(field => field.Name, field => (TEnum)field.GetValue(null), StringComparer.InvariantCultureIgnoreCase);

        private static Dictionary<TEnum, string> getRevNameIndex()
                                                                                                                                                                                                                            => getFields()
                .ToDictionary(field => (TEnum)field.GetValue(null), field => field.Name);

        private static Dictionary<long, TEnum> getValueIndex()
            => getFields()
                .Select(field => (TEnum)field.GetValue(null))
                .ToDictionary(entry => entry.value);
    }
}