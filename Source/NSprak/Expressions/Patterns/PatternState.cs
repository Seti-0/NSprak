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
        private readonly Stack<List<object>> _collections = new Stack<List<object>>();

        public IMessenger Messenger { get; }

        public bool Error { get; private set; }

        public PatternEnumerator Enumerator { get; }

        public List<PatternStep> Steps { get; } = new List<PatternStep>();

        public PatternState(IEnumerable<Token> tokens, IMessenger messenger)
        {
            Error = false;
            Enumerator = new PatternEnumerator(tokens);
            Messenger = messenger;
        }

        public void RaiseError(Message message, params object[] parameters)
        {
            Error = true;
            Messenger.Add(message, parameters);
        }

        public void RaiseError(Token token, Message message, params object[] parameters)
        {
            Error = true;
            Messenger.AtToken(token, message, parameters);
        }

        public void StartCollection()
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
                result = $"({_collections.Peek().Count})" + result;

            return result;
        }
    }
}
