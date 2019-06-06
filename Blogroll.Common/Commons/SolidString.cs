using System;

namespace Blogroll.Common.Commons
{
    /// <summary>
    /// A string that cannot be null. Could be obsoleted by an Option&lt;string&gt;
    /// but that would slightly complicate our APIs.
    /// </summary>
    public sealed class SolidString: IEquatable<string>, IEquatable<SolidString>
    {
        public SolidString(string value)
        {
            _value = value ?? string.Empty;
        }

        private readonly string _value;

        public override string ToString() => _value;

        public static implicit operator string(SolidString value) => value.ToString();

        public static implicit operator SolidString(string value) => new SolidString(value);

        public static bool operator == (SolidString one, SolidString two) => one?.Equals(two) ?? false;

        public static bool operator != (SolidString one, SolidString two) => !(one == two);

        public static bool operator == (SolidString one, string two) => one?.Equals(two) ?? false;

        public static bool operator != (SolidString one, string two) => !(one == two);

        public bool Equals(string other) => _value.Equals(other);

        public bool Equals(SolidString other) => other?.ToString().Equals(ToString()) ?? false;

        public override bool Equals(object other) => (other is string str && _value.Equals(str)) ||
                                                     (other is SolidString sstr && Equals(sstr));

        public override int GetHashCode() => _value.GetHashCode();
    }
}