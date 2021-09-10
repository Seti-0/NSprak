using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NSprak.Messaging;
using NSprak.Tokens;

namespace NSprak.Expressions.Patterns
{
    public class PatternState
    {
        public static bool EnableDebugTrace { get; set; } = true;


        // Keep track of if a sequence in the pattern is being
        // iterated. This isn't really necessary, but it means an exception
        // can be thrown if a command is used in the wrong place.
        private int _sequenceScope = 0;

        private readonly Stack<List<object>> _collections = new Stack<List<object>>();

        public Messenger Messenger { get; }

        public bool Error { get; private set; }

        public PatternEnumerator Enumerator { get; }

        public PatternCommand Command { get; private set; } = PatternCommand.None;

        public PatternEnd EndDestination { get; private set; }

        public bool InSequenceScope => _sequenceScope > 0;

        public RuntimeTrace Trace { get; }

        public PatternState(IEnumerable<Token> tokens, Messenger messenger)
        {
            Error = false;
            Enumerator = new PatternEnumerator(tokens);
            Messenger = messenger;

            if (EnableDebugTrace)
                Trace = new RuntimeTrace(this);
        }

        public void SetCommand(PatternCommand command)
        {
            if (Command != PatternCommand.None)
                throw new Exception("A command has already been set!");

            Command = command;
        }

        public void End(PatternEnd destination)
        {
            EndDestination = destination;
            SetCommand(PatternCommand.End);
        }

        public void ClearCommand()
        {
            Command = PatternCommand.None;
            EndDestination = null;
        }

        public void RaiseError(MessageTemplate message, params object[] parameters)
        {
            Error = true;
            Messenger.Add(message, parameters);
        }

        public void RaiseError(MessageLocation location, MessageTemplate message, params object[] parameters)
        {
            Error = true;
            Messenger.Add(location, message, parameters);
        }

        public void RaiseError(Token token, MessageTemplate message, params object[] parameters)
        {
            Error = true;
            Messenger.AtToken(token, message, parameters);
        }

        public void BeginCollection()
        {
            _collections.Push(new List<object>());
        }

        public void AddToCollection(object item)
        {
            if (!_collections.Any()) return;

            _collections.Peek().Add(item);
        }

        public List<object> EndCollection()
        {
            if (!_collections.Any())
                throw new InvalidOperationException("A collection has not been started in this pattern match");

            return _collections.Pop();
        }

        public override string ToString()
        {
            string result = Enumerator.ToString();

            if (Error)
                result = $"[Error] " + result;

            if (_collections.Count > 0)
                result = $"({_collections.Peek().Count}) " + result;

            return result;
        }

        public void BeginSequenceScope()
        {
            _sequenceScope++;
        }

        public void EndSequenceScope()
        {
            if (_sequenceScope == 0)
                throw new Exception("There is no sequence scope to end!");

            _sequenceScope--;
        }
    }
}
