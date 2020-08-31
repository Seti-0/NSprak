using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

using NSprak.Exceptions;
using NSprak.Language.Values;
using NSprak.Tokens;

namespace NSprak.Language
{
    public partial class SprakType : IEquatable<SprakType>
    {
        private Value _default;

        public string InternalName { get; private set; }

        // This uniqueness is NOT enforced
        public string UniqueID => InternalName;

        public Type ValueType { get; private set; }

        public string Text { get; private set; }

        private SprakType() { }

        public Value Default()
        {
            return _default.Copy();
        }

        public override string ToString()
        {
            return InternalName;
        }

        public override int GetHashCode()
        {
            return InternalName.GetHashCode();
        }

        #region Equals
        public bool Equals([AllowNull] SprakType other)
        {
            if (other is null)
                return false;

            return InternalName == other.InternalName;
        }

        public sealed override bool Equals(object obj)
        {
            if (obj is SprakType parameter)
                return Equals(parameter);

            return false;
        }

        public static bool operator ==(SprakType one, SprakType two)
        {
            return Equals(one, two);
        }

        public static bool operator !=(SprakType one, SprakType two)
        {
            return !Equals(one, two);
        }
        #endregion
    }

    public partial class SprakType
    {
        public static readonly IReadOnlyList<SprakType> All;

        public static readonly SprakType

            Unit = new SprakType
            {
                InternalName = nameof(Unit),
                Text = "void",
                ValueType = typeof(SprakUnit),

                _default = SprakUnit.Value
            },

            Boolean = new SprakType
            {
                InternalName = nameof(Boolean),
                Text = "boolean",
                ValueType = typeof(SprakBoolean),

                _default = new SprakBoolean()
            },

            Number = new SprakType
            {
                InternalName = nameof(Number),
                Text = "number",
                ValueType = typeof(SprakNumber),

                _default = new SprakNumber()
            },

            String = new SprakType
            {
                InternalName = nameof(String),
                Text = "string",
                ValueType = typeof(SprakString),

                _default = new SprakString()
            },

            Array = new SprakType
            {
                InternalName = nameof(Array),
                Text = "array",
                ValueType = typeof(SprakArray),

                _default = new SprakArray()
            },

            Connection = new SprakType
            {
                InternalName = nameof(Connection),
                // It is not possible to declare a variable of this type in sprak, the type "any"
                // must be used to refer to it
                Text = null,
                ValueType = typeof(SprakConnection),

                _default = new SprakConnection()
            },

            Any = new SprakType
            {
                InternalName = nameof(Any),
                Text = "var",
                // "Any" is kind of like the abstract base type of all of the above. All is assignable to it,
                // but no Value will have it as its type
                ValueType = typeof(Value),

                _default = SprakUnit.Value
            };

        private readonly static Dictionary<string, SprakType> stringToSprak = new Dictionary<string, SprakType>();

        public static IReadOnlyDictionary<SprakType, Type> SprakToNetLookup { get; private set; } = new Dictionary<SprakType, Type>();
        public static IReadOnlyDictionary<Type, SprakType> NetToSprakLookup { get; private set; } = new Dictionary<Type, SprakType>();

        static SprakType()
        {
            All = new SprakType[]
            {
                Unit, Boolean, Number, String, Array, Connection, Any
            };

            stringToSprak = All
                .Where(x => x.Text != null)
                .ToDictionary(x => x.Text);

            SprakToNetLookup = All
                .Where(x => x.ValueType != null)
                .ToDictionary(x => x, x => x.ValueType);

            NetToSprakLookup = All
                .Where(x => x.ValueType != null)
                .ToDictionary(x => x.ValueType);
        }

        public static bool IsType(string name)
        {
            return stringToSprak.ContainsKey(name);
        }

        public static bool TryParse(string name, out SprakType type)
        {
            return stringToSprak.TryGetValue(name, out type);
        }

        public static bool TryGetNet(SprakType sprakType, out Type netType)
        {
            return SprakToNetLookup.TryGetValue(sprakType, out netType);
        }

        public static bool TryGetSprak(Type netType, out SprakType sprakType)
        {
            return NetToSprakLookup.TryGetValue(netType, out sprakType);
        }
    }
}
