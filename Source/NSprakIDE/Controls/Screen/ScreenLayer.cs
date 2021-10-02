using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace NSprakIDE.Controls.Screen
{
    public abstract class ScreenLayer
    {
        public FixedSizeScreen Screen { get; set; }

        public event EventHandler<EventArgs> Invalidated;

        protected void Invalidate()
        {
            Screen.InvalidateVisual();
            OnInvalidated(EventArgs.Empty);
        }

        protected virtual void OnInvalidated(EventArgs e)
        {
            Invalidated?.Invoke(this, e);
        }

        public abstract void Render(DrawingContext context, Rect targetRect);

        public virtual void OnKeyDown(KeyEventArgs e) { }

        public virtual void OnKeyUp(KeyEventArgs e) { }

        public virtual void OnTextInput(TextCompositionEventArgs e) { }
    }
}
