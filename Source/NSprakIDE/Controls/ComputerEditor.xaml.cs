using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Linq;

using NSprak;
using NSprak.Tokens;
using NSprak.Expressions;
using NSprak.Expressions.Types;
using NSprak.Execution;
using NSprak.Operations;

using NSprakIDE.Commands;
using NSprakIDE.Controls.General;
using NSprakIDE.Controls.Screen;

namespace NSprakIDE.Controls
{
    using static NSprakIDE.Commands.CommandHelper;

    public class ComputerEditorEnviroment
    {
        public string Name;

        public string GivenID;

        public string FilePath;

        public LocalsView LocalsView;

        public CallStackView CallStackView;

        public MessageView MessageView;

        public ScreenView ScreenView;
    }

    public enum ComputerEditorMode
    {
        Source, Expressions, Operations
    }

    public partial class ComputerEditor : UserControl, IDisposable, ICommandButtonHost
    {
        public ComputerEditorEnviroment Environment { get; }

        public ComputerEditorMode Mode { get; private set; }

        public Computer Computer { get; }

        public string EditorName { get; }

        public string GivenID { get; }

        private readonly Executor _executor;

        private readonly string _filePath;

        private readonly SourceEditor _sourceEditor;
        private readonly ExpressionView _expressionView;
        private readonly OperationsView _operationsView;

        public bool HasChanges
        {
            get => _sourceEditor.HasChanges;
        }

        public event EventHandler<EventArgs> HasChangesChanged;
        public event EventHandler<EventArgs> CommandContextChanged;

        public event EventHandler<EventArgs> Closing;
        public event EventHandler<EventArgs> Compiled;
        public event EventHandler<EventArgs> DebuggerPaused;
        public event EventHandler<EventArgs> DebuggerStopped;

        public ComputerEditor(ComputerEditorEnviroment environment)
        {
            InitializeComponent();

            GivenID = environment.GivenID;
            EditorName = environment.Name;

            Environment = environment;

            ComputerScreen screen = Environment.ScreenView.Supplier.Start(
                    Environment.GivenID,
                    environment.Name,
                    MainWindow.ComputerLogCategory
                );

            Computer = new Computer()
            {
                Screen = screen
            };

            Environment.MessageView.Supplier.Start(
                Computer.Messenger,
                environment.GivenID,
                environment.Name,
                MainWindow.ComputerLogCategory
            );

            _executor = Computer.CreateExecutor();
            Environment.CallStackView.Target = _executor;

            _sourceEditor = new SourceEditor(Computer.Messenger);
            _expressionView = new ExpressionView();
            _operationsView = new OperationsView();

            _sourceEditor.Executor = _executor;

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
            Environment.ScreenView.Supplier.End(Environment.GivenID);
            Environment.MessageView.Supplier.End(Environment.GivenID);
            Environment.LocalsView.Supplier.End(Environment.GivenID);
            OnClosing();
        }

        protected virtual void OnClosing()
        {
            Closing?.Invoke(this, EventArgs.Empty);
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

            Environment.MessageView.Update();

            _expressionView.Root = Computer
                .Compiler
                .ExpressionTree
                .Root;

            _operationsView.Target = Computer
                .Executable;

            _executor.Reset();

            OnCompiled();
        }

        protected virtual void OnCompiled()
        {
            Compiled?.Invoke(this, EventArgs.Empty);
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
            Logs.Core.LogInformation("Saving '" + Path.GetFileName(_filePath) + "'");
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

            Environment.LocalsView.Update();
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
                ShowLocalsView();
                Environment.LocalsView.Update();
                Environment.CallStackView.Update();

                Token token = _executor.Instructions.CurrentInfo.FocusToken;
                if (token != null)
                {
                    int lineNumber = token.LineNumber;
                    int columnNumber = token.ColumnStart;
                    _sourceEditor.EnsureLineIsVisible(lineNumber, columnNumber);
                    _sourceEditor.Redraw();
                }

                CommandContextChanged?.Invoke(this, EventArgs.Empty);
                InvalidateVisual();

                OnDebuggerPaused();
            }

            Dispatcher.Invoke(Action);
        }

        protected virtual void OnDebuggerPaused()
        {
            DebuggerPaused?.Invoke(this, EventArgs.Empty);
        }

        private void Executor_OnStopped(object sender, EventArgs e)
        {
            void Action()
            {
                Environment.LocalsView.Update();
                _operationsView.ClearHighlight();
                _sourceEditor.Redraw();
                CommandContextChanged?.Invoke(this, EventArgs.Empty);
                InvalidateVisual();

                HideLocalsView();

                OnDebuggerStopped();
            }

            Dispatcher.Invoke(Action);
        }

        protected virtual void OnDebuggerStopped()
        {
            DebuggerStopped?.Invoke(this, EventArgs.Empty);
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

        private void ShowLocalsView()
        {
            ViewSupplier<Executor> supplier = Environment.LocalsView.Supplier;
            string key = Environment.GivenID;

            if (supplier.ContainsKey(key))
                return;

            Environment.LocalsView.Supplier.Start(
                _executor,
                Environment.GivenID,
                Environment.Name,
                MainWindow.ComputerLogCategory
            );
        }

        private void HideLocalsView()
        {
            Environment.LocalsView.Supplier.End(Environment.GivenID);
        }
    }
}
