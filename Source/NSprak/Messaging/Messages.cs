using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Messaging
{
    public static class Messages
    {
        public static MessageTemplate

            CanOnlyIndexArrays = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Only an array can be indexed. Found: {Type}"
            },

            ExtraEndStatement = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Found extra end statement that does not seem to close anything"
            },

            IndexShouldBeNumber = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "An index should be a number. Found: {Type}"
            },

            UnrecognizedSymbols = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized symbols"
            },

            UnrecognizedOperator = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized operator: '{Content}'"
            },

            UnrecognizedName = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized name: '{Name}'"
            },

            ReferenceBeforeDefinition = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Name referenced before its local definition: '{Name}'"
            },

            UnexpectedElseIfStatement = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "'else if' statement without preceding 'if' statement"
            },

            UnexpectedElseStatement = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "'else' statement without preceding 'if' statement"
            },

            UnresolvedCall = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unable to resolve function call: {Signature}"
            },

            UnrecognizedNumber = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized number: '{Content}'"
            },

            UnresolvedOperation = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized operation: '{Content}'"
            },

            UnexpectedEndOfLine = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unexpected end of line"
            },

            UnexpectedToken = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unexpected token"
            },

            UnexpectedTokenAtEnd = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "End of statement reached. Unexpected token: '{Token}'"
            },

            DuplicateVariable = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "A variable of name '{Name}' has already been declared in this scope"
            },

            DuplicateFunction = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "A function of name '{Name}' has already been declared in this scope"
            },

            NestedFunction = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "A function inside a block is not allowed. (Have you forgotten to close the previous block?)"
            },

            BlockNotClosed = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "No end found for this header"
            };
    }
}
