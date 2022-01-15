using System;
using System.Collections.Generic;
using System.Text;

namespace NSprak.Messaging
{
    public static class Messages
    {
        public static MessageTemplate

            AssignmentTypeMismatch = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "A value of type {Src} cannot be assigned to a variable of type {Dest}"
            },

            BlockNotClosed = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "No end found for this header"
            },

            CanOnlyIndexArrays = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Only an array can be indexed. Found: {Type}"
            },

            DuplicateFunction = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "A function of name '{Name}' has already been declared in this scope"
            },

            DuplicateVariable = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "A variable of name '{Name}' has already been declared in this scope"
            },

            ExpectedAssignmentOperator = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "'{Operator}' is an expression operator, and cannot be used for assignment"
            },

            ExtraEndStatement = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Found extra end statement that does not seem to close anything"
            },

            IncorrectUseOfAssignment = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "'{Operator}' is an assignment operator, and cannot be used as an expression"
            },

            IncompatibleReturnValue = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Return value of type '{Src}' is not assignable to declared function type '{Dst}'"
            },

            IndexerNotSupported = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Sprak only supports indexing of variable names"
            },

            IndexShouldBeNumber = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "An index should be a number. Found: {Type}"
            },

            InvalidDeclarationOperator = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Only the '=' operator may be used to declare variables"
            },

            InvalidIndexDeclaration = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "A single index cannot be declared on its own"
            },

            MissingContent = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Missing test command content"
            },

            MissingReturnValue = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "A return statement in a function that is not void must have a value"
            },

            MultipleIndices = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Sprak only allows one layer of indexing"
            },

            NestedFunction = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "A function inside a block is not allowed. (Have you forgotten to close the previous block?)"
            },

            ReturnOutsideFunction = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "A return statement is only valid within a function"
            },

            ReturnValueFromVoid = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "A return statement in a void function cannot have a value"
            },

            ReferenceBeforeDefinition = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Name referenced before its local definition: '{Name}'"
            },

            TestParseError = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unable to parse test command: '{Content}'"
            },

            TestValueParseError = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unable to parse test command value: '{Value}'"
            },

            UnexpectedAssertion = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Assertion found outside of test case"
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

            UnexpectedEndOfLine = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unexpected end of line"
            },

            UnrecognizedError = new MessageTemplate
            {
                Severity = MessageSeverity.Warning,
                Summary = "Unrecognized error message: '{Name}'"
            },

            UnrecognizedName = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized name: '{Name}'"
            },

            UnrecognizedNumber = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized number: '{Content}'"
            },

            UnresolvedCall = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unable to resolve function call: {Signature}"
            },

            UnresolvedOperation = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized operation: '{Content}'"
            },

            UnrecognizedOperator = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized operator: '{Content}'"
            },

            UnrecognizedSymbols = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Unrecognized symbols"
            },

            UnexpectedTestTitle = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "Test titles are only valid at the top of files"
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

            UexpectedType = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Title = "Unexpected type",
                Summary = "Expected {Expected}, found {Found}: '{Value}'"
            },

            VariableFromDisconnectedBlock = new MessageTemplate
            {
                Severity = MessageSeverity.Error,
                Summary = "This variable was declared in the same scope but in a disconnected block, and cannot be used here"
            };
    }
}
