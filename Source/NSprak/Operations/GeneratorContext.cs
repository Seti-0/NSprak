﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Microsoft.VisualBasic;
using System.Linq;

using NSprak.Exceptions;
using NSprak.Execution;
using NSprak.Expressions;
using NSprak.Expressions.Types;
using NSprak.Functions;
using NSprak.Functions.Signatures;
using NSprak.Operations.Creation;
using NSprak.Operations.Types;
using NSprak.Tokens;
using NSprak.Tests;

namespace NSprak.Operations
{
    using ReturnE = Expressions.Types.Return;

    public class GeneratorContext
    {
        public List<Op> Operations { get; }
        public List<OpDebugInfo> DebugInfo { get; }

        private readonly Dictionary<FunctionSignature, int> _entryPoints;

        private readonly Dictionary<string, int> _labels;
        private int _indexDepth;

        private readonly Stack<Expression> _sources;

        public Stack<string> BreakLabels { get; }
        public Stack<string> ContinueLabels { get; }

        public int InstructionCount => Operations.Count;

        public GeneratorContext()
        {
            Operations = new List<Op>();
            DebugInfo = new List<OpDebugInfo>();

            _entryPoints = new Dictionary<FunctionSignature, int>();
            _labels = new Dictionary<string, int>();
            _indexDepth = 0;

            _sources = new Stack<Expression>();
            BreakLabels = new Stack<string>();
            ContinueLabels = new Stack<string>();
        }

        public void AddCode(Expression expression)
        {
            expression.OperatorsHint.Clear();

            int startIndex = Operations.Count;

            _sources.Push(expression);

            switch (expression)
            {
                case Command x: Commands.GenerateCode(x, this); break;
                case FunctionCall x: Creation.Functions.GenerateCode(x, this); break;
                case FunctionHeader x: Creation.Functions.GenerateCode(x, this); break;
                case IfHeader x: Conditional.GenerateCode(x, this); break;
                case LiteralArrayGet x: Literals.GenerateCode(x, this); break;
                case LiteralGet x: Literals.GenerateCode(x, this); break;
                case LoopHeader x: Conditional.GenerateCode(x, this); break;
                case OperatorCall x: Creation.Functions.GenerateCode(x, this); break;
                case ReturnE x: Commands.GenerateCode(x, this); break;
                case VariableAssignment x: Variables.GenerateCode(x, this); break;
                case VariableReference x: Variables.GenerateCode(x, this); break;
                case Block block: Blocks.GenerateCode(block, this); break;
                case Indexer indexer: Variables.GenerateCode(indexer, this); break;
                case ElseHeader elseHeader: Conditional.GenerateCode(elseHeader, this); break;
                case ElseIfHeader elseIfHeader: Conditional.GenerateCode(elseIfHeader, this); break;
                case Group group: Literals.GenerateCode(group, this); break;
                case MainHeader _: break;

                case null: throw new ArgumentNullException(nameof(expression));

                default:
                    throw new NotSupportedException(
                        $"Unsupported expression type: {expression.GetType()}");
            }

            _sources.Pop();

            if (startIndex == Operations.Count)
                // No operations have been added, there is no point
                // trying to link test commands
                return;

            int endIndex = Operations.Count - 1;
            
            OpDebugInfo firstOp = DebugInfo[startIndex];
            OpDebugInfo lastOp = DebugInfo[endIndex];

            if (expression.TestsHint != null)
                foreach (TestCommand command in expression.TestsHint)
                {
                    if (command.IsPreOp && firstOp != null)
                        firstOp.AddTest(command);

                    else if (!command.IsPreOp && lastOp != null)
                        lastOp.AddTest(command);
                }
        }

        public void ThrowError(string errorMessage)
        {
            Expression subject = null;
            if (_sources.Count > 0) subject = _sources.Peek();

            string message = $"{errorMessage} " +
                $"(op line: {Operations.Count}) (subject: {subject?.GetTraceString()})";

            throw new CodeBuildingException(message);
        }

        public Executable GetResult(
            Dictionary<FunctionSignature, FunctionInfo> userDeclarations)
        {
            return new Executable(Operations, DebugInfo, 
                _entryPoints, _labels, userDeclarations);
        }

        public void AddEntryPoint(FunctionSignature signature, int index)
        {
            _entryPoints.Add(signature, index);
        }

        public void AddComment(string message)
        {
            Op pass = new Pass();
            AddOp(pass, null, message);
        }

        public void AddOp(Op op, Token focus = null, 
            string comment = null, Expression source = null)
        {
            if (source == null && _sources.Count > 0) 
                source = _sources.Peek();

            int index = Operations.Count;

            OpDebugInfo info = new OpDebugInfo(op, index, source, focus, comment);

            source?.OperatorsHint?.Add(info);

            Operations.Add(op);
            DebugInfo.Add(info);
        }

        public string DeclareLabel(string baseName)
        {
            int i = 0;

            string name;

            do
            {
                name = $"{baseName}_{i}";
                i++;
            }
            while (_labels.ContainsKey(name));

            _labels.Add(name, int.MinValue);

            return name;
        }

        public void SetLabelToNext(string name)
        {
            if (!_labels.ContainsKey(name))
                throw new ArgumentException($"Tried to set the value of a non-existent label: {name}");

            _labels[name] = InstructionCount;
        }

        public void PushIndex()
        {
            _indexDepth++;
        }

        public string GetIndexedName(string baseName = "index")
        {
            return $".{baseName}_{_indexDepth}";
        }

        public void PopIndex()
        {
            if (_indexDepth == 0)
                throw new InvalidOperationException("No index variable to clear");

            _indexDepth--;
        }
    }
}
