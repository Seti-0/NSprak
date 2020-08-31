using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SPRAK_IDE.Language.Values;

namespace NSprak.Language.Values
{
    public abstract class Value
    {
        public abstract SprakType Type { get; }

        public virtual T As<T>() where T : Value
        {
            // This method is ridiculous...

            if (typeof(T) == typeof(String))
                return new String(ToFriendlyString()) as T;

            else throw new SprakException($"Cannot convert from {Type.FriendlyName} to {typeof(T).Name}");
        }

        public virtual Value As(SprakType type)
        {
            if (type == SprakType.String)
                return new String(ToFriendlyString());

            throw GetValueException(Type, type);
        }

        public SprakString ToSprakString()
        {
            return new Sprak
        }

        public abstract string ToFriendlyString();

        public string ToDebugString()
        {
            return $"{ToFriendlyString()} ({Type.FriendlyName})";
        }

        protected static Exception GetValueException(SprakType current, SprakType given)
        {
            return new SprakException($"Cannot convert to {given.FriendlyName} from {current.FriendlyName}");
        }
    }

    public abstract class Value<T, TSelf> : Value
        // This is pretty silly... but it does allow for a generic copy method
        where TSelf : Value<T, TSelf>, new()
    {
        public T RawValue { get; set; }

        public Value(T initialValue = default)
        {
            RawValue = initialValue;
        }

        public TSelf Copy()
        {
            return new TSelf()
            {
                RawValue = RawValue
            };
        }

        public override NewTSelf As<NewTSelf>()
        {
            if (this is NewTSelf)
                return Copy() as NewTSelf;

            return base.As<NewTSelf>();
        }

        public override Value As(SprakType type)
        {
            if (type == Type)
                return Copy();

            return base.As(type);
        }
    }
}
