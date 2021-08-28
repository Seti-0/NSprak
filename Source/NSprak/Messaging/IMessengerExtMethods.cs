using System;
using System.Collections.Generic;
using System.Text;

using NSprak.Tokens;
using NSprak.Expressions;

namespace NSprak.Messaging
{
    public static class IMessengerExtMethods
    {
        public static void Add(this Messenger messenger, 
            MessageTemplate message, params object[] parameters)
        {
            messenger.Add(null, message, parameters);
        }

        public static void AtToken(this Messenger messenger, Token token, 
            MessageTemplate message, params object[] parameters)
        {
            messenger.Add(new MessageLocation(token), message, parameters);
        }

        public static void AtRange(this Messenger messenger, Token start, 
            Token end, MessageTemplate message, params object[] parameters)
        {
            messenger.Add(new MessageLocation(start, end), message, parameters);
        }

        public static void AtExpression(this Messenger messenger, 
            Expression expression, MessageTemplate message, params object[] parameters)
        {
            MessageLocation location = new MessageLocation(expression.StartToken, expression.EndToken);
            messenger.Add(location, message, parameters);
        }
    }
}
