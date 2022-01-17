using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

using NSprak.Exceptions;
using NSprak.Expressions;
using NSprak.Expressions.Types;
using NSprak.Functions;
using NSprak.Functions.Resolution;
using NSprak.Tests.Types;
using NSprak.Tests;
using NSprak.Messaging;

namespace NSprak.Execution
{
    public enum ExecutorState
    {
        Running,
        Paused,
        Stopped,
        Idle
    }
    public enum ExecutorStepMode
    {
        Operation,
        Source
    }

    public class Executor
    {
        public ExecutorStepMode StepMode { get; set; }

        public ExecutorState State { get; private set; }

        // If this is true, and the executor is paused,
        // stepping or continuing may cause an internal 
        // runtime error due to the memory being in an invalid state.
        // Only stopping should be allowed from here.
        public bool HasUnsafeError { get; set; }

        private readonly ExecutionContext _context;

        private bool _runTestCommands;

        private bool _stopRequested;
        private bool _pauseRequested;

        private bool _breakRequested;
        private bool _skipBreak;

        public Computer Computer => _context.Computer;
        public Executable Executable => _context.Executable;
        public Memory Memory => _context.Memory;
        public InstructionEnumerator Instructions => _context.Instructions;

        public event EventHandler Stopped;
        public event EventHandler Paused;

        public Executor(Computer computer)
        {
            // At some point the list of library names should be stored
            // in the executable... but there is only one library at the moment.
            List<Library> libraries = new List<Library>
            {
                Library.Core
            };

            AssignmentResolver assignments = new AssignmentResolver(libraries);
            SignatureResolver signatures = new SignatureResolver(libraries, assignments);

            signatures.SpecifyUserFunctions(computer.Executable.FunctionDeclarations);
            signatures.SpecifyOperationBindings(new List<OperationBinding>());

            _context = new ExecutionContext(computer, signatures);

            _context.Reset();
            State = ExecutorState.Idle;
        }

        public void Reset()
        {
            if (State != ExecutorState.Stopped && State != ExecutorState.Idle)
                return;

            _context.Reset();
            HasUnsafeError = false;
            State = ExecutorState.Idle;

            Computer.Screen?.SetColor(Color.White);
            Computer.Screen?.SetPrintColor(Color.White);

            _runTestCommands = false;
        }

        protected void OnPause(EventArgs e)
        {
            Paused?.Invoke(this, e);
        }

        protected void OnStop(EventArgs e)
        {
            Stopped?.Invoke(this, e);
        }

        public void RequestPause()
        {
            if (State == ExecutorState.Running)
            {
                _context.Computer.Screen.CancelInput();
                _pauseRequested = true;
            }
        }

        public void RequestStop()
        {
            switch (State)
            {
                case ExecutorState.Running:
                    _context.Computer.Screen.CancelInput();
                    _stopRequested = true;
                    break;

                case ExecutorState.Paused:
                    Stop();
                    break;       
            }
        }

        public void Start() => Start(false);

        public void StartTest() => Start(true);

        private void Start(bool assertions)
        {
            if (State != ExecutorState.Idle) return;

            _runTestCommands = assertions;

            Instructions.Step();
            
            Run(StepIndefinitely, Stop, stepInto: false);
        }

        public void Continue()
        {
            if (State != ExecutorState.Paused) return;
            Run(StepIndefinitely, Stop);
        }

        public void StepInto() => StepInto(false);

        public void StepIntoTest() => StepInto(true);

        private void StepInto(bool assertions)
        {
            if (State != ExecutorState.Paused && State != ExecutorState.Idle)
                return;

            _runTestCommands = assertions;

            Run(StepSingle, Pause);
        }

        public void StepOver()
        {
            if (!Instructions.HasCurrent)
                StepInto();

            else
            {
                void Action()
                {
                    int startDepth = _context.Memory.Frames.Count;
                    StepSingle();

                    bool endCondition() => (!Instructions.HasCurrent)
                        || _context.Memory.Frames.Count <= startDepth;

                    StepUntil(endCondition);
                }

                Run(Action, Pause);
            }
        }

        public void StepOutOf()
        {
            if (!Instructions.HasCurrent)
                StepInto();

            else
            {
                void Action()
                {
                    int startDepth = _context.Memory.Frames.Count;

                    bool endCondition() => (!Instructions.HasCurrent)
                        || _context.Memory.Frames.Count < startDepth;

                    StepUntil(endCondition);
                }

                Run(Action, Pause);
            }
        }

        private void Run(Action action, Action end, bool stepInto = true)
        {
            State = ExecutorState.Running;

            Expression startExpression = GetExpression();

            if (stepInto && Instructions.Index == -1)
                Instructions.Step();

            else action();

            ApplyStepThrough(startExpression);

            if (_stopRequested || _context.ExitRequested) Stop();
            else if (_pauseRequested || _breakRequested) Pause();
            else end();
        }

        private void Stop()
        {
            State = ExecutorState.Stopped;

            _breakRequested = false;
            _skipBreak = false;

            _stopRequested = false;
            _pauseRequested = false;

            Reset();

            OnStop(new EventArgs());
        }

        private void Pause()
        {
            if (!Instructions.HasCurrent)
            {
                Stop();
                return;
            }

            State = ExecutorState.Paused;

            _breakRequested = false;
            _skipBreak = true;

            _pauseRequested = false;
            _stopRequested = false;

            OnPause(new EventArgs());
        }

        private void StepSingle()
        {
            if (Instructions.Index == -1)
            {
                Instructions.Step();
                return;
            }

            if (Instructions.HasCurrent && !_context.ExitRequested)
                ExecuteCurrent();
        }

        private Expression GetExpression()
        {
            if (!Instructions.HasCurrent)
                return null;

            return Instructions.CurrentInfo.SourceExpression;
        }

        private void ApplyStepThrough(Expression startExpression)
        {
            if (StepMode == ExecutorStepMode.Operation)
                return;

            bool EndCondition()
            {
                Expression expression = GetExpression();
                if (expression == null)
                    return true;

                if (startExpression != null)
                    if (expression == startExpression)
                        return false;

                if (expression is Block)
                    return false;

                if (expression is FunctionCall)
                    return true;

                return expression.ParentBlockHint == expression.ParentHint;
            }

            StepUntil(EndCondition);
        }

        private void StepIndefinitely()
        {
            StepUntil(() => false);
        }

        private void StepUntil(Func<bool> condition)
        {
            while (Instructions.HasCurrent)
            {
                if (_pauseRequested || _stopRequested || _breakRequested)
                    break;

                if (_context.ExitRequested)
                    break;

                if (condition())
                    break;

                ExecuteCurrent();
            }
        }

        private void ExecuteCurrent()
        {
            if (Instructions.CurrentInfo.Breakpoint)
            {
                if (_skipBreak)
                    _skipBreak = false;

                else
                {
                    _breakRequested = true;
                    return;
                }
            }

            bool step = Instructions.Current.StepAfterwards;

            try
            {
                if (_runTestCommands)
                    InvokeTestCommands(pre: true);

                Instructions.Current.Execute(_context);

                if (_runTestCommands)
                    InvokeTestCommands(pre: false);

                if (step)
                    Instructions.Step();
            }
            catch (Exception e)
            {
                string sourceTrace = null;

                IComputerScreen screen = Computer.Screen;
                if (screen != null)
                {
                    if (e is SprakRuntimeException runtimeException)
                        HandleRuntimeError(runtimeException, screen);

                    else
                    {
                        screen.SetPrintColor(Color.Red);
                        screen.Print("Internal error: " + e.GetType().Name);
                        screen.Print(e.Message);
                        screen.Print(e.StackTrace);

                        if (Instructions.HasCurrent)
                            screen.Print("Op: " + Instructions.Current.ToString());

                        if (sourceTrace != null)
                            screen.Print("Expression: " + sourceTrace);

                        HasUnsafeError = true;
                        _breakRequested = true;
                    }
                }
                else
                {
                    // Ideally the error would be reported with some sort of log
                    // as well, but there is no logging mechanism at the moment,
                    // so the best that can be done without an output screen is to stop.
                    HasUnsafeError = true;
                    _breakRequested = true;
                }
            }
        }

        private void InvokeTestCommands(bool pre)
        {
            if (!Instructions.HasCurrent)
                return;

            IEnumerable<TestCommand> commands = Instructions.CurrentInfo.Tests;
            if (commands == null)
                return;

            IComputerScreen screen = Computer.Screen;

            foreach (TestCommand command in commands)
            {
                if (pre != command.IsPreOp)
                    continue;

                try
                {
                    command.Invoke(_context);

                    if (screen != null)
                    {
                        screen.SetPrintColor(Color.Green);
                        screen.Print("Assertion passed: " + command.Description);
                        screen.SetPrintColor(Color.White);
                    }
                }
                catch (SprakRuntimeException e)
                {
                    // Since operations can't be reversed as of writing this, failing 
                    // an assertion after the operation is complete leaves the execution in
                    // in a state where it cannot try the operation and assertion again.
                    HasUnsafeError = true;

                    // Add a bit of context, and then let the standard runtime error
                    // handling handle things.
                    e.Context = "While executing assertion: " + command.Description;
                    throw;
                }
            }
        }

        private void HandleRuntimeError(SprakRuntimeException e, IComputerScreen screen)
        {
            // First, consider assertions - the error could be expected, in which case the executor can log
            // a success and move on. Or it could be a different error to what was expected, so that the 
            // assertion fails.

            ErrorTest test = null;
            if (Instructions.HasCurrent) 
                test = Instructions.CurrentInfo.Tests?.OfType<ErrorTest>().FirstOrDefault();

            if (test != null && _runTestCommands)
            {
                if (test.ErrorName == e.Template.Name)
                {
                    // Success! The error was expected.
                    screen.SetPrintColor(Color.Green);
                    screen.Print($"Assertion passed: error '{test.ErrorName}' occured.");
                    Instructions.Step();
                }
                // This name comparison is a bit awkward, but doesn't seem worth rewriting
                else if (e.Template.Name != nameof(Messages.AssertionFailed))
                {
                    // An error occurred as expected, but it was the wrong one!
                    screen.SetPrintColor(Color.Red);
                    screen.Print("Runtime error: Assertion Failed.");
                    screen.Print($"Expected error: '{test.ErrorName}'. Found error: '{e.Template.Name}'");
                    screen.Print("Found error details:");

                    DisplayRuntimeError(e, screen);
                    _breakRequested = true;
                }
                else
                {
                    DisplayRuntimeError(e, screen);
                    _breakRequested = true;
                }
            }
            else
            {
                // If there was no error test, then this is a runtime error
                // to display and pause on.
                DisplayRuntimeError(e, screen);
                _breakRequested = true;
            }

            screen.SetPrintColor(Color.White);
        }

        private void DisplayRuntimeError(SprakRuntimeException e, IComputerScreen screen)
        {


            screen.SetPrintColor(Color.Red);
            screen.Print("Runtime Error: " + e.Template.Title);

            screen.Print("Detail: " + e.Template.Render(e.Args));

            string sourceTrace = GetCurrentTrace();
            if (sourceTrace != null)
                screen.Print("At: " + sourceTrace);

            screen.Print("Execution paused at error location.");
            screen.SetPrintColor(Color.White);
        }

        private string GetCurrentTrace()
        {
            string sourceTrace = null;
            if (Instructions.HasCurrent)
                sourceTrace = Instructions.CurrentInfo?.SourceExpression?.GetTraceString();

            return sourceTrace;
        }
    }
}
