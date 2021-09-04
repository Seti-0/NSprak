using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Expressions.Patterns;
using NSprak.Tokens;
using NSprak.Language;
using NSprak.Expressions.Types;

namespace NSprak.Expressions.Creation
{
    public static class ControlFlow
    {
        public static IfHeader CreateIfHeader(MatchIterator iterator)
        {
            iterator.AssertKeyword(Keywords.If);
            Token start = (Token)iterator.Current;

            iterator.AssertExpression(out Expression condition);
            iterator.AssertEnd();

            IfHeader result = new IfHeader(start, condition);
            return result;
        }

        public static LoopHeader CreateLoopHeader(MatchIterator iterator)
        {
            Expression array;

            iterator.AssertKeyword(Keywords.Loop);
            Token loop = (Token)iterator.Current;

            if (iterator.AtEnd())
            {
                LoopHeader result = new LoopHeader(loop);
                return result;
            }

            if (iterator.NextIsExpression(out array))
            {
                iterator.AssertEnd();

                LoopHeader result = new LoopHeader(loop, array);
                return result;
            }

            if (iterator.NextIsToken(TokenType.Name, out Token name))
            {
                if (iterator.NextIsKeyword(Keywords.In, out Token inToken))
                {
                    iterator.AssertExpression(out array);
                    iterator.AssertEnd();

                    LoopHeader result = new LoopHeader(loop, name, inToken, array);
                    return result;
                }

                if (iterator.NextIsKeyword(Keywords.From, out Token from))
                {
                    iterator.AssertExpression(out Expression start);
                    iterator.AssertKeyword(Keywords.To, out Token to);
                    iterator.AssertExpression(out Expression end);
                    iterator.AssertEnd();

                    LoopHeader result = new LoopHeader(
                        loop, name, from, start, to, end);
                    return result;
                }
            }

            throw iterator.UnexpectedEnd();
        }
    }
}
