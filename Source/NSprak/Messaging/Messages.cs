using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Messaging
{
    public static class Messages
    {
        public static MessageTemplate

            UnrecognizedSymbols = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized symbols"
            },

            UnrecognizedOperator = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized operator: {Content}"
            },

            UnrecognizedNumber = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized operator: {Content}"
            },

            UnexpectedEnd = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unexpected end of code"
            },

            UnexpectedEndOfLine = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unexpected end of line"
            },

            UnexpectedToken = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unexpected token: {Token}"
            },

            UnexpectedTokenAtEnd = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "End of statement reached. Unexpected token: {Token}"
            },

            DuplicateVariable = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "A variable of name \"{Name}\" has already been declared in this scope"
            },

            DuplicateFunction = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "A function of name \"{Name}\" has already been declared in this scope"
            },

            NestedFunction = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "A function inside a block is not allowed. (Have you forgotten to close the previous block?)"
            },

            MissingEndStatement = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "No end found for this header"
            };
    }
}
