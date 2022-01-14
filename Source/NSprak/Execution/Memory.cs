using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using NSprak.Exceptions;
using NSprak.Functions;
using NSprak.Functions.Signatures;
using NSprak.Language;
using NSprak.Language.Values;
using NSprak.Messaging;

namespace NSprak.Execution
{
    public class FrameDebugInfo
    {
        private int? _fixedLocation = null;
        private readonly InstructionEnumerator _instructions;

        public FunctionSignature FunctionSignature { get; }

        public int Location => _fixedLocation ?? _instructions.Index;
  
        public ExecutionScope Scope { get; set; }

        public FrameDebugInfo(InstructionEnumerator instructions,
            FunctionSignature signature, ExecutionScope scope = null)
        {
            _instructions = instructions;

            FunctionSignature = signature;
            Scope = scope;
        }

        public void FixLocation()
        {
            _fixedLocation = _instructions.Index;
        }

        public void ClearFixedLocation()
        {
            _fixedLocation = null;
        }
    }

    public class ExecutionScope
    {
        private readonly bool _inherit;
        private readonly Dictionary<string, Value> _locals 
            = new Dictionary<string, Value>();

        public ExecutionScope Parent { get; }

        public ExecutionScope Global { get; }

        public bool IsGlobal => this == Global;

        public ExecutionScope()
        {
            Parent = null;
            Global = this;
            _inherit = false;
        }

        public ExecutionScope(ExecutionScope parent, bool inherit)
        {
            Parent = parent;
            Global = parent.Global;
            _inherit = inherit;
        }

        public bool TryDeclareVariable(string name, Value initialValue)
        {
            // This means that a duplicate variable cannot be defined in the same
            // scope, but duplicate variables *can* be declared in nested scopes.
            // This is more flexible than Sprak, and the precise limitations
            // are for the static checker to pick up.
            if (_locals.ContainsKey(name))
                return false;

            _locals.Add(name, initialValue);
            return true;
        }

        public bool TrySetVariable(string name, Value newValue)
        {
            if (_locals.ContainsKey(name))
            {
                _locals[name] = newValue;
                return true;
            }

            if (IsGlobal)
                return false;

            if (Parent != null || _inherit)
                return Parent.TrySetVariable(name, newValue);

            return Global.TrySetVariable(name, newValue);
        }

        public bool TryFindVariable(string name, out Value result)
        {
            if (_locals.TryGetValue(name, out result))
                return true;

            if (IsGlobal)
                return false;

            if (Parent != null && _inherit)
                return Parent.TryFindVariable(name, out result);

            return Global.TryFindVariable(name, out result);
        }

        public IEnumerable<KeyValuePair<string, Value>> ListVariables()
        {
            if (IsGlobal)
                return _locals;

            if (Parent != null && _inherit)
                return _locals.Concat(Parent.ListVariables());

            return _locals.Concat(Global.ListVariables());
        }
    }

    public class Memory
    {
        private readonly SprakConverter _converter;

        public Stack<Value> Values { get; } = new Stack<Value>();

        public Stack<int> Frames { get; } = new Stack<int>();

        public FrameDebugInfo MainFrame { get; }

        public Stack<FrameDebugInfo> FrameDebugInfo { get; } 
            = new Stack<FrameDebugInfo>();

        public ExecutionScope CurrentScope { get; private set; } = new ExecutionScope();

        public Memory(SprakConverter converter, FrameDebugInfo mainFrame)
        {
            _converter = converter;
            MainFrame = mainFrame;
        }

        public void Reset()
        {
            Values.Clear();
            Frames.Clear();
            FrameDebugInfo.Clear();

            CurrentScope = new ExecutionScope();

            MainFrame.Scope = CurrentScope;
            FrameDebugInfo.Push(MainFrame);
        }

        public void BeginScope(bool inherit)
        {
            CurrentScope = new ExecutionScope(CurrentScope, inherit);
            
            FrameDebugInfo.Peek().Scope = CurrentScope;
        }

        public void EndScope()
        {
            if (CurrentScope.Parent == null)
                throw new SprakInternalRuntimeException("Cannot end root scope");

            CurrentScope = CurrentScope.Parent;

            FrameDebugInfo.Peek().Scope = CurrentScope;
        }

        public void Declare(string name, Value initialValue)
        {
            if (!CurrentScope.TryDeclareVariable(name, initialValue))
                throw new SprakInternalRuntimeException($"A variable named \"{name}\" has already been declared");
        }

        public void SetVariable(string name, Value value)
        {
            if (!CurrentScope.TrySetVariable(name, value))
                throw new SprakInternalRuntimeException($"Cannot find a variable named \"{name}\"");
        }

        public Value GetVariable(string name)
        {
            if (!CurrentScope.TryFindVariable(name, out Value result))
                throw new SprakInternalRuntimeException($"Cannot find a variable named \"{name}\"");

            return result;
        }

        public Value GetVariable(string name, SprakType targetType)
        {
            Value rawResult = GetVariable(name);

            _converter.TryConvertValue(rawResult, targetType, out Value result);
            return result;
        }

        public T GetVariable<T>(string name)
            where T : Value
        {
            SprakType type = SprakType.NetToSprakLookup[typeof(T)];
            return (T)GetVariable(name, type);
        }

        public Value PopValue()
        {
            if (Values.Count == 0)
            {
                string message = $"Attempted to pop a value from an empty stack";
                throw new SprakInternalExecutionException(message);
            }

            Value result =  Values.Pop();
            return result;
        }

        public Value PopValue(SprakType type)
        {
            Value value = PopValue();

            if (!_converter.TryConvertValue(value, type, out Value result))
            {
                PushValue(value); // This is a recoverable runtime error, so restore the stack.
                throw new SprakRuntimeException(Messages.UexpectedType, type, value.Type, value);
            }

            return result;
        }

        public T PopValue<T>() where T : Value
        {
            SprakType target = SprakType.NetToSprakLookup[typeof(T)];
            return PopValue(target) as T;
        }

        public void PushValue(Value value)
        {
            value = value ?? throw new ArgumentNullException(nameof(value));
            Values.Push(value);
        }
    }
}
