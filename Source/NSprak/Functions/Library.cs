using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Markup;
using Microsoft.VisualBasic;
using NSprak.Expressions.Types;
using NSprak.Functions.Signatures;
using NSprak.Language;
using NSprak.Language.Libraries;

namespace NSprak.Functions
{
    public class SprakConversionAttribute : Attribute
    {
        public bool IsPerfect { get; }

        public SprakConversionAttribute(bool isPerfect = true)
        {
            IsPerfect = isPerfect;
        }
    }

    public class SprakOperatorAttribute : Attribute
    {
        public string OperatorName;

        public InputSides InputsHint;

        public SprakOperatorAttribute(string opName, 
            InputSides inputHint = InputSides.Both)
        {
            OperatorName = opName;
            InputsHint = inputHint;
        }
    }

    public class Library
    {
        public static Library Core = new Library("Core",
            typeof(Core), typeof(CoreConversions), typeof(CoreOperators));

        public string Name { get; }

        public string UniqueID => Name;

        private readonly Dictionary<FunctionSignature, BuiltInFunction> _functions = new Dictionary<FunctionSignature, BuiltInFunction>();
        private readonly Dictionary<OperatorSignature, BuiltInFunction> _operators = new Dictionary<OperatorSignature, BuiltInFunction>();
        private readonly Dictionary<ConversionTypeSignature, BuiltInFunction> _conversions = new Dictionary<ConversionTypeSignature, BuiltInFunction>();

        public IReadOnlyDictionary<FunctionSignature, BuiltInFunction> Functions => _functions;
        public IReadOnlyDictionary<OperatorSignature, BuiltInFunction> Operators => _operators;
        public IReadOnlyDictionary<ConversionTypeSignature, BuiltInFunction> Conversions => _conversions;


        public Library(string name, params Type[] types)
        {
            Name = name;
            AddMethods(types);
        }

        public Library(string name, IEnumerable<Type> types)
        {
            Name = name;
            AddMethods(types);
        }

        /*
        public bool HasFunction(FunctionSignature signature)
        {
            return _directLookup.ContainsKey(signature);
        }

        public bool TryLookupFunction(FunctionSignature signature, out BuiltInFunction result)
        {
            return _directLookup.TryGetValue(signature, out result);
        }

        public BuiltInFunction LookupFunction(FunctionSignature signature)
        {
            return _directLookup[signature];
        }
        */

        private void AddMethods(IEnumerable<Type> types)
        {
            foreach (Type type in types)
                foreach (MethodInfo method in type.GetMethods())
                {
                    if (method.IsPublic && method.IsStatic && typeof(Value).IsAssignableFrom(method.ReturnType))
                        AddMethod(method);
                }
        }

        private string MethodName(MethodInfo info)
        {
            string paramStr = string.Join(',', info.GetParameters().Select(
                x => x.ParameterType.Name + " " + x.Name
            ));

            return $"{info.DeclaringType.Name}.{info.Name}({paramStr})";
        }

        private void AddMethod(MethodInfo methodInfo)
        {
            SprakConversionAttribute conversionAttribute = methodInfo.GetCustomAttribute<SprakConversionAttribute>();
            bool isConversion = conversionAttribute != null;

            SprakOperatorAttribute opAttribute = methodInfo.GetCustomAttribute<SprakOperatorAttribute>();
            bool isOperator = opAttribute != null;

            if (isConversion && isOperator)
                throw new ArgumentException($"{MethodName(methodInfo)} is declared to be both an operator and a conversion");

            if (isConversion)
                AddConversion(methodInfo, conversionAttribute.IsPerfect);

            else if (isOperator)
                AddOperator(methodInfo, opAttribute);

            else AddFunction(methodInfo);
        }

        private void AddFunction(MethodInfo methodInfo)
        {
            BuiltInFunction function = new BuiltInFunction(methodInfo, this);
            _functions.Add(function.Signature, function);
        }

        private void AddConversion(MethodInfo methodInfo, bool perfect)
        {
            BuiltInFunction function = new BuiltInFunction(methodInfo, this);

            if (function.Parameters.Count != 1)
            {
                string message = $"{MethodName(methodInfo)} must have a single Sprak parameter in order to be a conversion";
                throw new ArgumentException(message);
            }

            ConversionTypeSignature signature
                = new ConversionTypeSignature(function.Parameters[0], function.ReturnType, perfect);

            _conversions.Add(signature, function);
        }

        private void AddOperator(MethodInfo methodInfo, SprakOperatorAttribute meta)
        {
            BuiltInFunction function = new BuiltInFunction(methodInfo, this);

            if (!Operator.TryParse(out Operator op, name: meta.OperatorName))
            {
                string message = $"{MethodName(methodInfo)} is declared to be an unrecognized operator: \"{meta.OperatorName}\"";
                throw new ArgumentException(message);
            }

            int paramCount = function.Parameters.Count;

            bool binary = meta.InputsHint == InputSides.Both;
            int requiredCount = binary ? 2 : 1;

            if (paramCount != requiredCount)
            {
                string desc = binary ? "binary" : "unary";

                string message = $"{MethodName(methodInfo)} was declared to be the {desc} operator \"{meta.OperatorName}\", " +
                    $"but has {function.Parameters.Count} Sprak arguments";

                throw new ArgumentException(message);
            }

            InputSides inputs = meta.InputsHint;

            SprakType left = null;
            SprakType right = null;

            switch (inputs)
            {
                case InputSides.Both:
                    left = function.Parameters[0];
                    right = function.Parameters[1];
                    break;

                case InputSides.Left:
                    left = function.Parameters[0];
                    break;

                case InputSides.Right:
                    right = function.Parameters[0];
                    break;
            }

            OperatorTypeSignature typeSignature 
                = new OperatorTypeSignature(left, right, inputs);

            OperatorSignature signature = new OperatorSignature(op.Name, typeSignature);

            _operators.Add(signature, function);
        }
    }
}
