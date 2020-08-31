using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

using System.Threading;
using System.Threading.Tasks;

namespace NSprakIDE.Controls.General
{
    public class DelayHelper
    {
        public event EventHandler Complete;

        public TimeSpan WaitTime { get; set; } = TimeSpan.FromSeconds(0.5);

        public bool Waiting { get; private set; }

        private DateTime _lastPokeTime = DateTime.MinValue;

        public void Poke()
        {
            _lastPokeTime = DateTime.Now;

            if (!Waiting)
                new Task(Wait).Start();
        }

        private void Wait()
        {
            Waiting = true;

            while (true)
            {
                DateTime currentTime = DateTime.Now;
                TimeSpan timeElapsed = currentTime - _lastPokeTime;

                if (timeElapsed >= WaitTime)
                    break;

                TimeSpan timeToWait = WaitTime - timeElapsed;

                Thread.Sleep((int)timeToWait.TotalMilliseconds);
            }

            Complete?.Invoke(this, new EventArgs());
            Waiting = false;
        }
    }
}
