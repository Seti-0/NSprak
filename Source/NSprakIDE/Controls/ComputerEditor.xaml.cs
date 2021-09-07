using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Linq;

using Microsoft.Extensions.Logging;

using NSprak;
using NSprak.Tokens;
using NSprak.Expressions;
using NSprak.Expressions.Types;
using NSprak.Execution;
using NSprak.Operations;

using NSprakIDE.Commands;
using NSprakIDE.Controls.General;
using NSprakIDE.Controls.Output;

namespace NSprakIDE.Controls
{
    using static NSprakIDE.Commands.CommandHelper;

    public class ComputerEditorEnviroment
    {
        public string FilePath;

        public OutputLog Output;

        public LocalsView LocalsView;

        public CallStackView CallStackView;

        public MessageView MessageView;
    }

    public enum ComputerEditorMode
    {
        Source, Expressions, Operations
    }

    public partial class ComputerEditor : UserControl, IDisposable
    {
        public ComputerEditorEnviroment Enviroment { get; }

        public ComputerEditorMode Mode { get; private set; }

        public Computer Computer { get; }

        private Executor _executor;

        private string _filePath;

        private SourceEditor _sourceEditor;
        private ExpressionView _expressionView;
        private OperationsView _operationsView;

        private LocalsView _localsView;
        private MessageView _messageView;

        public bool HasChanges
        {
            get => _sourceEditor.HasChanges;
        }

        public event EventHandler<EventArgs> HasChangesChanged;

        public ComputerEditor(ComputerEditorEnviroment environment)
        {
            InitializeComponent();

            Enviroment = environment;

            string name = Path.GetFileNameWithoutExtension(environment.FilePath);
            IConsole console = new ComputerOutput(environment.Output);

            Computer = new Computer()
            {
                StandardOut = console
            };

            _sourceEditor = new SourceEditor(Computer.Messenger);
            _expressionView = new ExpressionView();
            _operationsView = new OperationsView();

            _executor = Computer.CreateExecutor();

            _localsView = environment.LocalsView;
            _localsView.Target = _executor;
            _messageView = environment.MessageView;
            _messageView.Target = Computer.Messenger;
            _sourceEditor.Executor = _executor;
            
            _expressionView.ShowDebug = true;

            MainContent.Content = _sourceEditor;

            _filePath = environment.FilePath;
            _sourceEditor.Text = File.ReadAllText(environment.FilePath);
            Compile();

            SetupBindings();

            _executor.Paused += Executor_OnPaused;
            _executor.Stopped += Executor_OnStopped;

            _sourceEditor.FinishedEditing += (obj, e) =>
            {
                Dispatcher.Invoke(Compile);
            };

            _sourceEditor.HasChangesChanged += (obj, e) =>
            {
                OnHasChangesChanged();
            };

            UpdateMode(ComputerEditorMode.Source);
        }

        protected virtual void OnHasChangesChanged()
        {
            HasChangesChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            Enviroment.Output.End();
        }

        public void Compile()
        {
            Computer.Source = _sourceEditor.Text;

            Logs.Core.LogDebug("Compiling...");

            if (Computer.Compile())
                Logs.Core.LogDebug("Compilation successful.");
            else
                Logs.Core.LogDebug("Compilation failed.");

            _sourceEditor.Update(Computer.Compiler);
            _sourceEditor.Redraw();

            _messageView.Update();

            _expressionView.Root = Computer
                .Compiler
                .ExpressionTree
                .Root;

            _operationsView.Target = Computer
                .Executable;

            _executor.Reset();
        }

        private void SetupBindings()
        {
            bool Running() => _executor.State == ExecutorState.Running;
            bool Paused() => _executor.State == ExecutorState.Paused;
            bool Idle() => _executor.State == ExecutorState.Idle;

            bool IdleOrPaused() => Paused() || Idle();
            bool RunningOrPaused() => Running() || Paused();

            Bind(this, EditorCommands.Save, Save);

            Bind(this, EditorCommands.ViewCode, ShowSource);
            Bind(this, EditorCommands.ViewExpressionTree, ShowExpressionTree);
            Bind(this, EditorCommands.ViewOperations, ShowExecutable);

            Bind(this, EditorCommands.StartDebug, StartOrContinue, IdleOrPaused);
            Bind(this, EditorCommands.Stop, _executor.RequestStop, RunningOrPaused);
            Bind(this, EditorCommands.Pause, _executor.RequestPause, Running);
            Bind(this, EditorCommands.StepOver, StepOver, Paused);
            Bind(this, EditorCommands.StepInto, StepInto, IdleOrPaused);
            Bind(this, EditorCommands.StepOut, StepOut, Paused);

            Bind(this, EditorCommands.ToggleBreakpoint, ToggleBreakpoint);
        }

        public void Save()
        {
            Logs.Core.LogInformation("Saving " + _filePath);
            File.WriteAllText(_filePath, _sourceEditor.Text);
            _sourceEditor.ResetDiff();
        }

        public void ShowSource()
        {
            UpdateMode(ComputerEditorMode.Source);
        }

        public void ShowExpressionTree()
        {
            UpdateMode(ComputerEditorMode.Expressions);
        }

        public void ShowExecutable()
        {
            UpdateMode(ComputerEditorMode.Operations);
        }

        private void UpdateMode(ComputerEditorMode mode)
        {
            Mode = mode;

            switch (Mode)
            {
                case ComputerEditorMode.Source:
                    MainContent.Content = _sourceEditor;
                    _executor.StepMode = ExecutorStepMode.Source;
                    break;

                case ComputerEditorMode.Expressions:
                    MainContent.Content = _expressionView;
                    _executor.StepMode = ExecutorStepMode.Source;
                    break;

                case ComputerEditorMode.Operations:
                    MainContent.Content = _operationsView;
                    _executor.StepMode = ExecutorStepMode.Operation;
                    break;
            }
        }

        public void StartOrContinue()
        {
            if (_executor.State == ExecutorState.Idle)
                Task.Run(_executor.Start);
            else
                Task.Run(_executor.Continue);
        }

        public void StepInto()
        {
            PerformExecutorAction(_executor.StepInto);
        }

        public void StepOut()
        {
            PerformExecutorAction(_executor.StepOutOf);
        }

        public void StepOver()
        {
            PerformExecutorAction(_executor.StepOver);
        }

        private void PerformExecutorAction(Action action)
        {
            Task.Run(action);
        }

        private void Executor_OnPaused(object sender, EventArgs e)
        {
            void Action()
            {
                _operationsView.Highlight(_executor.Instructions.Index);
                _localsView.Update();

                Token token = _executor.Instructions.CurrentInfo.FocusToken;
                int lineNumber = token.LineNumber;
                int columnNumber = token.ColumnStart;
                _sourceEditor.EnsureLineIsVisible(lineNumber, columnNumber);
                _sourceEditor.Redraw();
            }

            Dispatcher.Invoke(Action);
        }

        private void Executor_OnStopped(object sender, EventArgs e)
        {
            void Action()
            {
                _operationsView.ClearHighlight();
                _localsView.Clear();
                _sourceEditor.Redraw();
            }

            Dispatcher.Invoke(Action);
        }

        private void ToggleBreakpoint()
        {
            switch (Mode)
            {
                case ComputerEditorMode.Operations:
                    _operationsView.ToggleBreakpoint();
                    break;

                case ComputerEditorMode.Expressions:
                    break;

                case ComputerEditorMode.Source:
                    Expression current = GetStatementAtCursor();
                    ToggleBreakpoint(current);
                    break;
            }

            _sourceEditor.Update(Computer.Compiler);
            _sourceEditor.Redraw();
        }

        private void ToggleBreakpoint(Expression expression)
        {
            OpDebugInfo opInfo = expression.OperatorsHint.FirstOrDefault();
            if (opInfo != null)
            {
                opInfo.Breakpoint = !opInfo.Breakpoint;
                _sourceEditor.Redraw();
                _operationsView.Update();
            }
        }

        private Expression GetStatementAtCursor()
        {
            int location = _sourceEditor.CaretOffset;

            // Is there a better way of doing this?
            // Definitely. But it'll do.

            // It's kinda like a binary search in disguise anyways because
            // of the tree structure of expressions.

            Expression FindExpression(Expression current)
            {
                if (!(current is Block))
                    return current;

                foreach (Expression child in current.GetSubExpressions())
                {
                    if (child.StartToken == null || child.EndToken == null)
                        continue;

                    if (location > child.EndToken.End)
                        continue;

                    current = FindExpression(child);
                    break;
                }

                return current;
            }

            Expression root = Computer.Compiler.ExpressionTree.Root;
            return FindExpression(root);
        }
    }
}
