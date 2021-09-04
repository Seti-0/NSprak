using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Expressions.Types;
using NSprak.Language;
using NSprak.Language.Builtins;

namespace NSprak.Expressions.Structure.Transforms
{
    public class ResolveTypesAndSignatures : ITreeTransform
    {
        public void Apply(Block root, CompilationEnvironment environment)
        {
            Update(root, environment.SignatureLookup);
        }

        private void Update(IEnumerable<Expression> expressions, SignatureResolver resolver)
        {
            foreach (Expression expression in expressions)
                Update(expression, resolver);
        }

        private void Update(Expression expression, SignatureResolver resolver)
        {
            // Type hints can be dependent on child expressions.
            // (They are indie of parents, and neighbours)
            Update(expression.GetSubExpressions(), resolver);

            switch (expression)
            {
                case FunctionCall function:
                    ResolveCallAndTypeHint(function, resolver);
                    break;

                case LiteralArrayGet array:
                    array.TypeHint = SprakType.Array;
                    break;

                case LiteralGet literal:
                    literal.TypeHint = literal.Value.Type;
                    break;

                case OperatorCall op:
                    ResolveCallAndTypeHint(op, resolver);
                    break;

                case Return ret:

                    if (ret.HasValue) 
                        ret.TypeHint = ret.Value.TypeHint;
                    
                    break;

                case VariableAssignment assignment:
                    ResolveAssignmentOperator(assignment, resolver);
                    break;

                case VariableReference variable:

                    if (variable.ParentBlockHint.TryGetVariableInfo(variable.Name, out VariableInfo result))
                        variable.TypeHint = result.DeclaredType;

                    else
                    {
                        //variable.RaiseError(environment.Messages, $"Unable to find variable: {variable.Name}");
                        variable.TypeHint = null;
                    }

                    break;

            }
        }

        private void ResolveCallAndTypeHint(FunctionCall call, SignatureResolver resolver)
        {
            call.TypeHint = null;

            if (call.Arguments.Any(x => x.TypeHint == null))
                // At some point we'll want to search for possible overloads 
                return;

            FunctionTypeSignature typeSignature = new FunctionTypeSignature(
                call.Arguments.Select(x => x.TypeHint).ToArray());

            string name = call.Name;

            SignatureLookupResult lookupResult;
            lookupResult = resolver.TryFindMatch(name, typeSignature);

            if (lookupResult.Success)
            {
                if (lookupResult.BuiltInFunction != null)
                {
                    call.BuiltInFunctionHint = lookupResult.BuiltInFunction;
                    call.NameToken.Hints |= Tokens.TokenHints.BuiltInFunction;
                }
                else
                {
                    call.UserFunctionHint = lookupResult.FunctionInfo.Signature;
                    call.NameToken.Hints |= Tokens.TokenHints.UserFunction;
                }

                call.TypeHint = lookupResult.FunctionInfo?.ReturnType;
            }
        }

        private void ResolveCallAndTypeHint(OperatorCall call, SignatureResolver resolver)
        {
            call.TypeHint = null;

            if (call.LeftInput != null && call.LeftInput.TypeHint == null)
                return;

            if (call.RightInput != null && call.RightInput.TypeHint == null)
                return;

            OperatorTypeSignature signature
                = new OperatorTypeSignature(call.LeftInput?.TypeHint, call.RightInput?.TypeHint);

            SignatureLookupResult lookupResult;
            lookupResult = resolver.TryFindMatch(call.Operator.Name, signature);

            if (lookupResult.Success)
            {
                call.BuiltInFunctionHint = lookupResult.BuiltInFunction;
                call.TypeHint = lookupResult.FunctionInfo?.ReturnType;
            }
        }

        private void ResolveAssignmentOperator(VariableAssignment call, SignatureResolver resolver)
        {
            if (call.Value != null && call.Value.TypeHint == null)
                return;

            bool both = call.Operator.Inputs == OperatorSide.Both;
            bool requiresLeft = both || call.Operator.Inputs == OperatorSide.Left;
            bool requiresRight = both || call.Operator.Inputs == OperatorSide.Right;

            SprakType left;

            if (!requiresLeft)
                left = null;

            else if (call.IsDeclaration)
                left = call.DeclarationType;

            else if (call.ParentBlockHint.TryGetVariableInfo(call.Name, out VariableInfo info))
                left = info.DeclaredType;

            else return;

            SprakType right = null;

            if (requiresRight)
            {
                right = call.Value?.TypeHint;
                if (right == null) return;
            }

            OperatorTypeSignature signature
                = new OperatorTypeSignature(left, right);

            string name = call.Operator.Name;

            SignatureLookupResult lookupResult = resolver.TryFindMatch(name, signature);

            if (lookupResult.Success)
            {
                call.BuiltInFunctionHint = lookupResult.BuiltInFunction;
                call.OpHint = lookupResult.OpBuilder;
            }
        }
    }
}
