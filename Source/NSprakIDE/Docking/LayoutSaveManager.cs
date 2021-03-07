using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Controls;
using System.CodeDom;
using System.Reflection.Metadata;
using System.Diagnostics;
using System.IO;

using AvalonDock;
using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

using Microsoft.Extensions.Logging;

namespace NSprakIDE.Docking
{
    public abstract class LayoutSaveManager
    {
        private string _folderPath;
        private DockingManager _manager;

        private const string DummyName = "Dummy";

        public string Current { get; private set; }

        public LayoutSaveManager(DockingManager manager, string saveFolderPath = "Layouts")
        {
            _folderPath = saveFolderPath;
            _manager = manager;
        }

        public void Init(string lastName = null)
        {
            SetupDefaults();

            if (lastName != null && File.Exists(GetPathForName(lastName)))
                SwitchToLayout(lastName);
        }

        public abstract void SetupDefaults();

        public void SwitchToLayout(string name)
        {
            LoadLayoutFromName(name);
        }

        public void SaveDummyLayout()
        {
            SaveCurrentLayout(DummyName);
        }

        public void SaveCurrentLayout(string withName = null, bool overwrite = true)
        {
            withName ??= Current;

            Directory.CreateDirectory(_folderPath);

            string path = GetPathForName(withName);

            if (File.Exists(path) && !overwrite)
                return;

            try
            {
                XmlLayoutSerializer serializer = new XmlLayoutSerializer(_manager);
                serializer.Serialize(path);
            }
            catch(Exception e)
            {
                Logs.Core.LogError(e, "Unable to save layout {Name} to path \"{Path}\"", withName, path);
            }
        }

        protected void LoadLayout(string name, LayoutPanel layout)
        {
            Current = name;
            _manager.Layout ??= new LayoutRoot();
            _manager.Layout.RootPanel = layout;
        }

        private void LoadLayoutFromName(string name)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));

            string path = GetPathForName(name);

            if (!File.Exists(path))
            {
                Logs.Core.LogError("Unable to load layout: {Name}", name);
                Logs.Core.LogDebug("Unable to find file: {Path}", path);
            }

            Current = name;

            LoadDummyLayout();
            LoadLayoutFromPath(path);
        }

        private void LoadDummyLayout()
        {
            string path = GetPathForName(DummyName);

            if (!File.Exists(path))
            {
                Logs.Core.LogError("Unable to load dummy layout");
                Logs.Core.LogDebug("Unable to find file: {Path}", path);
                return;
            }

            LoadLayoutFromPath(path);
        }

        private void LoadLayoutFromPath(string path)
        {
            try
            {
                XmlLayoutSerializer serializer = new XmlLayoutSerializer(_manager);
                serializer.Deserialize(path);
            }
            catch (Exception e)
            {
                Logs.Core.LogError(e, "Error while loading layout: " + path);
            }
        }

        public IEnumerable<string> GetLayoutNames()
        {
            if (Directory.Exists(_folderPath))
                return Directory
                    .EnumerateFiles(_folderPath, "*.layout")
                    .Select(GetNameForPath)
                    .Where(x => !string.IsNullOrWhiteSpace(x));

            return new string[0];
        }

        private string GetPathForName(string name)
        {
            return Path.Combine(_folderPath, $"{name}.layout");
        }

        private string GetNameForPath(string path)
        {
            return Path.GetFileNameWithoutExtension(path);
        }

        protected bool TryFindAnchorable(string name, out LayoutAnchorablePane result)
        {
            LayoutPanel root = _manager?.Layout?.RootPanel;
            if (root == null)
            {
                result = null;
                return false;
            }

            return TryFindAnchorable(name, root, out result);
        }

        private bool TryFindAnchorable(string name, LayoutPanel panel, out LayoutAnchorablePane result)
        {
            foreach (ILayoutPanelElement element in panel.Children)
            {
                if (element is LayoutAnchorablePane pane && pane.Name == name)
                {
                    result = pane;
                    return true;
                }

                else if (element is LayoutPanel subPanel)
                {
                    if (TryFindAnchorable(name, subPanel, out result))
                        return true;
                }
            }

            result = null;
            return false;
        }

        protected bool TryFindDocumentPane(out LayoutDocumentPane result)
        {
            LayoutPanel root = _manager?.Layout?.RootPanel;
            if (root == null)
            {
                result = null;
                return false;
            }

            return TryFindDocumentPane(root, out result);
        }

        private bool TryFindDocumentPane(LayoutPanel panel, out LayoutDocumentPane result)
        {
            foreach (ILayoutPanelElement element in panel.Children)
            {
                if (element is LayoutDocumentPane pane)
                {
                    result = pane;
                    return true;
                }

                else if (element is LayoutPanel subPanel)
                {
                    if (TryFindDocumentPane(subPanel, out result))
                        return true;
                }
            }

            result = null;
            return false;
        }
    }
}
