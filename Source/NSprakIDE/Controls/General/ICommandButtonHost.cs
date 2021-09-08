using System;
using System.Collections.Generic;
using System.Text;

namespace NSprakIDE.Controls.General
{
    public interface ICommandButtonHost
    {
        public event EventHandler<EventArgs> CommandContextChanged;
    }
}
