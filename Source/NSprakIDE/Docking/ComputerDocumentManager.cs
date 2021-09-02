using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

using Microsoft.Extensions.Logging;

using AvalonDock.Layout;

using NSprakIDE.Controls;

namespace NSprakIDE.Docking
{
    public class ComputerDocumentManager
    {
        private NSprakSaveManager _dockingHelper;
        private Dictionary<string, LayoutDocument> _openDocuments = new Dictionary<string, LayoutDocument>();

        public ComputerDocumentManager(NSprakSaveManager dockingManager)
        {
            _dockingHelper = dockingManager;
        }

        public void OpenComputerEditor(ComputerEditorEnviroment enviroment)
        {
            string id = StringHelper.Simplify("Computer - " + enviroment.FilePath);

            LayoutDocument document;

            if (_openDocuments.TryGetValue(id, out document))
            {
                document.IsActive = true;
                return;
            }

            ComputerEditor editor = new ComputerEditor(enviroment);
            editor.Name = id;

            string title = System.IO.Path.GetFileNameWithoutExtension(enviroment.FilePath);
            document = _dockingHelper.AddDocument(title, editor);
            document.IsActive = true;

            _openDocuments.Add(id, document);
            document.Closing += CloseHandler(id, editor);
            document.IsSelectedChanged += (sender, e) =>
            {
                if (document.IsSelected)
                    Logs.Core.LogInformation($"Hello world from {title}!");
            };
        }

        private EventHandler<CancelEventArgs> CloseHandler(string name, ComputerEditor editor)
        {
            void Action(object sender, CancelEventArgs e)
            {
                // I don't know why, but exceptions thrown in this callback
                // are not caught in any of the global exception handlers.
                // Hence the try-catch here.

                try
                {
                    _openDocuments.Remove(name);
                    editor.Dispose();
                }
                catch (Exception exception)
                {
                    string msg = "Unexpected error while closing window.";
                    Logs.Core.LogError(exception, msg);
                }
            }

            return Action;
        }
    }
}
