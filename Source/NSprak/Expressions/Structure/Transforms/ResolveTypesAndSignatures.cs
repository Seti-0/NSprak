using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Expressions.Types;
using NSprak.Language;
using NSprak.Language.Builtins;
using NSprak.Messaging;

namespace NSprak.Expressions.Structure.Transforms
{
    public class ResolveTypesAndSignatures : ITreeTransform
    {
        public void Apply(Block root, CompilationEnvironment environment)
        {
            Update(root, environment);
        }

        private void Update(IEnumerable<Expression> expressions, CompilationEnvironment env)
        {
            foreach (Expression expression in expressions)
                Update(expression, env);
        }

        private void Update(Expression expression, CompilationEnvironment env)
        {
            // Type hints can be dependent on child expressions.
            // (They are indie of parents, and neighbours)
            Update(expression.GetSubExpressions(), env);

            switch (expression)
            {
                case FunctionCall function:
                    ResolveCallAndTypeHint(function, env);
                    break;

                case LiteralArrayGet array:
                    array.TypeHint = SprakType.Array;
                    break;

                case LiteralGet literal:
                    literal.TypeHint = literal.Value.Type;
                    break;

                case OperatorCall op:
                    ResolveCallAndTypeHint(op, env);
                    break;

                case Return ret:

                    if (ret.HasValue) 
                        ret.TypeHint = ret.Value.TypeHint;
                    
                    break;

                case VariableAssignment assignment:
                    ResolveAssignmentOperator(assignment, env);
                    break;

                case VariableReference variable:

                    if (variable.ParentBlockHint.TryGetVariableInfo(variable.Name, out VariableInfo result))
                    {
                        if (variable.StartToken.Start < result.DeclarationEnd)
                        {
                            env.Messages.AtToken(variable.Token,
                                Messages.ReferenceBeforeDefinition, variable.Name);
                        }

                        variable.TypeHint = result.DeclaredType;
                    }   
                    else
                    {
                        env.Messages.AtToken(variable.Token, 
                            Messages.UnrecognizedName, variable.Name);
                        variable.TypeHint = null;
                    }

                    break;

            }
        }

        private void ResolveCallAndTypeHint(FunctionCall call, CompilationEnvironment env)
        {
            call.TypeHint = null;

            if (call.Arguments.Any(x => x.TypeHint == null))
                // At some point we'll want to search for possible overloads 
                return;

            FunctionTypeSignature typeSignature = new FunctionTypeSignature(
                call.Arguments.Select(x => x.TypeHint).ToArray());

            string name = call.Name;

            SignatureLookupResult lookupResult;
            lookupResult = env.SignatureLookup
                .TryFindMatch(name, typeSignature);

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
            else
            {
                string signature = name + typeSignature.ToString();
                env.Messages.AtToken(call.NameToken, 
                    Messages.UnresolvedCall, signature);
            }
        }

        private void ResolveCallAndTypeHint(OperatorCall call, CompilationEnvironment env)
        {
            call.TypeHint = null;

            if (call.LeftInput != null && call.LeftInput.TypeHint == null)
                return;

            if (call.RightInput != null && call.RightInput.TypeHint == null)
                return;

            OperatorTypeSignature signature
                = new OperatorTypeSignature(call.LeftInput?.TypeHint, call.RightInput?.TypeHint);

            SignatureLookupResult lookupResult;
            lookupResult = env.SignatureLookup
                .TryFindMatch(call.Operator.Name, signature);

            if (lookupResult.Success)
            {
                call.BuiltInFunctionHint = lookupResult.BuiltInFunction;
                call.TypeHint = lookupResult.FunctionInfo?.ReturnType;
            }
            else
            {
                string operation = $"({call.LeftInput.TypeHint.Text})"
                    + $" {call.OperatorToken.Content}"
                    + $" ({call.RightInput.TypeHint.Text})";

                env.Messages.AtToken(call.OperatorToken, 
                    Messages.UnresolvedOperation, operation);
            }
        }

        private void ResolveAssignmentOperator(VariableAssignment call, CompilationEnvironment env)
        {
            if (call.Value != null && call.Value.TypeHint == null)
                return;

            if (!call.ParentBlockHint
                .TryGetVariableInfo(call.Name, out VariableInfo nameInfo))
            {
                env.Messages.AtToken(call.NameToken,
                    Messages.UnrecognizedName, call.NameToken.Content);
                return;
            }

            bool both = call.Operator.Inputs == OperatorSide.Both;
            bool requiresLeft = both || call.Operator.Inputs == OperatorSide.Left;
            bool requiresRight = both || call.Operator.Inputs == OperatorSide.Right;

            SprakType left;

            if (!requiresLeft)
                left = null;

            else if (call.IsDeclaration)
                left = call.DeclarationType;

            else
                left = nameInfo.DeclaredType;

            SprakType right = null;

            if (requiresRight)
            {
                if (!requiresLeft)
                    right = nameInfo.DeclaredType;
                else
                {
                    right = call.Value?.TypeHint;
                    if (right == null) return;
                }
            }

            OperatorTypeSignature signature
                = new OperatorTypeSignature(left, right);

            string name = call.Operator.Name;

            SignatureLookupResult lookupResult = env.SignatureLookup
                .TryFindMatch(name, signature);

            if (lookupResult.Success)
            {
                call.BuiltInFunctionHint = lookupResult.BuiltInFunction;
                call.OpHint = lookupResult.OpBuilder;
            }
            else
            {
                string operation = call.ToString();
                env.Messages.AtExpression(call,
                    Messages.UnresolvedOperation, operation);
            }
        }
    }
}
