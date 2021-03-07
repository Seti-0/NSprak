using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Tokens;
using NSprak.Expressions;

namespace NSprak.Messaging
{
    public static class IMessengerExtMethods
    {
        public static void Add(this IMessenger messenger, 
            Message message, params object[] parameters)
        {
            messenger.Add(null, message, parameters);
        }

        public static void AtToken(this IMessenger messenger, Token token, 
            Message message, params object[] parameters)
        {
            messenger.Add(new MessageLocation(token), message, parameters);
        }

        public static void AtRange(this IMessenger messenger, Token start, 
            Token end, Message message, params object[] parameters)
        {
            messenger.Add(new MessageLocation(start, end), message, parameters);
        }

        public static void AtExpression(this IMessenger messenger, 
            Expression expression, Message message, params object[] parameters)
        {
            MessageLocation location = new MessageLocation(expression.StartToken, expression.EndToken);
            messenger.Add(location, message, parameters);
        }
    }
}
