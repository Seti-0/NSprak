using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Messaging
{
    public static class Messages
    {
        public static Message

            UnrecognizedSymbols = new Message
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized symbols"
            },

            UnrecognizedOperator = new Message
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized operator: {Content}"
            },

            UnrecognizedNumber = new Message
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized operator: {Content}"
            },

            UnexpectedEnd = new Message
            {
                Severity = MessageSeverity.Error,
                Summary = "Unexpected end of code"
            },

            UnexpectedEndOfLine = new Message
            {
                Severity = MessageSeverity.Error,
                Summary = "Unexpected end of line"
            },

            UnexpectedToken = new Message
            {
                Severity = MessageSeverity.Error,
                Summary = "Unexpected token: {Token}"
            },

            UnexpectedTokenAtEnd = new Message
            {
                Severity = MessageSeverity.Error,
                Summary = "End of statement reached. Unexpected token: {Token}"
            },

            DuplicateVariable = new Message
            {
                Severity = MessageSeverity.Error,
                Summary = "A variable of name \"{Name}\" has already been declared in this scope"
            },

            DuplicateFunction = new Message
            {
                Severity = MessageSeverity.Error,
                Summary = "A function of name \"{Name}\" has already been declared in this scope"
            },

            NestedFunction = new Message
            {
                Severity = MessageSeverity.Error,
                Summary = "A function inside a block is not allowed. (Have you forgotten to close the previous block?)"
            },

            MissingEndStatement = new Message
            {
                Severity = MessageSeverity.Error,
                Summary = "No end found for this header"
            };
    }
}
