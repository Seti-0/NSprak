using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using Microsoft.VisualBasic;
using NSprak.Functions.Resolution.OpBindings;
using NSprak.Functions.Signatures;
using NSprak.Operations;

namespace NSprak.Functions.Resolution
{
    public class SignatureResolver
    {
        private AssignmentResolver _assignmentResolver;
        private Dictionary<FunctionSignature, FunctionInfo> _userDeclarations;

        private TwoStepLookup<FunctionTypeSignature, FunctionInfo> _userFunctionsLookup;
        private TwoStepLookup<FunctionTypeSignature, Func<Op>> _functionBindingLookup;
        private TwoStepLookup<FunctionTypeSignature, BuiltInFunction> _builtinFunctionsLookup;

        private TwoStepLookup<OperatorTypeSignature, Func<Op>> _operatorBindingLookup;
        private TwoStepLookup<OperatorTypeSignature, BuiltInFunction> _builtInOperatorLookup;

        private Dictionary<ConversionTypeSignature, Func<Op>> _conversionBindingLookup;
        private Dictionary<ConversionTypeSignature, BuiltInFunction> _builtinConversionLookup;

        public Dictionary<FunctionSignature, FunctionInfo> UserDeclarations
            => _userDeclarations;

        public SignatureResolver(List<Library> libraries, AssignmentResolver assignmentLookup)
        {
            SpecifyAssignmentResolver(assignmentLookup);
            SpecifyLibraries(libraries);
        }

        private void SpecifyAssignmentResolver(AssignmentResolver resolver)
        {
            _assignmentResolver = resolver;
        }

        private void SpecifyLibraries(List<Library> libraries)
        {
            CheckForDuplicates(libraries.SelectMany(x => x.Functions), x => x.Key);
            CheckForDuplicates(libraries.SelectMany(x => x.Operators), x => x.Key);
            CheckForDuplicates(libraries.SelectMany(x => x.Conversions), x => x.Key);

            _builtinFunctionsLookup = libraries
                .SelectMany(x => x.Functions.Values)
                .ToLookup(x => x.Name, x => x.Signature.TypeSignature);

            _builtInOperatorLookup = libraries
                .SelectMany(x => x.Operators)
                .ToLookup(x => x.Key.Name, x => x.Key.TypeSignature, x => x.Value);

            _builtinConversionLookup = libraries
                .SelectMany(x => x.Conversions)
                .ToDictionary(x => x.Key, x => x.Value);
        }

        public void SpecifyOperationBindings(List<OperationBinding> opBindings)
        {
            List<FunctionOpBinding> functions = new List<FunctionOpBinding>();
            List<OperatorOpBinding> operators = new List<OperatorOpBinding>();
            List<ConversionOpBinding> conversions = new List<ConversionOpBinding>();

            foreach (OperationBinding binding in opBindings)
                switch (binding)
                {
                    case FunctionOpBinding function: functions.Add(function); break;
                    case OperatorOpBinding op: operators.Add(op); break;
                    case ConversionOpBinding conversion: conversions.Add(conversion); break;
                }

            CheckForDuplicates(functions, x => x.Signature);
            CheckForDuplicates(operators, x => x.Signature);
            CheckForDuplicates(conversions, x => x.Signature);

            _functionBindingLookup = functions
                .ToLookup(x => x.Signature.Name, x => x.Signature.TypeSignature, x => x.Builder);

            _operatorBindingLookup = operators
                .ToLookup(x => x.Signature.Name, x => x.Signature.TypeSignature, x => x.Builder);

            _conversionBindingLookup = conversions
                .ToDictionary(x => x.Signature, x => x.Builder);
        }

        public void SpecifyUserFunctions(Dictionary<FunctionSignature, FunctionInfo> userDeclarations)
        {
            _userDeclarations = userDeclarations;
            _userFunctionsLookup = userDeclarations
                .ToLookup(x => x.Key.Name, x => x.Key.TypeSignature, x => x.Value);
        }

        private void CheckForDuplicates<TSource, TKey>(IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            IEnumerable<IGrouping<TKey, TSource>> groups;

            groups = source
                .GroupBy(keySelector);

            IEnumerable<IGrouping<TKey, TSource>> duplicates;

            duplicates = groups
                .Where(x => x.Count() > 1);

            // This should be replaced by a warning at some point (and a descriptive one)
            if (duplicates.Any())
            {
                throw new Exception("Duplicate function found");
            }
        }

        public SignatureLookupResult TryFindMatch(ConversionTypeSignature signature)
        {
            SignatureLookupResult result = new SignatureLookupResult();

            if (_conversionBindingLookup.TryGetValue(signature, out Func<Op> value))
            {
                result.Success = true;
                result.OpBuilder = value;
            }
            else if (_builtinConversionLookup.TryGetValue(signature, out BuiltInFunction builtIn))
            {
                result.Success = true;
                result.BuiltInFunction = builtIn;
                result.FunctionInfo = builtIn.Info;
            }

            return result;
        }

        public IEnumerable<FunctionInfo> FindFunctions(string name)
        {
            return _builtinFunctionsLookup
                .GetValues(name)
                .Select(x => x.Info)
                .Concat(_userFunctionsLookup.GetValues(name));
        }

        public SignatureLookupResult TryFindMatch(string name, FunctionTypeSignature typeSignature)
        {
            SignatureLookupResult result = new SignatureLookupResult();
            var checks = GetChecks(typeSignature);

            Dictionary<FunctionTypeSignature, FunctionInfo> userOptions;
            if (_userFunctionsLookup.TryGetOptions(name, out userOptions))
            {
                var searchResult = SignatureSearcher.Search(userOptions, checks, x => x);
                if (searchResult.Success)
                {
                    result.Success = true;
                    result.Ambiguous = searchResult.Ambiguous;
                    result.FunctionInfo = searchResult.Value;

                    return result;
                }
            }

            Dictionary<FunctionTypeSignature, Func<Op>> opOptions;
            if (_functionBindingLookup.TryGetOptions(name, out opOptions))
            {
                var searchResult = SignatureSearcher.Search(opOptions, checks, x => x);
                if (searchResult.Success)
                {
                    result.Success = true;
                    result.Ambiguous = searchResult.Ambiguous;
                    result.FunctionInfo = new FunctionInfo(new FunctionSignature(null, null, searchResult.Key), null);
                    result.OpBuilder = searchResult.Value;

                    return result;
                }
            }

            Dictionary<FunctionTypeSignature, BuiltInFunction> builtinOptions;
            if (_builtinFunctionsLookup.TryGetOptions(name, out builtinOptions))
            {
                var searchResult = SignatureSearcher.Search(builtinOptions, checks, x => x);
                if (searchResult.Success)
                {
                    result.Success = true;
                    result.Ambiguous = searchResult.Ambiguous;
                    result.FunctionInfo = searchResult.Value.Info;
                    result.BuiltInFunction = searchResult.Value;

                    return result;
                }
            }

            return result;
        }

        public SignatureLookupResult TryFindMatch(string name, OperatorTypeSignature typeSignature)
        {
            SignatureLookupResult result = new SignatureLookupResult();
            var checks = GetChecks(typeSignature);

            Dictionary<OperatorTypeSignature, Func<Op>> opOptions;
            if (_operatorBindingLookup.TryGetOptions(name, out opOptions))
            {
                var searchResult = SignatureSearcher.Search(opOptions, checks, x => x);
                if (searchResult.Success)
                {
                    result.Success = true;
                    result.Ambiguous = searchResult.Ambiguous;
                    result.OpBuilder = searchResult.Value;

                    return result;
                }
            }

            Dictionary<OperatorTypeSignature, BuiltInFunction> builtinOptions;
            if (_builtInOperatorLookup.TryGetOptions(name, out builtinOptions))
            {
                var searchResult = SignatureSearcher.Search(builtinOptions, checks, x => x);
                if (searchResult.Success)
                {
                    result.Success = true;
                    result.Ambiguous = searchResult.Ambiguous;
                    result.FunctionInfo = searchResult.Value.Info;
                    result.BuiltInFunction = searchResult.Value;

                    return result;
                }
            }

            return result;
        }

        private SignatureSearcher.Checks<FunctionTypeSignature> GetChecks(FunctionTypeSignature target)
        {
            return new SignatureSearcher.Checks<FunctionTypeSignature>
            {
                Matches = (option) => option.CanAccept(target, _assignmentResolver),
                ExactComparison = CheckExact(target),
                LosslessComparison = CheckLossless(target)
            };
        }

        private SignatureSearcher.Checks<OperatorTypeSignature> GetChecks(OperatorTypeSignature target)
        {
            return new SignatureSearcher.Checks<OperatorTypeSignature>
            {
                Matches = (option) => option.CanAccept(target, _assignmentResolver),
                ExactComparison = CheckExact(target),
                LosslessComparison = CheckLossless(target)
            };
        }

        private Func<FunctionTypeSignature, int> CheckExact(FunctionTypeSignature target)
        {
            int KeyFunction(FunctionTypeSignature subject)
            {
                int key = 0; ;

                for (int i = 0; i < target.Parameters.Count; i++)
                {
                    if (subject.Parameters[i] != target.Parameters[i])
                        key++;
                }

                return key;
            }

            return KeyFunction;
        }

        private Func<OperatorTypeSignature, int> CheckExact(OperatorTypeSignature target)
        {
            int KeyFunction(OperatorTypeSignature subject)
            {
                int key = 0; ;

                if (target.LeftParam != subject.LeftParam)
                    key++;

                if (target.RightParam != subject.RightParam)
                    key++;

                return key;
            }

            return KeyFunction;
        }


        private Func<FunctionTypeSignature, int> CheckLossless(FunctionTypeSignature target)
        {
            int KeyFunction(FunctionTypeSignature subject)
            {
                int key = 0; ;

                for (int i = 0; i < target.Parameters.Count; i++)
                {
                    if (!_assignmentResolver.IsPerfect(target.Parameters[i], subject.Parameters[i]))
                        key++;
                }

                return key;
            }

            return KeyFunction;
        }

        private Func<OperatorTypeSignature, int> CheckLossless(OperatorTypeSignature target)
        {
            int KeyFunction(OperatorTypeSignature subject)
            {
                int key = 0; ;

                if (!_assignmentResolver.IsPerfect(target.LeftParam, subject.LeftParam))
                    key++;

                if (!_assignmentResolver.IsPerfect(target.RightParam, subject.RightParam))
                    key++;

                return key;
            }

            return KeyFunction;
        }
    }
}
