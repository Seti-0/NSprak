using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace NSprakIDE.Controls.Files
{
    using PathHelper = Path;

    public class FileTreeItem : INotifyPropertyChanged
    {
        public string Name { get; }

        public string NewName { get; set; }

        public FileTreeItem Parent { get; }

        public bool IsFile { get; }

        public bool IsExpanded { get; set; }

        public bool IsSelected { get; set; }

        public bool IsEditing
        {
            get => _editing;

            set
            {
                if (_editing != value)
                {
                    _editing = value;
                    OnPropertyChanged(new PropertyChangedEventArgs(nameof(IsEditing)));
                }
            }
        }

        public bool IsEditable { get; }

        public string Context { get; }

        private Dictionary<string, FileTreeItem> _children;
        private bool _editing;

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Unknown
        {
            get
            {
                bool foundFile = IsFile && File.Exists(Path);
                bool foundFolder = (!IsFile) && Directory.Exists(Path);

                return !(foundFile || foundFolder);
            }
        }

        public IEnumerable<FileTreeItem> Children
        {
            get
            {
                if (_children == null)
                    UpdateChildren();

                return _children?.Values;
            }

            set => throw new NotImplementedException();
        }

        public string Path => FindPath();

        public FileTreeItem(string name, FileTreeItem parent, bool isFile)
        {
            Name = name;
            NewName = name;
            Parent = parent;
            IsFile = isFile;

            if (parent == null)
                Context = FileContexts.Root;

            else if (IsFile)
                Context = FileContexts.Computer;

            else
                Context = FileContexts.Folder;

            IsEditable = parent != null;
        }

        public override string ToString()
        {
            return Path;
        }

        private string FindPath()
        {
            if (Parent == null)
                return Name;

            else return PathHelper.Combine(Parent.Path, Name);
        }

        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public void Refresh()
        {
            Update();
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Children)));
        }

        private void Update()
        {
            UpdateChildren();

            if (_children == null) return;

            foreach (FileTreeItem item in _children.Values)
                item.Update();
        }

        private void UpdateChildren()
        {
            if (Unknown || IsFile)
            {
                _children = null;
                return;
            }

            _children ??= new Dictionary<string, FileTreeItem>();

            IEnumerable<string> toRemove = _children
                .Values
                .Where(x => x.Unknown)
                .Select(x => x.Name);

            foreach (string key in toRemove)
                _children.Remove(key);

            foreach (string path in Directory.EnumerateDirectories(Path))
            {
                string name = PathHelper.GetFileName(path);

                if (!_children.ContainsKey(name))
                    _children.Add(name, new FileTreeItem(name, this, isFile: false));
            }

            foreach (string path in Directory.EnumerateFiles(Path))
            {
                string name = PathHelper.GetFileName(path);

                if (!_children.ContainsKey(name))
                    _children.Add(name, new FileTreeItem(name, this, isFile: true));
            }
        }

        public FileTreeItem CloneDeep(FileTreeItem newParent = null)
        {
            FileTreeItem result = new FileTreeItem(Name, newParent, IsFile)
            {
                IsExpanded = IsExpanded,
                IsSelected = IsSelected,
            };
           
            if (Children != null)
            {
                result._children = Children
                   .Select(x => x.CloneDeep(this))
                   .ToDictionary(x => x.Name);
            }

            return result;
        }
    }
}
