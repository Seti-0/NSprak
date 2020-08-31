using System;
using System.Collections.Generic;
using System.Text;

using ICSharpCode.AvalonEdit.Rendering;

using NSprak.Tokens;

namespace NSprakIDE.Controls.Code
{
    public interface IColorizerElement<T>
    {
        public void Apply(VisualLineElement element, T item);
    }
}
