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

        protected void Invalidate()
        {
            Screen.InvalidateVisual();
        }

        public abstract void Render(DrawingContext context, Rect targetRect);

        public virtual void OnKeyUp(KeyEventArgs e) { }

        public virtual void OnTextInput(TextCompositionEventArgs e) { }
    }
}
