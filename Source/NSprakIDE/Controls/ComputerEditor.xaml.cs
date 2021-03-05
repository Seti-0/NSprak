using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;

using NSprak;
using NSprak.Execution;
using NSprak.Tokens;
using NSprakIDE.Commands;
using NSprakIDE.Controls.General;
using NSprakIDE.Controls.Output;

namespace NSprakIDE.Controls
{
    using static NSprakIDE.Commands.CommandHelper;

    public class ComputerEditorEnviroment
    {
        public string FilePath;

        public OutputView OutputView;

        public LocalsView LocalsView;

        public CallStackView CallStackView;
    }

    public enum ComputerEditorMode
    {
        Source, Expressions, Operations
    }

    /// <summary>
    /// Interaction logic for ComputerEditor.xaml
    /// </summary>
    public partial class ComputerEditor : UserControl
    {
        public ComputerEditorEnviroment Enviroment { get; }

        public ComputerEditorMode Mode { get; private set; }

        public Computer Computer { get; }

        private Executor _executor;

        private SourceEditor _sourceEditor;
        private ExpressionView _expressionView;
        private OperationsView _operationsView;

        private LocalsView _localsView;

        public ComputerEditor(ComputerEditorEnviroment enviroment)
        {
            InitializeComponent();

            Enviroment = enviroment;

            string name = Path.GetFileNameWithoutExtension(enviroment.FilePath);
            OutputLog log = enviroment.OutputView.StartLog(MainWindow.ComputerLogCategory, name);
            IConsole console = new ComputerOutput(log);

            Computer = new Computer()
            {
                StandardOut = console
            };

            _sourceEditor = new SourceEditor();
            _expressionView = new ExpressionView();
            _operationsView = new OperationsView();

            _executor = Computer.CreateExecutor();

            _localsView = enviroment.LocalsView;
            _localsView.Target = _executor;

            _expressionView.ShowDebug = true;

            MainContent.Content = _sourceEditor;

            _sourceEditor.Text = File.ReadAllText(enviroment.FilePath);
            Compile();

            SetupBindings();

            _executor.Paused += Executor_OnPaused;
            _executor.Stopped += Executor_OnStopped;
        }

        private void SetupBindings()
        {
            bool Running() => _executor.State == ExecutorState.Running;
            bool Paused() => _executor.State == ExecutorState.Paused;
            bool Idle() => _executor.State == ExecutorState.Idle;

            bool IdleOrPaused() => Paused() || Idle();
            bool RunningOrPaused() => Running() || Paused();

            Bind(this, EditorCommands.ViewCode, ShowSource);
            Bind(this, EditorCommands.ViewExpressionTree, ShowExpressionTree);
            Bind(this, EditorCommands.ViewOperations, ShowExecutable);

            Bind(this, EditorCommands.StartDebug, StartOrContinue, IdleOrPaused);
            Bind(this, EditorCommands.Stop, _executor.RequestStop, RunningOrPaused);
            Bind(this, EditorCommands.Pause, _executor.RequestPause, Running);
            Bind(this, EditorCommands.StepOver, StepOver, Paused);
            Bind(this, EditorCommands.StepInto, StepInto, IdleOrPaused);
            Bind(this, EditorCommands.StepOut, StepOut, Paused);

            Bind(this, EditorCommands.ToggleBreakpoint, _operationsView.ToggleBreakpoint);
        }

        private void ToggleBreakpoint()
        {
            if (Mode == ComputerEditorMode.Operations)
                _operationsView.ToggleBreakpoint();

            switch (Mode)
            {
                case ComputerEditorMode.Operations:
                    _operationsView.ToggleBreakpoint();
                    break;

                case ComputerEditorMode.Expressions:
                    break;

                case ComputerEditorMode.Source:
                    break;
            }
        }

        private void UpdateMode(ComputerEditorMode mode)
        {
            Mode = mode;

            switch (Mode)
            {
                case ComputerEditorMode.Source:
                    MainContent.Content = _sourceEditor;
                    _executor.StepMode = ExecutorStepMode.Expression;
                    break;

                case ComputerEditorMode.Expressions:
                    MainContent.Content = _expressionView;
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
            _executor.StepMode = Mode switch
            {
                ComputerEditorMode.Operations => ExecutorStepMode.Operation,
                _ => ExecutorStepMode.Operation
            };

            if (Mode == ComputerEditorMode.Expressions)
                return;

            Task.Run(action);
        }

        private void Executor_OnPaused(object sender, EventArgs e)
        {
            void Action()
            {
                _operationsView.Highlight(_executor.Instructions.Index);
                _localsView.Update();
            }

            Dispatcher.Invoke(Action);
        }

        private void Executor_OnStopped(object sender, EventArgs e)
        {
            void Action()
            {
                _operationsView.ClearHighlight();
                _localsView.Clear();
            }

            Dispatcher.Invoke(Action);
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

        public void Compile()
        {
            Computer.Source = _sourceEditor.Text;
            Computer.Compile();

            _sourceEditor.Update(Computer.Compiler);
            _sourceEditor.Redraw();

            _expressionView.Root = Computer
                .Compiler
                .ExpressionTree
                .Root;

            _operationsView.Target = Computer
                .Executable;

            _executor.Reset();
        }
    }
}
