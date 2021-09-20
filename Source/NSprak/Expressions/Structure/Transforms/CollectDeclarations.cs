using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Expressions.Types;
using NSprak.Functions;
using NSprak.Functions.Signatures;
using NSprak.Language;
using NSprak.Messaging;

namespace NSprak.Expressions.Structure.Transforms
{
    public class CollectDeclarations : ITreeTransform
    {
        public void Apply(Block root, CompilationEnvironment environment)
        {
            Dictionary<FunctionSignature, FunctionInfo> functions 
                = new Dictionary<FunctionSignature, FunctionInfo>();

            UpdateBlock(root, environment.Messages, functions);

            environment.SignatureLookup.SpecifyUserFunctions(functions);
        }

        private void UpdateBlock(Block block, 
            Messenger messenger, Dictionary<FunctionSignature, FunctionInfo> functions)
        {
            if (block.ScopeHint == null)
                block.ScopeHint = new Scope();

            if (block.Header is IfHeader ifHeader)
                ApplyCombinedScope(ifHeader);

            Scope scope = block.ScopeHint;

            // Three steps

            // First: check if the header declares any variables - function arguments, for example

            switch (block.Header)
            {
                case FunctionHeader function:

                    for (int i = 0; i < function.ParameterCount; i++)
                    {
                        string name = function.ParameterNames[i];
                        SprakType type = function.ParameterTypes[i];
                        scope.VariableDeclarations
                            .Add(name, new VariableInfo(type, -1, block));
                    }

                    break;

                case LoopHeader loop when loop.HasName:

                    // we need an "undetermined" type here
                    scope.VariableDeclarations
                        .Add(loop.Name, new VariableInfo(SprakType.Any, -1, block));

                    break;
            }

            // Second: check if variables or functions have been declared in the statements of the block.
            // If there is a subblock, apply this update to that block as well.

            // Note that a subblock can a function declaration, but any other declarations in that subblock belong to its hints,
            // not the the ones for the current block.

            foreach (Expression expression in block.Statements)
            {
                switch (expression)
                {
                    case VariableAssignment assignment:

                        if (assignment.IsDeclaration)
                        {
                            if (scope.VariableDeclarations.ContainsKey(assignment.Name))
                                messenger.AtToken(assignment.NameToken, 
                                    Messages.DuplicateVariable, assignment.Name);

                            else scope.VariableDeclarations.Add(assignment.Name, 
                                new VariableInfo(assignment.DeclarationType, 
                                assignment.EndToken.End, block));
                        }

                        break;

                    case Block subBlock:

                        if (subBlock.Header is FunctionHeader function)
                        {
                            if (functions.ContainsKey(function.Signature))
                                messenger.AtToken(function.NameToken, 
                                    Messages.DuplicateFunction, function.Name);

                            else functions.Add(function.Signature, 
                                new FunctionInfo(function));
                        }

                        UpdateBlock(subBlock, messenger, functions);

                        break;
                }
            }

            block.ScopeHint = scope;
        }

        private void ApplyCombinedScope(IfHeader ifHeader)
        {
            if (ifHeader.NextConditionalComponentHint == null)
                return;

            Scope scope = ifHeader.ParentBlockHint.ScopeHint;

            IConditionalSubComponent current = ifHeader.NextConditionalComponentHint;
            while (current != null)
            {
                ((Expression)current).ParentBlockHint.ScopeHint = scope;
                current = current.NextConditionalComponentHint;
            }
        }
    }
}
