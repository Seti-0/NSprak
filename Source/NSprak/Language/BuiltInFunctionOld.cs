using NSprak.Expressions.Statments;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace NSprak.Language
{
    public class BuiltInEnviroment
    {

    }

    /// <summary>
    /// Interesting experiment - but, ultimately silly. Builtins should really interact with the stack directly
    /// </summary>
    public class BuiltInFunction
    {
        private static Dictionary<Type, SprakType> conversions = new Dictionary<Type, SprakType>
        {
            { typeof(bool), SprakTypes.Boolean },
            { typeof(double), SprakTypes.Number },
            { typeof(string), SprakTypes.String },
            { typeof(List<object>), SprakTypes.Array },
        };

        public string Name { get; }

        public SprakType ReturnType { get; }

        public List<FunctionParameter> Parameters { get; } = new List<FunctionParameter>();

        private bool _acceptsState;

        private MethodInfo _methodInfo;

        public BuiltInFunction(MethodInfo methodInfo)
        {
            Name = _methodInfo.Name;

            _methodInfo = methodInfo;

            if (!conversions.TryGetValue(_methodInfo.ReturnType, out SprakType returnType))
                throw new InvalidCastException($"Unable to find SprakType for return type {methodInfo.ReturnType} of {DebugString(_methodInfo)}");

            ReturnType = returnType;

            IEnumerator enumerator = methodInfo.GetParameters().GetEnumerator();

            // Allow an optional "Enviroment" parameter which gives the method access
            // to things like the Computer's "hardware"
            if (enumerator.MoveNext() 
                && ((ParameterInfo)enumerator.Current).ParameterType == typeof(BuiltInEnviroment))
            {
                _acceptsState = true;
                enumerator.MoveNext();
            }

            while(enumerator.MoveNext())
            {
                ParameterInfo parameter = (ParameterInfo)enumerator.Current;
                
                if (!conversions.TryGetValue(parameter.ParameterType, out SprakType type))
                    throw new InvalidCastException($"Unable to find SprakType for parameter {parameter.Name} {methodInfo.ReturnType} of {DebugString(_methodInfo)}");

                Parameters.Add(new FunctionParameter(parameter.Name, type));
            }
        }

        private string DebugString(MethodInfo info)
        {
            return $"method {info.Name} in class {info.DeclaringType.Name}";
        }

        public Value Call(List<Value> arguments, BuiltInEnviroment env)
        {
            List<object> actualArguments = new List<object>();

            if (_acceptsState) actualArguments.Add(env);
            actualArguments.AddRange(arguments.Select(x => x.RawObject));

            _methodInfo.Invoke(null, actualArguments.ToArray());

            return null;
        }
    }
}
