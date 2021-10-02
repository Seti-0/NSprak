using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Operations.Types;
using NSprak.Expressions;
using NSprak.Expressions.Types;
using NSprak.Execution;
using NSprak.Operations.Creation;
using NSprak.Functions.Signatures;
using NSprak.Functions;

namespace NSprak.Operations
{
    public class CodeGenerator
    {
        public static Executable Create(ExpressionTree source, 
            Dictionary<FunctionSignature, FunctionInfo> userDeclarations)
        {
            GeneratorContext builder = new GeneratorContext();

            List<Expression> mainStatements = new List<Expression>();
            List<Block> functions = new List<Block>();

            foreach (Expression statement in source.Root.Statements)
            {
                if (statement is Block block && block.Header is FunctionHeader)
                    functions.Add(block);

                else mainStatements.Add(statement);
            }

            if (mainStatements.Count == 0 && functions.Count == 0)
                builder.AddComment("Empty executable");

            if (functions.Count > 0)
                builder.AddComment("Begin main function");

            foreach (Expression statement in mainStatements)
                builder.AddCode(statement);

            if (functions.Count > 0)
            {
                builder.AddOp(new Exit());
                builder.AddComment("End main function");
            }

            foreach (Block function in functions)
            {
                FunctionHeader header = function.Header as FunctionHeader;
                builder.AddEntryPoint(header.Signature, builder.InstructionCount);
                builder.AddCode(function);
            }

            return builder.GetResult(userDeclarations);
        }
    }
}
