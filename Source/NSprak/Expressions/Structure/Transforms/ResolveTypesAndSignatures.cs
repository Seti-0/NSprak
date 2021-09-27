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

                case Group group:
                    group.TypeHint = group.Value.TypeHint;
                    break;

                case LiteralArrayGet array:
                    array.TypeHint = SprakType.Array;
                    break;

                case Indexer indexer:

                    if (!(indexer.SourceExpression is VariableReference))
                        env.Messages.AtExpression(indexer.SourceExpression,
                            Messages.IndexerNotSupported);

                    SprakType sourceType = indexer.SourceExpression.TypeHint;

                    // The source should be an array
                    if (sourceType != null && sourceType != SprakType.Array && sourceType != SprakType.Any)
                        env.Messages.AtExpression(indexer.SourceExpression,
                            Messages.CanOnlyIndexArrays, sourceType.Text);

                    // The index should be a number
                    SprakType indexType = indexer.IndexExpression.TypeHint;
                    if (indexType != null && indexType != SprakType.Number && indexType != SprakType.Any)
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

                    Block parent = ret.ParentBlockHint;
                    while (parent != null && !(parent.Header is FunctionHeader))
                        parent = parent.ParentBlockHint;

                    if (parent == null)
                        env.Messages.AtToken(ret.ReturnToken, 
                            Messages.ReturnOutsideFunction);

                    else
                    {
                        SprakType src = ret.TypeHint;
                        SprakType dst = ((FunctionHeader)parent.Header).ReturnType;

                        if (ret.HasValue)
                        {
                            if (dst == SprakType.Unit)
                                env.Messages.AtExpression(ret.Value,
                                    Messages.ReturnValueFromVoid);

                            else if (src != null)
                            {
                                if (!env.AssignmentLookup.IsAssignable(src, dst))
                                    env.Messages.AtExpression(ret.Value,
                                        Messages.IncompatibleReturnValue, 
                                        src.Text, dst.Text);
                            }
                        }
                        else
                        {
                            if (dst != SprakType.Unit)
                                env.Messages.AtToken(ret.ReturnToken,
                                    Messages.MissingReturnValue);
                        }
                    }

                    break;

                case VariableAssignment assignment:
                    ResolveAssignment(assignment, env);
                    break;

                case VariableReference variable:

                    if (variable.ParentBlockHint.TryGetVariableInfo(variable.Name, out VariableInfo info))
                    {
                        Block refAncestor = variable.ParentBlockHint;
                        while (refAncestor != null && refAncestor != info.Source)
                            refAncestor = refAncestor.ParentBlockHint;

                        if (refAncestor == null)
                            env.Messages.AtToken(variable.Token,
                                Messages.VariableFromDisconnectedBlock);

                        else if (variable.StartToken.Start < info.DeclarationEnd)
                        {
                            env.Messages.AtToken(variable.Token,
                                Messages.ReferenceBeforeDefinition, variable.Name);
                        }

                        variable.TypeHint = info.DeclaredType;
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
                string signature = $"{name}({typeSignature})";
                env.Messages.AtToken(call.NameToken, 
                    Messages.UnresolvedCall, signature);
            }
        }

        private void ResolveCallAndTypeHint(OperatorCall call, CompilationEnvironment env)
        {
            if (!Operator.TryParse(out Operator op, text: call.OperatorText))
            {
                env.Messages.AtToken(call.OperatorToken,
                    Messages.UnrecognizedOperator, call.OperatorText);
                return;
            }

            if (op.IsAssignment)
            {
                env.Messages.AtToken(call.OperatorToken, 
                    Messages.IncorrectUseOfAssignment, op.Text);
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
                .TryFindMatch(op.Name, signature);

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

        private void ResolveAssignment(VariableAssignment assignment, CompilationEnvironment env)
        {
            if (assignment.Indices.Count > 1)
            {
                env.Messages.AtExpression(assignment.Indices.Last().Index,
                    Messages.MultipleIndices);
            }

            if (!assignment.ParentBlockHint
                .TryGetVariableInfo(assignment.Name, out VariableInfo nameInfo))
            {
                env.Messages.AtToken(assignment.NameToken,
                    Messages.UnrecognizedName, assignment.NameToken.Content);
                return;
            }

            if (!assignment.IsDeclaration)
            {
                Block ancestor = assignment.ParentBlockHint;
                while (ancestor != null && ancestor != nameInfo.Source)
                    ancestor = ancestor.ParentBlockHint;

                if (ancestor == null)
                    env.Messages.AtToken(assignment.NameToken,
                        Messages.VariableFromDisconnectedBlock);
            }

            if (nameInfo.DeclaredType == null)
                // I don't think this should be possible, but this is all
                // very convoluted. I wish C# was more nullable-aware.
                throw new Exception("Declaration with Declaration Type");

            SprakType dstType;

            if (assignment.Indices.Count == 0)
                dstType = nameInfo.DeclaredType;

            else
            {
                if (nameInfo.DeclaredType != SprakType.Array)
                {
                    env.Messages.AtToken(assignment.NameToken,
                        Messages.CanOnlyIndexArrays, nameInfo.DeclaredType);
                }

                // Best we can do with array assignments until generic
                // type tracking is introduced.
                dstType = SprakType.Any;
            }

            SprakType srcType;

            if (assignment.Value == null)
                srcType = nameInfo.DeclaredType;

            else if (assignment.Value.TypeHint != null)
                srcType = assignment.Value.TypeHint;

            else
                srcType = SprakType.Any;

            if (assignment.IsDeclaration)
            {
                if (assignment.Operator != Operator.Set)
                {
                    env.Messages.AtToken(assignment.OperatorToken,
                        Messages.InvalidDeclarationOperator);
                }

                if (assignment.Indices.Count > 0)
                {
                    env.Messages.AtExpression(assignment.Indices.First().Index,
                        Messages.InvalidIndexDeclaration);
                }
            }
            else if (!assignment.Operator.IsAssignment)
            {
                env.Messages.AtToken(assignment.OperatorToken, 
                    Messages.ExpectedAssignmentOperator, assignment.Operator.Text);
                return;
            }
            else if (assignment.Operator.AssignmentOperation != null)
            {
                // I doubt I'll bother implementing right-only inputs.
                // (Like --i) They don't show up much.

                InputSides inputs;
                SprakType left;
                SprakType right;

                if (!assignment.HasValue)
                {
                    inputs = InputSides.Left;
                    left = srcType;
                    right = null;
                }
                else
                {
                    inputs = InputSides.Both;
                    left = srcType;
                    right = dstType;
                }

                // We need the name of the function to called before assignment.
                // That may or may not be the same name as that of the operator.
                string name = assignment.Operator.AssignmentOperation;

                OperatorTypeSignature signature
                    = new OperatorTypeSignature(left, right, inputs);

                SignatureLookupResult lookupResult = env.SignatureLookup
                    .TryFindMatch(name, signature);

                if (lookupResult.Success)
                {
                    assignment.BuiltInFunctionHint = lookupResult.BuiltInFunction;
                    assignment.OpHint = lookupResult.OpBuilder;
                    srcType = lookupResult.BuiltInFunction.ReturnType;
                }
                else
                {
                    string operation = assignment.ToString();
                    env.Messages.AtExpression(assignment,
                        Messages.UnresolvedOperation, operation);

                    srcType = SprakType.Any;
                }
            }

            if (!env.AssignmentLookup.IsAssignable(srcType, dstType))
            {
                env.Messages.AtExpression(
                    assignment, Messages.AssignmentTypeMismatch, srcType, dstType);
            }
        }
    }
}
