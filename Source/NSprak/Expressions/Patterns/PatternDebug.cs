using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using NSprak.Expressions.Patterns.Elements;
using NSprak.Tokens;

namespace NSprak.Expressions.Patterns
{
    public enum TraceKind
    {
        CanExecute, Execute
    }

    public enum TraceState
    {
        InProgress, Success, Fail
    }

    public class TraceItem
    {
        public TraceKind Kind;

        public PatternElement PatternElement;

        public TraceState State;

        public void Update(bool success)
        {
            State = success ?
                TraceState.Success : TraceState.Fail;
        }

        public override string ToString()
        {
            string kind = Enum.GetName(typeof(TraceKind), Kind);
            string state = Enum.GetName(typeof(TraceState), State);
            return $"[{kind}][{state}] {PatternElement}";
        }
    }

    public class TallyEntry
    {
        public int LastPosition;

        public int Count;
    }

    public class RuntimeTrace
    {
        // Note to future me, because I always get confused over this.
        // The default hashcode is FINE for classes where equality is 
        // still plain old reference equality. 

        // It only needs overriding if you change the meaning of equality,
        // or potentially with value types since the default implementation 
        // there is slow.

        // Source: https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode?view=net-5.0

        public int CycleThreshold { get; set; } = 20;

        public List<TraceItem> Items { get; } = new List<TraceItem>();

        public Dictionary<PatternElement, TallyEntry> Tally { get; }
            = new Dictionary<PatternElement, TallyEntry>();

        public PatternState State { get; }

        public RuntimeTrace(PatternState state)
        {
            State = state;
        }

        public TraceItem OnCanExecute(PatternElement source)
        {
            return StartTrace(TraceKind.CanExecute, source);
        }

        public TraceItem OnExecute(PatternElement source)
        {
            return StartTrace(TraceKind.Execute, source);
        }

        private TraceItem StartTrace(TraceKind kind, PatternElement source)
        {
            TraceItem traceElement = new TraceItem
            {
                Kind = kind,
                PatternElement = source,
                State = TraceState.InProgress
            };

            Items.Add(traceElement);

            if (!Tally.TryGetValue(source, out TallyEntry entry))
            {
                entry = new TallyEntry
                {
                    LastPosition = 0,
                    Count = 0
                };

                Tally.Add(source, entry);
            }

            if (entry.LastPosition != State.Enumerator.Index)
            {
                entry.Count = 0;
                entry.LastPosition = State.Enumerator.Index;
            }

            entry.Count++;

            if (entry.Count > CycleThreshold)
                throw new Exception("Possible cycle detected!");

            return traceElement;
        }

        public class PatternTextHelper
        {
            public void Visit(PatternElement root)
            {
                HashSet<PatternElement> ancestors = new HashSet<PatternElement>();
                HashSet<PatternElement> visited = new HashSet<PatternElement>();
                Visit(root, ancestors, visited);
            }

            private void Visit(
                PatternElement current, 
                HashSet<PatternElement> ancestors,
                HashSet<PatternElement> visited)
            {
                if (visited.Contains(current))
                    return;

                if (ancestors.Contains(current))
                {
                    string typeName = current.GetType().Name;
                    current.SpecifySourceText($"{{Cycle: {typeName}}}");
                    return;
                }

                ancestors.Add(current);

                string result = "";

                switch (current)
                {
                    case Pattern pattern:

                        Visit(pattern.Value, ancestors, visited);

                        result = $"Pattern(\"{pattern.Name}\")";
                        break;

                    case CommandElement command:

                        string commandName = Enum
                            .GetName(typeof(PatternCommand), command.Value);

                        result = $"Command({commandName})";
                        break;

                    case EmptyElement _:

                        result = "Empty";
                        break;

                    case EndElement end:

                        string opName = "End";

                        if (end.Destination == null)
                        {
                            result = $"{opName}(null)";
                            break;
                        }

                        string enclosingName = end.Destination
                            .Method
                            .DeclaringType
                            .Name;

                        string fnName = end.Destination
                            .Method
                            .Name;

                        result = $"{opName}({enclosingName}.{fnName})";
                        break;

                    case OptionalElement option:

                        Visit(option, ancestors, visited);
                        result = $"Allow({option.Value})";

                        break;

                    case Options options:

                        foreach (PatternElement element in options.Elements)
                            Visit(element, ancestors, visited);

                        string elements = string.Join(" | ", 
                            options.Elements.Select(x => x.ToString()));

                        result = $"({elements})";
                        break;

                    case Sequence sequence:

                        foreach (PatternElement element in sequence.Elements)
                            Visit(element, ancestors, visited);

                        elements = string.Join(" & ", 
                            sequence.Elements.Select(x => x.ToString()));

                        result = elements;
                        break;

                    case TokenElement token:

                        string typeName = Enum
                            .GetName(typeof(TokenType), token.Type);

                        result = $"Token({typeName}, {token.Content})";
                        break;

                    case TokenTypeElement tokenType:

                        typeName = Enum
                            .GetName(typeof(TokenType), tokenType.Type);

                        result = $"Token({typeName})";
                        break;

                    default: throw new Exception("Unrecognized element type");
                }

                ancestors.Remove(current);
                visited.Add(current);
                current.SpecifySourceText(result);
            }
        }
    }
}
