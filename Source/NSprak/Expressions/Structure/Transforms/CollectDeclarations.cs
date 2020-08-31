using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Expressions.Types;
using NSprak.Language;
using NSprak.Language.Builtins;
using NSprak.Messaging;

namespace NSprak.Expressions.Structure.Transforms
{
    public class CollectDeclarations : ITreeTransform
    {
        public void Apply(Block root, CompilationEnvironment environment)
        {
            Dictionary<FunctionSignature, FunctionInfo> functions = new Dictionary<FunctionSignature, FunctionInfo>();

            int offset = 0;
            UpdateBlock(root, ref offset, environment.Messages, functions);

            environment.SignatureLookup.SpecifyUserFunctions(functions);
        }

        private void UpdateBlock(Block block, ref int offset, MessageCollection messenger, Dictionary<FunctionSignature, FunctionInfo> functions)
        {
            Dictionary<string, VariableInfo> variables = new Dictionary<string, VariableInfo>();

            // Two steps

            // First: check if the header declares any variables - function arguments, for example

            switch (block.Header)
            {
                case FunctionHeader function:

                    for (int i = 0; i < function.ParameterCount; i++)
                    {
                        string name = function.ParameterNames[i];
                        SprakType type = function.ParameterTypes[i];
                        variables.Add(name, new VariableInfo(type, -1));
                    }

                    break;

                case LoopHeader loop when loop.HasName:

                    // we need an "undetermined" type here
                    variables.Add(loop.Name, new VariableInfo(SprakType.Any, -1));

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
                            if (variables.ContainsKey(assignment.Name))
                                assignment.RaiseError(messenger, "A variable with this name has already been declared in this scope");

                            else variables.Add(assignment.Name, new VariableInfo(assignment.DeclarationType, offset));
                        }

                        break;

                    case Block subBlock:

                        if (subBlock.Header is FunctionHeader function)
                        {
                            if (functions.ContainsKey(function.Signature))
                                function.RaiseError(messenger, "A function with signature has already been declared in this scope");

                            else functions.Add(function.Signature, new FunctionInfo(function));
                        }

                        UpdateBlock(subBlock, ref offset, messenger, functions);

                        break;
                }

                offset++;
            }

            block.VariableDeclarationsHint = variables;
        }
    }
}
