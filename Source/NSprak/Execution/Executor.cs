using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using NSprak.Expressions;
using NSprak.Expressions.Types;
using NSprak.Language;
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
        Expression
    }

    public class Executor
    {
        public ExecutorStepMode StepMode { get; set; }

        public ExecutorState State { get; private set; }

        private ExecutionContext _context;
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

        public Executor(Computer computer, SignatureResolver resolver)
        {
            _context = new ExecutionContext(computer, resolver);

            _context.Reset();
            State = ExecutorState.Idle;
        }

        public void Reset()
        {
            if (State != ExecutorState.Stopped && State != ExecutorState.Idle)
                return;

            _context.Reset();
            State = ExecutorState.Idle;

            Computer.StandardOut?.SetColor(System.Drawing.Color.White);
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
                _pauseRequested = true;
        }

        public void RequestStop()
        {
            switch (State)
            {
                case ExecutorState.Running:
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

        public void StepOver()
        {
            if (!Instructions.HasCurrent)
                StepInto();

            else
            {
                int startDepth = _context.Memory.Frames.Count;
                StepInto();

                while (Instructions.HasCurrent && _context.Memory.Frames.Count > startDepth)
                    StepInto();
            }
        }

        public void StepOutOf()
        {
            if (!Instructions.HasCurrent)
                StepInto();

            else
            {
                int startDepth = _context.Memory.Frames.Count;
                StepInto();

                while (Instructions.HasCurrent && _context.Memory.Frames.Count >= startDepth)
                    StepInto();
            }
        }

        public void StepInto()
        {
            if (State != ExecutorState.Paused && State != ExecutorState.Idle)
                return;

            if (StepMode == ExecutorStepMode.Operation || !Instructions.HasCurrent)
            {
                Run(StepSingle, Pause);
            }
            else
            {
                Expression startSource = Instructions.CurrentInfo.SourceExpression;
                bool newExpression() => Instructions.CurrentInfo.SourceExpression != startSource;

                Run(() => StepUntil(newExpression), Pause);
            }
        }

        private void Run(Action action, Action end, bool stepInto = true)
        {
            State = ExecutorState.Running;

            if (stepInto && Instructions.Index == -1)
                Instructions.Step();

            else
                action();

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

            OnStop(new EventArgs());

            Reset();
        }

        private void Pause()
        {
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

        private void StepIndefinitely()
        {
            StepUntil(() => true);
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

                Computer.StandardOut?.SetColor(System.Drawing.Color.Red);
                Computer.StandardOut?.Print(message);
                Computer.StandardOut?.Print(sourceTrace);
                Computer.StandardOut?.Print(exception);

                _breakRequested = true;
            }
        }
    }
}
