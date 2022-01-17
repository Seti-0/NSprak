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

        public string TempPath;

        public MessageView MessageView;

        public ScreenView ScreenView;

        public MemoryView MemoryView;
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
        private readonly MemoryViewContext _memoryContext;

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

            _memoryContext = new MemoryViewContext(_executor);
            _memoryContext.Changed += MemoryContext_Changed;

            _sourceEditor = new SourceEditor(Computer.Messenger);
            _expressionView = new ExpressionView();
            _operationsView = new OperationsView();

            MainContent.Content = _sourceEditor;

            SetupBindings();

            _executor.Paused += Executor_OnPaused;
            _executor.Stopped += Executor_OnStopped;

            _sourceEditor.FinishedEditing += (obj, e) =>
            {
                Dispatcher.Invoke(Compile);
            };

            UpdateMode(ComputerEditorMode.Source);

            LoadTemp();

            _sourceEditor.HasChangesChanged += (obj, e) =>
            {
                OnHasChangesChanged(EventArgs.Empty);
            };

            // Run the program once on opening.
            Compile();
            StartOrContinue();
        }

        protected virtual void OnHasChangesChanged(EventArgs e)
        {
            HasChangesChanged?.Invoke(this, e);
        }

        public void Dispose()
        {
            Environment.ScreenView.Supplier.End(Environment.GivenID);
            Environment.MessageView.Supplier.End(Environment.GivenID);
            Environment.MemoryView.Supplier.End(Environment.GivenID);

            File.Delete(Environment.TempPath);
            string currentDir = Path.GetDirectoryName(Environment.TempPath);
            while (Directory.Exists(currentDir) && Directory.GetFiles(currentDir).Length == 0)
            {
                Directory.Delete(currentDir);
                currentDir = Path.GetDirectoryName(currentDir);
            }

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

            SaveTemp();

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
            bool Safe() => !_executor.HasUnsafeError;

            bool RunningOrPaused() => Running() || Paused();
            bool SafelyPaused() => Paused() && Safe();
            bool IdleOrSafelyPaused() => Idle() || (Paused() && Safe());

            Bind(this, EditorCommands.Save, Save);

            Bind(this, EditorCommands.ViewCode, ShowSource);
            Bind(this, EditorCommands.ViewExpressionTree, ShowExpressionTree);
            Bind(this, EditorCommands.ViewOperations, ShowExecutable);

            Bind(this, EditorCommands.StartDebug, StartOrContinue, IdleOrSafelyPaused);
            Bind(this, EditorCommands.StartTest, StartTest, Idle);
            Bind(this, EditorCommands.Stop, _executor.RequestStop, RunningOrPaused);
            Bind(this, EditorCommands.Pause, _executor.RequestPause, Running);
            Bind(this, EditorCommands.StepOver, StepOver, SafelyPaused);
            Bind(this, EditorCommands.StepInto, StepInto, IdleOrSafelyPaused);
            Bind(this, EditorCommands.StepOut, StepOut, SafelyPaused);

            Bind(this, EditorCommands.ToggleBreakpoint, ToggleBreakpoint);
        }

        public void Save()
        {
            Logs.Core.LogInformation("Saving '" + Path.GetFileName(Environment.FilePath) + "'");
            
            Directory.CreateDirectory(Path.GetDirectoryName(Environment.FilePath));
            File.WriteAllText(Environment.FilePath, _sourceEditor.Text);
            
            _sourceEditor.ResetDiff();
        }

        public void SaveTemp()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Environment.TempPath));   
            File.WriteAllText(Environment.TempPath, _sourceEditor.Text);
        }

        public void Load()
        {
            string content = File.ReadAllText(Environment.FilePath);
            _sourceEditor.SetText(content, resetDiff: true);
        }

        public void LoadTemp()
        {
            Load();

            if (File.Exists(Environment.TempPath))
            {
                string content = File.ReadAllText(Environment.TempPath);
                _sourceEditor.SetText(content, resetDiff: false);
            }
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

            Environment.MemoryView.Update();
        }

        public void StartOrContinue()
        {
            if (_executor.State == ExecutorState.Idle)
                Task.Run(_executor.Start);
            else
                Task.Run(_executor.Continue);

            ClearRuntimeHighlights();
        }

        public void StartTest()
        {
            Task.Run(_executor.StartTest);
            ClearRuntimeHighlights();
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
                ShowDebugViews();
                Environment.MemoryView.Update();
                UpdateRuntimeHighlights();

                CommandContextChanged?.Invoke(this, EventArgs.Empty);
                OnDebuggerPaused();
            }

            Dispatcher.Invoke(Action);
        }

        public void ClearRuntimeHighlights()
        {
            _operationsView.ClearHighlight();
            _sourceEditor.ClearHighlight();
        }

        private void UpdateRuntimeHighlights()
        {
            if (_executor.State != ExecutorState.Paused)
            {
                ClearRuntimeHighlights();
                return;
            }

            if (_executor.Executable.InstructionCount == 0)
                return;

            int opIndex = _memoryContext.Frame.Location;
            
            if (opIndex < 0)
                opIndex = 0;

            int lineNumber, columnNumber;

            if (opIndex < _executor.Executable.InstructionCount)
            {
                _operationsView.Highlight(opIndex);

                _sourceEditor.Highlight(opIndex);
                _sourceEditor.Redraw();

                Token focus = _executor.Executable.DebugInfo[opIndex].FocusToken;
                if (focus != null)
                {
                    lineNumber = focus.LineNumber;
                    columnNumber = focus.ColumnStart;
                    _sourceEditor.EnsureLineIsVisible(lineNumber, columnNumber);
                }
            }
            else
            {
                _operationsView.ClearHighlight();
                _sourceEditor.ClearHighlight();
                _sourceEditor.ScrollToEnd();

            }

            InvalidateVisual();
        }

        protected virtual void OnDebuggerPaused()
        {
            DebuggerPaused?.Invoke(this, EventArgs.Empty);
        }

        private void Executor_OnStopped(object sender, EventArgs e)
        {
            void Action()
            {
                _operationsView.ClearHighlight();
                _sourceEditor.Redraw();
                CommandContextChanged?.Invoke(this, EventArgs.Empty);
                ClearRuntimeHighlights();
                InvalidateVisual();

                HideDebugViews();

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

        private void ShowDebugViews()
        {
            string key = Environment.GivenID;

            ViewSupplier<MemoryViewContext> memory
                = Environment.MemoryView.Supplier;

            if (!memory.ContainsKey(key))
                memory.Start(
                    _memoryContext,
                    key,
                    Environment.Name,
                    MainWindow.ComputerLogCategory);
        }

        private void HideDebugViews()
        {
            Environment.MemoryView.Supplier.End(Environment.GivenID);
        }

        private void CallStackContext_Changed(object sender, EventArgs e)
        {
            UpdateRuntimeHighlights();
        }

        private void MemoryContext_Changed(object sender, EventArgs e)
        {
            UpdateRuntimeHighlights();
        }
    }
}
