using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Transactions;

using NSprak.Expressions.Types;
using NSprak.Tokens;
using NSprak.Messaging;

namespace NSprak.Expressions.Structure
{
    public static class TreeBuilder
    {
        public static Block Build(List<Expression> statements, CompilationEnvironment env)
        {
            Token start = null, end = null;

            if (statements.Count > 0)
            {
                start = statements[0].StartToken;
                end = statements[^1].EndToken;
            }

            return Parse(new MainHeader(), statements, env, 
                startToken: start, endToken: end, allowFunctions: true);
        }

        private static Block Parse(Header currentHeader, List<Expression> statements, CompilationEnvironment env, 
            Token startToken = null, Token endToken = null, bool allowFunctions = true)
        {
            StatementEnumerator enumerator = new StatementEnumerator(statements);
            enumerator.BeginCollection();

            bool error = false;

            while ((!error) && enumerator.SeekHeader(out Header header))
            {
                enumerator.MoveNext(collect:false); // don't include the "header" statement
                enumerator.BeginCollection();
                bool found = enumerator.SeekEnd(out Token subEndToken);
                List<Expression> innerStatements = enumerator.EndCollection();

                if ((!allowFunctions) && header is FunctionHeader functionHeader)
                {
                    // A function inside a function is no problem in theory, though it is not a specified possibility in Sprak
                    env.Messages.AtExpression(functionHeader, Messages.NestedFunction);
                    error = true;
                }
                else if (!found)
                {
                    env.Messages.AtExpression(header, Messages.MissingEndStatement);
                    error = true;
                }
                else
                {
                    enumerator.MoveNext(collect:false); // don't include the "end" statement

                    Block subBlock = Parse(header, innerStatements, env,
                        startToken: null, endToken: subEndToken, allowFunctions: false);

                    enumerator.AddToCollection(subBlock);
                }
            }

            if (!error) 
                enumerator.Complete();
            
            List<Expression> body = enumerator.EndCollection();
            Block result = new Block(currentHeader, body, startToken, endToken);

            return result;
        }
    }
}
