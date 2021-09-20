using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NSprak.Expressions.Types;
using NSprak.Functions.Resolution;
using NSprak.Functions.Signatures;
using NSprak.Language;
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

                case Indexer indexer:

                    SprakType sourceType = indexer.SourceExpression.TypeHint;

                    // The source should be an array
                    if (sourceType != null && sourceType != SprakType.Array && sourceType != SprakType.Any)
                        env.Messages.AtExpression(indexer.SourceExpression,
                            Messages.CanOnlyIndexArrays, sourceType.Text);

                    // The index should be a number
                    SprakType indexType = indexer.IndexExpression.TypeHint;
                    if (indexType != null && indexType != SprakType.Number && sourceType != SprakType.Any)
                        env.Messages.AtExpression(indexer.IndexExpression,
                            Messages.IndexShouldBeNumber, indexType.Text);

                    // This is unfortunate, and to be worked on
                    // later.
                    indexer.TypeHint = SprakType.Any;

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
            if (call.Operator.IsAssignment)
            {
                env.Messages.AtToken(call.OperatorToken, 
                    Messages.IncorrectUseOfAssignment, call.Operator.Text);
                return;
            }

            call.TypeHint = null;

            if (call.LeftInput != null && call.LeftInput.TypeHint == null)
                return;

            if (call.RightInput != null && call.RightInput.TypeHint == null)
                return;

            SprakType left = call.LeftInput?.TypeHint;
            SprakType right = call.RightInput?.TypeHint;
            InputSides inputs;

            if (left != null && right != null)
                inputs = InputSides.Both;

            else if (left != null)
                inputs = InputSides.Left;

            else
                inputs = InputSides.Right;

            OperatorTypeSignature signature
                = new OperatorTypeSignature(left, right, inputs);

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
                string operation = $"({call.LeftInput?.TypeHint?.Text})"
                    + $" {call.OperatorToken.Content}"
                    + $" ({call.RightInput?.TypeHint?.Text})";

                env.Messages.AtToken(call.OperatorToken, 
                    Messages.UnresolvedOperation, operation);
            }
        }

        private void ResolveAssignmentOperator(VariableAssignment call, CompilationEnvironment env)
        {
            if (call.IsDeclaration)
                // The name and type were determined in the call constructor,
                // nothing more to determine here.
                return;

            if (!call.Operator.IsAssignment)
            {
                env.Messages.AtToken(call.OperatorToken, 
                    Messages.ExpectedAssignmentOperator, call.Operator.Text);
                return;
            }

            if (call.Operator.AssignmentOperation == null)
                // Straight forward variable set, nothing to resolve here.
                return;

            if (call.Value != null && call.Value.TypeHint == null)
                // No need to raise an error, the value will already have
                // one explaining why it's type could not be determined.
                return;

            // Variable not recognized.
            if (!call.ParentBlockHint
                .TryGetVariableInfo(call.Name, out VariableInfo nameInfo))
            {
                env.Messages.AtToken(call.NameToken,
                    Messages.UnrecognizedName, call.NameToken.Content);
                return;
            }

            if (nameInfo.DeclaredType == null)
                // I don't think this should be possible, but this is all
                // very convoluted. I wish C# was more nullable-aware.
                throw new Exception("Declaration with Declaration Type");

            // Ignoring left assignments for now, they aren't really needed 
            // anyways. (Statements like --i)

            InputSides inputs;

            if (!call.HasValue)
                inputs = InputSides.Left;
            else
                inputs = InputSides.Both;

            SprakType left = nameInfo.DeclaredType;
            SprakType right = call.Value?.TypeHint;

            // We need the name of the function to called before assignment.
            // That may or may not be the same name as that of the operator.
            string name = call.Operator.AssignmentOperation;

            OperatorTypeSignature signature
                = new OperatorTypeSignature(left, right, inputs);

            SignatureLookupResult lookupResult = env.SignatureLookup
                .TryFindMatch(name, signature);

            if (lookupResult.Success)
            {
                SprakType src = lookupResult.BuiltInFunction.ReturnType;
                SprakType dst = nameInfo.DeclaredType;

                if (env.AssignmentLookup.IsAssignable(src, dst))
                {
                    call.BuiltInFunctionHint = lookupResult.BuiltInFunction;
                    call.OpHint = lookupResult.OpBuilder;
                }
                else
                {
                    env.Messages.AtExpression(
                        call, Messages.AssignmentTypeMismatch, src, dst);
                }
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
