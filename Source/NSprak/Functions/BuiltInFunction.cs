using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using NSprak.Exceptions;
using NSprak.Execution;
using NSprak.Expressions.Types;
using NSprak.Functions.Signatures;
using NSprak.Language;

namespace NSprak.Functions
{
    public class SprakNameOverride : Attribute
    {
        public string Name;

        public SprakNameOverride(string nameOverride)
        {
            Name = nameOverride;
        }
    }

    public delegate Value BuiltInAction(ExecutionContext context);

    public class BuiltInFunction
    {
        private readonly MethodInfo _methodInfo;

        private readonly bool _acceptsContext;

        public FunctionSignature Signature { get; }

        public IReadOnlyList<string> ParameterNames { get; }

        public SprakType ReturnType { get; }

        public string Name => Signature.Name;

        public string LibraryID => Signature.Namespace;

        public IReadOnlyList<SprakType> Parameters => Signature.TypeSignature.Parameters;

        public string FullName => Signature.FullName;

        public FunctionInfo Info => new FunctionInfo(Signature, ReturnType);

        public BuiltInFunction(MethodInfo methodInfo, Library parent)
        {
            _methodInfo = methodInfo;

            string name = methodInfo
                .GetCustomAttribute<SprakNameOverride>()
                ?.Name;

            name ??= methodInfo.Name;

            List<SprakType> types;
            List<string> names;
            ParseParameters(methodInfo, out _acceptsContext, out types, out names);

            Signature = new FunctionSignature(parent.UniqueID, name, new FunctionTypeSignature(types));
            ParameterNames = names;

            ParseReturnType(methodInfo, out SprakType returnType);
            ReturnType = returnType;
        }

        public Value Call(Value[] arguments, ExecutionContext context)
        {
            if (arguments.Length != Parameters.Count)
            {
                string message = $"Expected {Parameters.Count} parameter(s) for builtin {FullName}, " +
                    $"found {arguments.Length}";

                throw new SprakInternalRuntimeException(message);
            }

            for (int i = 0; i < arguments.Length; i++)
            {
                if (Parameters[i] == SprakType.Any)
                    continue;

                if (arguments[i].Type == Parameters[i])
                    continue;

                // Note that any attempted conversion will have happened before the call.
                // This means that this should never happen, ideally, since the error
                // would have been raised at the attempted conversion.

                string expected = Parameters[i].InternalName;
                string given = arguments[i].Type.InternalName;
                string name = ParameterNames[i];

                string message = $"Expected {expected} as argument {i} ({name}) for" +
                    $" builtin {FullName}, found {given}. Cannot convert from found to expected";

                throw new SprakInternalRuntimeException(message);
            }

            List<object> netArguments = new List<object>(arguments.Length + 1);
            if (_acceptsContext) netArguments.Add(context);
            netArguments.AddRange(arguments);

            Value result = (Value)_methodInfo.Invoke(null, netArguments.ToArray());

            if (result.Type != ReturnType)
            {
                string message = $"Expected action for {FullName}" +
                    $" to return {ReturnType.InternalName}, found {result.Type.InternalName}";

                throw new SprakInternalRuntimeException(message);
            }

            return result;
        }

        public override string ToString()
        {
            return Info.ToString();
        }

        private void ParseParameters(MethodInfo info, out bool acceptsContext, out List<SprakType> types, out List<string> names)
        {
            types = new List<SprakType>();
            names = new List<string>();
            acceptsContext = false;

            ParameterInfo[] parameters = info.GetParameters();

            if (parameters.Length == 0)
                return;

            int index = 0;

            if (parameters[0].ParameterType == typeof(ExecutionContext))
            {
                acceptsContext = true;
                index++;
            }

            while (index < parameters.Length)
            {
                if (SprakType.TryGetSprak(parameters[index].ParameterType, out SprakType sprakType))
                {
                    types.Add(sprakType);
                    names.Add(parameters[index].Name);
                }
                else
                {
                    string paramName = parameters[index].Name;
                    string typeName = parameters[index].ParameterType.Name;
                    string methodName = info.DeclaringType.Name + "." + info.Name;
                    string message = $"Unable to recogize the type of parameter [{index} ({paramName}) of {methodName}], {typeName}, as a Sprak type";

                    throw new ArgumentException(message);
                }

                index++;
            }
        }

        private void ParseReturnType(MethodInfo info, out SprakType returnType)
        {
            if (!SprakType.NetToSprakLookup.TryGetValue(info.ReturnType, out returnType))
            {
                string returnName = info.ReturnType.Name;
                string methodName = info.DeclaringType.Name + "." + info.Name;
                string message = $"Unable to recognize return type {returnName} of {methodName} as a Sprak type";

                throw new ArgumentException(message);
            }
        }
    }
}
