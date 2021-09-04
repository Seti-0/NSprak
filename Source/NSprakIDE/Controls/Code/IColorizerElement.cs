using System;
using System.Collections.Generic;
using System.Text;

using ICSharpCode.AvalonEdit.Rendering;

using NSprak.Tokens;

namespace NSprakIDE.Controls.Code
{
    public interface IColorizerElement<T>
    {
        bool CanApply(T item);

        void Apply(VisualLineElement element, T item);
    }
}
