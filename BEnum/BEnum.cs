using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace BEnum
{
    /// <summary>
    /// Represents the abstract base for classes that should work like Java-style <see langword="enum"/>s.
    /// <para/>
    /// Every named member of your enum needs to be declared as <see langword="public static readonly"/>,
    /// otherwise it will be ignored. The names are treated case-insensative and each member should have a unique value
    /// among the other instances of the enum, passed to the base constructor.
    /// <para/>
    /// If you want your enum to support [Flags] style operations, you have to provide a constructor,
    /// that takes a single argument of type <see langword="ulong"/>.
    /// </summary>
    /// <typeparam name="TEnum">The deriving enum class.</typeparam>
    public abstract class BEnum<TEnum>
        where TEnum : BEnum<TEnum>
    {
        /// <summary>
        /// The <see cref="Type"/> of the deriving enum class.
        /// </summary>
        private static readonly Type enumType = typeof(TEnum);

        /// <summary>
        /// For getting instances from names.
        /// </summary>
        private static readonly Lazy<Dictionary<string, TEnum>> nameIndex = new Lazy<Dictionary<string, TEnum>>(getNameIndex, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// For getting names from instances.
        /// </summary>
        private static readonly Lazy<Dictionary<TEnum, string>> revNameIndex = new Lazy<Dictionary<TEnum, string>>(getRevNameIndex, LazyThreadSafetyMode.ExecutionAndPublication);

        /// <summary>
        /// For getting instances from values.
        /// </summary>
        private static readonly Dictionary<ulong, TEnum> valueIndex = new Dictionary<ulong, TEnum>();

        /// <summary>
        /// Function for making an instance with just a value.
        /// </summary>
        private static Func<ulong, TEnum> makeInstance;

        /// <summary>
        /// The numeric value of the instance.
        /// </summary>
        private readonly ulong value;

        /// <summary>
        /// Can't be called from the constructor of an enum member.
        /// <para/>
        /// Gets whether this instance is composited of other members.
        /// </summary>
        public bool IsComposite => GetFlags().Count() > 1;

        /// <summary>
        /// Looks for the constructor to set <see cref="makeInstance"/>.
        /// </summary>
        static BEnum()
        {
            var longType = typeof(ulong);

            var valueCtr = enumType.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(ctr =>
                    {
                        var parameters = ctr.GetParameters();
                        return parameters.Length == 1 && parameters[0].ParameterType == longType;
                    });

            if (valueCtr == null)
                return;

            var valueParam = Expression.Parameter(longType);
            makeInstance = Expression.Lambda<Func<ulong, TEnum>>(
                Expression.New(valueCtr, valueParam),
                valueParam).Compile();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="BEnum{TEnum}"/> class in derived classes.
        /// </summary>
        /// <param name="value">The numeric value of the instance. Should be unique among other instances of the enum.</param>
        protected BEnum(ulong value)
        {
            this.value = value;

            lock (valueIndex)
                valueIndex.Add(value, this);
        }

        /// <summary>
        /// Converts a value into an instance. May result in instances with otherwise impossible values if used for a [Flags] enabled enum.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        public static explicit operator BEnum<TEnum>(ulong value)
            => getInstance(value);

        /// <summary>
        /// Converts an instance's <see cref="ToString"/> back into an instance using <see cref="Parse(string)"/>.
        /// </summary>
        /// <param name="name">The <see cref="ToString"/> of the instance.</param>
        public static explicit operator BEnum<TEnum>(string name)
            => Parse(name);

        /// <summary>
        /// Converts an instance to its numeric value.
        /// </summary>
        /// <param name="bEnum">The instance to convert.</param>
        public static explicit operator ulong(BEnum<TEnum> bEnum)
            => bEnum.value;

        /// <summary>
        /// Can't be called from the constructor of an enum member.
        /// <para/>
        /// Gets the names of all the members of the enum.
        /// </summary>
        /// <returns>A new array containing all the names of the members of the enum.</returns>
        public static string[] GetNames()
            => nameIndex.Value.Keys.ToArray();

        /// <summary>
        /// Can't be called from the constructor of an enum member.
        /// <para/>
        /// Gets the instances of all the members of the enum.
        /// </summary>
        /// <returns>A new array containing all the instances of the members of the enum.</returns>
        public static TEnum[] GetValues()
            => nameIndex.Value.Values.ToArray();

        /// <summary>
        /// Casts the base enum type to the derived type.
        /// </summary>
        /// <param name="bEnum">An instance of the base type.</param>
        public static implicit operator TEnum(BEnum<TEnum> bEnum)
            => (TEnum)bEnum;

        /// <summary>
        /// Determines, whether two instances are unequal.
        /// </summary>
        /// <param name="left">The instance on the left.</param>
        /// <param name="right">The instance on the right.</param>
        /// <returns>Whether the two instances are unequal.</returns>
        public static bool operator !=(BEnum<TEnum> left, BEnum<TEnum> right)
            => !(left == right);

        /// <summary>
        /// Performs a bitwise and on the values of the instances to create a new instance.
        /// <para/>
        /// Usually requires a [Flags] enabled enum.
        /// </summary>
        /// <param name="left">The instance on the left.</param>
        /// <param name="right">The instance on the right.</param>
        /// <returns>The instance with the new value.</returns>
        public static TEnum operator &(BEnum<TEnum> left, BEnum<TEnum> right)
            => getInstance(left.value & right.value);

        /// <summary>
        /// Performs a bitwise xor on the values of the instances to create a new instance.
        /// <para/>
        /// Usually requires a [Flags] enabled enum.
        /// </summary>
        /// <param name="left">The instance on the left.</param>
        /// <param name="right">The instance on the right.</param>
        /// <returns>The instance with the new value.</returns>
        public static TEnum operator ^(BEnum<TEnum> left, BEnum<TEnum> right)
            => getInstance(left.value ^ right.value);

        /// <summary>
        /// Performs a bitwise or on the values of the instances to create a new instance.
        /// <para/>
        /// Usually requires a [Flags] enabled enum.
        /// </summary>
        /// <param name="left">The instance on the left.</param>
        /// <param name="right">The instance on the right.</param>
        /// <returns>The instance with the new value.</returns>
        public static TEnum operator |(BEnum<TEnum> left, BEnum<TEnum> right)
            => getInstance(left.value | right.value);

        /// <summary>
        /// Determines, whether two instances are equal.
        /// </summary>
        /// <param name="left">The instance on the left.</param>
        /// <param name="right">The instance on the right.</param>
        /// <returns>Whether the two instances are equal.</returns>
        public static bool operator ==(BEnum<TEnum> left, BEnum<TEnum> right)
            => !(left is null) && (left.Equals(right) || right is null);

        /// <summary>
        /// Can't be called from the constructor of an enum member.
        /// <para/>
        /// Parses an instance's <see cref="ToString"/> back into an instance.
        /// <para/>
        /// Throws a <see cref="FormatException"/> if parsing fails.
        /// </summary>
        /// <param name="name">The <see cref="ToString"/> of the instance.</param>
        /// <returns>The parsed instance.</returns>
        public static TEnum Parse(string name)
            => TryParse(name, out var instance) ? instance : throw new FormatException("Parsing failed.");

        /// <summary>
        /// Can't be called from the constructor of an enum member.
        /// <para/>
        /// Parses an instance's <see cref="ToString"/> back into an instance,
        /// returning whether the parsing was successful or not.
        /// </summary>
        /// <param name="name">The <see cref="ToString"/> of the instance.</param>
        /// <param name="instance">The parsed instance, or null if it fails.</param>
        /// <returns>Whether the parsing was successful or not.</returns>
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
                {
                    instance = null;
                    return false;
                }

                instance |= nameIndex.Value[part];
            }

            return true;
        }

        /// <summary>
        /// Determines, whether the given object is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare.</param>
        /// <returns>Whether the given object is equal to this instance.</returns>
        public override bool Equals(object obj)
            => obj is TEnum instance && instance.value == value;

        /// <summary>
        /// Can't be called from the constructor of an enum member.
        /// <para/>
        /// Gets all instances of members of the enum that are parts of this instance.
        /// <para/>
        /// If includeCompositeMembers is set to <see langword="true"/> (default) it will also include the instances of members,
        /// that are themselves composites of other instances in the list. Set to <see langword="false"/> to only get pure instances.
        /// </summary>
        /// <param name="includeCompositeMembers">Whether to include instances of members that are themselves composites of other instances in the list.</param>
        /// <returns>All instances of members of the enum that are parts of this instance.</returns>
        public IEnumerable<TEnum> GetFlags(bool includeCompositeMembers = true)
            => nameIndex.Value.Values.Where(instance => includeCompositeMembers || !instance.IsComposite).Where(instance => HasFlag(instance));

        /// <summary>
        /// Gets the hash code of this instance.
        /// </summary>
        /// <returns>The hash code of this instance.</returns>
        public override int GetHashCode()
            => value.GetHashCode();

        /// <summary>
        /// Determines whether this instance is composited in part or totally of the given instance.
        /// </summary>
        /// <param name="other">The instance to determine the inclusion of.</param>
        /// <returns>Whether this instance is composited in part or totally of the given instance.</returns>
        public bool HasFlag(TEnum other)
        {
            if ((value & other.value) == 0)
                return false;

            if (value < other.value)
                return false; // throw new InvalidOperationException("Instance to test for is more composited than the one being tested.");

            return true;
        }

        /// <summary>
        /// Can't be called from the constructor of an enum member.
        /// <para/>
        /// Turns this instance into a string representation that can be turned back into an instance using <see cref="Parse(string)"/>.
        /// </summary>
        /// <returns>The roundtrip-safe string representation of the instance.</returns>
        public override string ToString()
        {
            if (revNameIndex.Value.ContainsKey(this))
                return revNameIndex.Value[this];

            var flags = GetFlags().Where(flag => revNameIndex.Value.ContainsKey(flag)).ToArray();

            if (flags.Length == 1)
                return revNameIndex.Value[flags[0]];

            return $"[{string.Join(", ", flags.Select(flag => revNameIndex.Value[flag]))}]";
        }

        /// <summary>
        /// Gets all instance members of the enum class.
        /// </summary>
        private static IEnumerable<FieldInfo> getFields()
        {
            return enumType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
                           .Where(field => field.IsPublic && field.IsStatic && field.IsInitOnly && field.FieldType == enumType);
        }

        /// <summary>
        /// Gets the instance for the given value, either from the <see cref="valueIndex"/> or creating a new one (and adding it).
        /// </summary>
        /// <param name="value">The value to get the instance for.</param>
        private static TEnum getInstance(ulong value)
        {
            // The constructor adds the new value to the index
            lock (valueIndex)
                return valueIndex.ContainsKey(value) ? valueIndex[value] : makeInstance(value);
        }

        /// <summary>
        /// Gets the map of names to instances.
        /// </summary>
        private static Dictionary<string, TEnum> getNameIndex()
            => getFields()
                .ToDictionary(field => field.Name, field => (TEnum)field.GetValue(null), StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Gets the map of instances to names.
        /// </summary>
        private static Dictionary<TEnum, string> getRevNameIndex()
            => getFields()
                .ToDictionary(field => (TEnum)field.GetValue(null), field => field.Name);
    }
}