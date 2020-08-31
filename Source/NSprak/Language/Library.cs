using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Markup;
using Microsoft.VisualBasic;
using NSprak.Expressions.Types;
using NSprak.Language.Builtins;
using NSprak.Language.Libraries;

namespace NSprak.Language
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

        public SprakOperatorAttribute(string opName)
        {
            OperatorName = opName;
        }
    }

    public class Library
    {
        public static Library Core = new Library("Core",
            typeof(Core), typeof(CoreConversions), typeof(CoreOperators));

        public string Name { get; }

        public string UniqueID => Name;

        private Dictionary<FunctionSignature, BuiltInFunction> _functions = new Dictionary<FunctionSignature, BuiltInFunction>();
        private Dictionary<OperatorSignature, BuiltInFunction> _operators = new Dictionary<OperatorSignature, BuiltInFunction>();
        private Dictionary<ConversionTypeSignature, BuiltInFunction> _conversions = new Dictionary<ConversionTypeSignature, BuiltInFunction>();

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
                AddOperator(methodInfo, opAttribute.OperatorName);

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

        private void AddOperator(MethodInfo methodInfo, string opName)
        {
            BuiltInFunction function = new BuiltInFunction(methodInfo, this);

            if (!Operator.TryParse(out Operator op, name: opName))
            {
                string message = $"{MethodName(methodInfo)} is declared to be an unrecognized operator: \"{opName}\"";
                throw new ArgumentException(message);
            }

            SprakType left = null;
            SprakType right = null;

            bool binary = op.Inputs == OperatorSide.Both;
            int requiredParams = binary ? 2 : 1;

            if (function.Parameters.Count != requiredParams)
            {
                string desc = binary ? "binary" : "unary";

                string message = $"{MethodName(methodInfo)} was declared to be the {desc} operator {opName}, " +
                    $"but has {function.Parameters.Count} Sprak arguments";

                throw new ArgumentException(message);
            }

            switch (op.Inputs)
            {
                case OperatorSide.Both:
                    left = function.Parameters[0];
                    right = function.Parameters[1];
                    break;

                case OperatorSide.Left:
                    left = function.Parameters[0];
                    break;

                case OperatorSide.Right:
                    right = function.Parameters[0];
                    break;
            }

            OperatorTypeSignature typeSignature = new OperatorTypeSignature(left, right);
            OperatorSignature signature = new OperatorSignature(op.Name, typeSignature);

            _operators.Add(signature, function);
        }
    }
}
