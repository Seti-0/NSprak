﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Transactions;

using NSprak.Expressions.Types;
using NSprak.Tokens;
using NSprak.Messaging;
using NSprak.Language;

namespace NSprak.Expressions.Structure
{
    public static class TreeBuilder
    {
        public static Block Build(List<Expression> statements, CompilationEnvironment env)
        {
            Token mainStart = null;
            Token mainEnd = null;

            if (statements.Count > 0)
            {
                mainStart = statements[0].StartToken;
                mainEnd = statements[^1].EndToken;
            }

            Stack<List<Expression>> statementStack = new Stack<List<Expression>>();
            Stack<Header> headerStack = new Stack<Header>();

            statementStack.Push(new List<Expression>());

            foreach (Expression statement in statements)
            {
                if (statement is FunctionHeader functionHeader)
                {
                    // Sprak does not allow functions within other blocks.
                    if (headerStack.Count > 0)
                        env.Messages.AtToken(functionHeader.NameToken,
                            Messages.NestedFunction);
                }

                if (statement is IConditionalSubComponent subcomponent)
                {
                    bool valid = false;
                    if (headerStack.Count > 0)
                    {
                        if (headerStack.Peek() is IfHeader ifHeader)
                        {
                            ifHeader.NextConditionalComponentHint = subcomponent;

                            Token endToken = null;
                            List<Expression> ifStatements = statementStack.Peek();
                            if (ifStatements.Count > 0)
                                endToken = ifStatements[^1].EndToken;

                            EndBlock(null);
                            valid = true;
                        }
                        else if (headerStack.Peek() is ElseIfHeader elseIfHeader)
                        {
                            elseIfHeader.NextConditionalComponentHint = subcomponent;

                            Token endToken = null;
                            List<Expression> ifStatements = statementStack.Peek();
                            if (ifStatements.Count > 0)
                                endToken = ifStatements[^1].EndToken;

                            EndBlock(null);
                            valid = true;
                        }
                    }

                    if (!valid)
                    {
                        if (statement is ElseHeader)
                            env.Messages.AtExpression(
                                statement, Messages.UnexpectedElseStatement);

                        else if (statement is ElseIfHeader)
                            env.Messages.AtExpression(
                                statement, Messages.UnexpectedElseIfStatement);
                    }
                }

                if (statement is Header newBlockHeader)
                {
                    headerStack.Push(newBlockHeader);
                    statementStack.Push(new List<Expression>());
                }
                else if (statement is Command command && command.Keyword == Keywords.End)
                {
                    if (headerStack.Count == 0)
                        env.Messages.AtExpression(command, Messages.ExtraEndStatement);

                    else EndBlock(command);
                }
                else statementStack.Peek().Add(statement);
            }

            void EndBlock(Expression blockEnd = null,
                Token startToken = null, Token endToken = null)
            {
                Header blockHeader = headerStack.Pop();
                List<Expression> blockStatements = statementStack.Pop();

                startToken ??= blockHeader.StartToken;
                endToken ??= blockEnd?.EndToken;

                Block block = new Block(blockHeader, blockStatements, blockEnd, 
                    startToken, endToken);

                statementStack.Peek().Add(block);
            }

            while (headerStack.Count > 0)
            {
                env.Messages.AtExpression(
                    headerStack.Peek(), Messages.BlockNotClosed);

                EndBlock(null, endToken: mainEnd);
            }

            if (statementStack.Count != 1)
                // Unless I've made a mistake, this should never happen.
                throw new Exception("Assertion error");

            Header header = new MainHeader();
            List<Expression> mainStatements = statementStack.Pop();
            
            // The main header has no start/end token, so the start token
            // has to be explicitly given here.
            Block mainBlock = new Block(header, mainStatements, null, 
                startToken: mainStart, endToken: mainEnd);

            mainBlock.ScopeHint = new Scope();
            return mainBlock;
        }
    }
}
