using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NSprak.Expressions;
using NSprak.Expressions.Types;
using NSprak.Functions;
using NSprak.Functions.Resolution;
using NSprak.Operations;
using NSprak.Operations.Types;

using EReturn = NSprak.Expressions.Types.Return;
using OReturn = NSprak.Operations.Types.Return;

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

        private readonly ExecutionContext _context;
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

            _context = new ExecutionContext(computer, signatures);

            _context.Reset();
            State = ExecutorState.Idle;
        }

        public void Reset()
        {
            if (State != ExecutorState.Stopped && State != ExecutorState.Idle)
                return;

            _context.Reset();
            State = ExecutorState.Idle;

            Computer.Screen?.SetColor(Color.White);
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

        public void Start()
        {
            if (State != ExecutorState.Idle) return;

            Instructions.Step();
            
            Run(StepIndefinitely, Stop, stepInto: false);
        }

        public void Continue()
        {
            if (State != ExecutorState.Paused) return;
            Run(StepIndefinitely, Stop);
        }

        public void StepInto()
        {
            if (State != ExecutorState.Paused && State != ExecutorState.Idle)
                return;

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
                Instructions.Current.Execute(_context);

                if (step)
                    Instructions.Step();
            }
            catch (Exception e)
            {
                string sourceTrace = null;

                if (Instructions.HasCurrent)
                    sourceTrace = "at " + Instructions.CurrentInfo?.SourceExpression?.GetTraceString();


                string opString = Instructions.HasCurrent ? Instructions.Current.ToString() : "null";

                string message = $"Exception occured while executing op {Instructions.Index}: {opString}";
                string exception = e.ToString();

                Computer.Screen?.SetColor(Color.Red);
                Computer.Screen?.Print(message);
                Computer.Screen?.Print(sourceTrace);
                Computer.Screen?.Print(exception);

                _breakRequested = true;
            }
        }
    }
}
