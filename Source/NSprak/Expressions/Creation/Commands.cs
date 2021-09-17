using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NSprak.Expressions.Patterns;
using NSprak.Expressions.Types;
using NSprak.Language;
using NSprak.Tokens;

namespace NSprak.Expressions.Creation
{
    public static class Commands
    {
        public static Command Create(MatchIterator iterator)
        {
            iterator.AssertTokenType(TokenType.KeyWord, out Token keyWord);
            iterator.AssertEnd();

            Command result = new Command(keyWord);
            return result;
        }

        public static Return Return(MatchIterator iterator)
        {
            iterator.AssertKeyword(Keywords.Return);
            Token startToken = (Token)iterator.Current;

            Expression expression;

            iterator.NextIsExpression(out expression);

            iterator.AssertEnd();

            Return result = new Return(startToken, expression);
            return result;
        }
    }
}
