using AvalonDock.Layout;
using NSprakIDE.Controls;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

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
            document.Closing += CloseHandler(id);
        }

        private EventHandler<CancelEventArgs> CloseHandler(string name)
        {
            void Action(object sender, CancelEventArgs e)
            {
                _openDocuments.Remove(name);
            }

            return Action;
        }
    }
}
