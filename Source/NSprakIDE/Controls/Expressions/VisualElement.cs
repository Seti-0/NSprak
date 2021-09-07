using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup.Localizer;
using System.Windows.Media;
using NSprak.Expressions.Types;
using NSprak.Expressions.Structure;

using NSprak.Language;
using NSprak.Language.Builtins;
using NSprak.Language.Values;
using NSprak.Operations;

using NSprakIDE.Themes;

namespace NSprakIDE.Controls.Expressions
{
    using NSprakExpression = NSprak.Expressions.Expression;
    using NSprakBlock = NSprak.Expressions.Types.Block;

    public class BlockReferenceElement
    {
        public NSprakBlock Target;

        public BlockReferenceElement(NSprakBlock target) { Target = target; }
    }

    public class FunctionParameter
    {
        public string Name { get; }

        public SprakType Type { get; }

        public FunctionParameter(string name, SprakType type)
        {
            Name = name;
            Type = type;
        }
    }

    public class VisualElement
    {
        public List<VisualElement> Items { get; set; } = new List<VisualElement>();

        public bool ShowDebug { get; set; }

        public string NameColorKey { get; set; }

        public string ValueColorKey { get; set; }

        public bool MonocolorValue { get; set; }

        public string Name { get; }

        public object Value { get; }

        private bool _monocolor;

        private RichTextBox _target;

        public VisualElement()
        {
            Name = null;
            Value = null;
        }

        public VisualElement(object value, bool showDebug)
        {
            Value = value;
            ShowDebug = showDebug;

            //if (Value is NSprakExpression expression)
            //    AddChildren(expression);

            AddChildren(Value);
        }

        public VisualElement(string name, object value, bool showDebug)
        {
            Name = name;
            Value = value;
            ShowDebug = showDebug;

            AddChildren(value);
        }

        #region Silly

        // I don't know how else to support readonly collections without a type,
        // since they don't seem to share a relevant base class?

        private void AddParameter<T>(string name, IReadOnlyList<T> items)
        {
            AddParameter(name, (object) items.ToList());
        }

        private void AddDebugParameter<T>(string name, IReadOnlyList<T> items)
        {
            AddDebugParameter(name, (object) items.ToList());
        }

        private void AddParameter<K,V>(string name, IReadOnlyDictionary<K,V> items)
        {
            AddParameter(name, (object) items.ToDictionary(x => x.Key, x => x.Value));
        }

        private void AddDebugParameter<K,V>(string name, IReadOnlyDictionary<K,V> items)
        {
            AddDebugParameter(name, (object) items.ToDictionary(x => x.Key, x => x.Value));
        }

        #endregion

        private void AddParameter(string name, object obj)
        {
            VisualElement element = new VisualElement(name, obj, ShowDebug);
            Items.Add(element);
        }

        private void AddDebugParameter(string name, object obj)
        {
            if (!ShowDebug)
                return;

            VisualElement element = new VisualElement(name, obj, ShowDebug);
            element.NameColorKey = Theme.Expressions.Debug;
            element.ValueColorKey = Theme.Expressions.Comment;
            element.MonocolorValue = true;
            Items.Add(element);
        }

        private void ErrorParameter(string text)
        {
            Items.Add(new VisualElement("Error", text, ShowDebug));
        }

        private void AddChildren(object obj)
        {
            switch (obj)
            {
                case IDictionary dictionary:

                    foreach (object key in dictionary.Keys)
                    {
                        object value = dictionary[key];
                        AddParameter(key.ToString(), value);
                    }

                    break;

                case IList list:

                    int count = 0;

                    foreach (object item in list)
                    {
                        AddParameter(count.ToString(), item);
                        count++;
                    }

                    break;

                case FunctionInfo function:
                    AddParameter("Return Type", function.ReturnType);
                    AddParameter("Parameters", function.Parameters.ToList());
                    break;

                case NSprakExpression expression:
                    AddChildren(expression);
                    break;

                case OpDebugInfo op:
                    AddParameter("Code", op.Op.ShortName);
                    AddParameter("Param", op.Op.RawParam);
                    AddParameter("Breakpoint", op.Breakpoint);
                    AddDebugParameter("Step", op.Op.StepAfterwards);
                    AddDebugParameter("Token", op.FocusToken);
                    // We can't have the full expression here because the tree 
                    // is not lazy.
                    AddDebugParameter("Expression", op.SourceExpression.ToString());
                    break;
            }
        }

        private void AddChildren(NSprakExpression parent)
        {
            AddDebugParameter("Trace", parent.GetTraceString());

            if (parent.TypeHint != SprakType.Unit)
                AddDebugParameter("Type Hint", parent.TypeHint);

            if (!(parent is NSprakBlock mainBlock && mainBlock.Header is MainHeader))
                AddDebugParameter("Parent Hint", new BlockReferenceElement(parent.ParentBlockHint));

            if (parent.OperatorsHint != null)
            {
                List<OpDebugInfo> ops = parent.OperatorsHint.ToList();
                if (parent is NSprakBlock block)
                    ops.AddRange(block.Header.OperatorsHint);

                AddDebugParameter("Operators", ops);
            }

            switch (parent)
            {
                case Command _: break;
                case LiteralGet _: break;

                case NSprakBlock block: 

                    AddHeaderParameters(block.Header);

                    if (block.VariableDeclarationsHint == null || block.VariableDeclarationsHint.Count > 0)
                        AddDebugParameter("Variable Declarations Hint", block.VariableDeclarationsHint);

                    foreach (NSprakExpression statement in block.Statements)
                        AddParameter(null, statement);
                    
                    break;

                case FunctionCall functionCall:
                    AddFunctionHintParameters(functionCall.UserFunctionHint, functionCall.BuiltInFunctionHint, null);
                    AddParameter("Arguments", functionCall.Arguments);
                    break;

                case LiteralArrayGet array:
                    AddParameter("Elements", array.Elements);
                    break;

                case OperatorCall op:

                    AddFunctionHintParameters(null, op.BuiltInFunctionHint, null);

                    if (op.LeftInput != null)
                        AddParameter("Left", op.LeftInput);

                    if (op.RightInput != null)
                        AddParameter("Right", op.RightInput);

                    break;

                case Return ret:

                    if (ret.HasValue)
                        AddParameter("Value", ret.Value);

                    break;

                case VariableAssignment varAssign:

                    AddFunctionHintParameters(null, varAssign.BuiltInFunctionHint, varAssign.OpHint);
                    
                    if (varAssign.OpHint != null)


                    if (varAssign.IsDeclaration)
                        AddParameter("Declared Type", varAssign.DeclarationType);

                    AddParameter("Name", varAssign.Name);
                    AddParameter("Operator", varAssign.Operator);
                    AddParameter("Value", varAssign.Value);

                    break;

                case VariableReference varRef:
                    AddParameter("Name", varRef.Name);
                    break;

                default:
                    ErrorParameter($"Unrecognized type: {parent.GetType().Name}");
                    break;
            }
        }

        protected void AddFunctionHintParameters(FunctionSignature userHint,
            BuiltInFunction builtinHint, Func<Op> opHint)
        {
            if (userHint != null)
                AddDebugParameter("User Function", userHint);

            if (builtinHint != null)
                AddDebugParameter("Builtin Function", builtinHint);

            if (opHint != null)
            {
                Type opType = opHint()?.GetType();
                AddDebugParameter("Op Hint", opType.Name);
            }

        }

        protected void AddHeaderParameters(Header header)
        {
            switch (header)
            {
                case MainHeader _:
                    // pass
                    break;

                case LoopHeader loop:

                    if (loop.IsRange)
                    {
                        AddParameter("From", loop.RangeStart);
                        AddParameter("To", loop.RangeEnd);
                    }

                    else if (loop.IsForeach)
                    {
                        if (loop.HasName)
                            AddParameter("Name", loop.Name);

                        AddParameter("Array", loop.Array);
                    }

                    break;

                case IfHeader conditional:
                    AddParameter("Condition", conditional.Condition);
                    break;

                case FunctionHeader function:
                    
                    AddParameter("Return Type", function.ReturnType);

                    List<FunctionParameter> parameters = new List<FunctionParameter>();

                    for (int i = 0; i < function.ParameterCount; i++)
                        parameters.Add(new FunctionParameter(function.ParameterNames[i], function.ParameterTypes[i]));

                    AddParameter("Parameters", parameters);
                    break;

                default:
                    ErrorParameter($"Unrecognized type: {header.GetType().Name}");
                    break;
            }
        }

        public void RenderTo(RichTextBox textBox)
        {
            _target = textBox;

            if (NameColorKey == null)
                NameColorKey = Theme.Expressions.Comment;

            if (ValueColorKey == null)
                ValueColorKey = Theme.Expressions.Name;

            if (Name != null)
                Write($"{Name}: ", NameColorKey);

            if (MonocolorValue)
                _monocolor = true;

            Render(Value);

            _monocolor = false;

            _target = null;
        }

        private void Write(string text, Brush brush)
        {
            TextPointer end = _target.Document.ContentEnd;
            TextRange range = new TextRange(end, end);
            range.Text = text ?? "";
            range.ApplyPropertyValue(TextElement.ForegroundProperty, brush);
        }

        private void Write(string text, string colorKey)
        {
            if (_monocolor)
                colorKey = ValueColorKey;

            Brush brush = Theme.GetBrush(colorKey);
            Write(text, brush);
        }

        private void RenderKeyword(string text)
        {
            Write(" " + text, Theme.Expressions.Keyword);
        }

        private void RenderType(SprakType type)
        {
            Write(type?.InternalName, Theme.Expressions.Type);
        }

        private void RenderName(string name)
        {
            Write(" " + name, Theme.Expressions.Name);
        }

        private void RenderOperator(Operator op)
        {
            Write(op.Name, Theme.Expressions.Operator);
        }

        private void RenderBoolean(bool boolean)
        {
            Write(" " + boolean, Theme.Expressions.Literal);
        }

        private void RenderNumber(double number)
        {
            Write(" " + number.ToString(), Theme.Expressions.Literal);
        }

        private void RenderString(string text)
        {
            Write($" \"{text}\"", Theme.Expressions.Literal);
        }

        private void RenderComment(string comment)
        {
            Write(comment, Theme.Expressions.Comment);
        }

        private void RenderFunctionParameter(FunctionParameter parameter)
        {
            RenderType(parameter.Type);
            RenderName(parameter.Name);
        }

        private void Render(object obj)
        {
            switch (obj)
            {
                case bool boolean:
                    RenderBoolean(boolean);
                    break;

                case double number:
                    RenderNumber(number);
                    break;

                case SprakType type:
                    RenderType(type);
                    break;

                case Operator op:
                    RenderOperator(op);
                    break;

                case NSprakExpression expr:
                    RenderExpression(expr);
                    break;

                case VariableInfo variable:
                    RenderType(variable.DeclaredType);
                    break;

                case FunctionInfo function:
                    RenderName(function.Name);
                    break;

                case FunctionParameter parameter:
                    RenderFunctionParameter(parameter);
                    break;

                case IDictionary dictionary:
                    RenderComment($"{dictionary.Count} item(s)");
                    break;

                case IList list:
                    RenderComment($"{list.Count} item(s)");
                    break;

                case BlockReferenceElement blockRef:

                    if (blockRef.Target != null)
                        RenderHeader(blockRef.Target.Header);

                    else Render(null);

                    break;

                default:

                    if (obj == null)
                        obj = "<null>";

                    Write(obj.ToString(), ValueColorKey);

                    break;
            }
        }

        private void RenderExpression(NSprakExpression expression)
        {
            switch (expression)
            {
                case NSprakBlock block: RenderHeader(block.Header); break;

                case Command command: RenderKeyword(command.Keyword); break;
                
                case FunctionCall functionCall:
                    RenderKeyword("Call ");
                    RenderName(functionCall.Name);
                    break;

                case LiteralArrayGet _:
                    RenderComment("(Array literal)");
                    break;

                case LiteralGet literal:

                    switch (literal.Value)
                    {
                        case SprakBoolean boolean: RenderBoolean(boolean.Value); break;
                        case SprakNumber number: RenderNumber(number.Value); break;
                        case SprakString text: RenderString(text.Value); break;
                    }

                    break;

                case OperatorCall op:
                    RenderKeyword("Op ");
                    RenderOperator(op.Operator);
                    break;

                case Return _:
                    RenderKeyword(Keywords.Return);
                    break;

                case VariableAssignment varAssign:
                    RenderKeyword(varAssign.IsDeclaration?"Declare ":"Assign ");
                    RenderName(varAssign.Name);
                    break;

                case VariableReference varRef:
                    RenderKeyword("Ref ");
                    RenderName(varRef.Name);
                    break;

                default:
                    RenderComment("Unrecognized type ");
                    RenderComment($"(Name: {expression.GetType().Name})"); 
                    break;
            }
        }

        private void RenderHeader(Header header)
        {
            switch (header)
            {
                case LoopHeader loop:

                    RenderKeyword("Loop");

                    if (loop.IsInfinite)
                        RenderComment(" (infinite)");

                    else if (loop.IsRange)
                        RenderComment(" (range)");

                    else if (loop.IsForeach)
                        RenderComment(" (foreach)");

                    break;

                case IfHeader _: RenderKeyword("If"); break;

                case FunctionHeader function:
                    RenderKeyword("Function");
                    RenderName(function.Name);
                    break;

                case MainHeader _: RenderKeyword("Main"); break;

                default:
                    RenderComment("Unrecognized type ");
                    RenderComment($"(Name: {header.GetType().Name})");
                    break;
            }
        }
    }
}
