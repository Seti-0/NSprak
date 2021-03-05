using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using NSprakIDE.Commands;
using NSprakIDE.Logging;

using NSprakIDE.Controls.Files;

namespace NSprakIDE.Controls
{
    using static CommandHelper;

    public class FileOpenedEventArgs
    {
        public string Path { get; }

        public FileOpenedEventArgs(string path)
        {
            Path = path;
        }
    }

    /// <summary>
    /// Interaction logic for FileView.xaml
    /// </summary>
    public partial class FileView : UserControl
    {
        public const string SaveDir = "Computers";

        public FileTreeItem Root { 
            
            get => _root; 
            
            set
            {
                _root = value;
                Tree.ItemsSource = new List<FileTreeItem>() { _root };
            }
        }

        private FileTreeItem _editTarget;
        private FileTreeItem _root;

        //public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<FileOpenedEventArgs> FileOpened;

        public FileView()
        {
            // This is not great
            DataContext = this;

            InitializeComponent();

            Root = new FileTreeItem(SaveDir, parent: null, isFile: false);
            Root.IsExpanded = true;

            SetupBindings();
        }

        private void SetupBindings()
        {
            Bind(this, FileCommands.AddFile, AddFile);
            Bind(this, FileCommands.AddFolder, AddFolder);
            Bind(this, FileCommands.OpenInFileExplorer, OpenInFileExplorer);

            Bind(this, GeneralCommands.Rename, StartEditingSelected);
            Bind(this, ApplicationCommands.Delete, Delete);

            Bind(this, FileCommands.OpenSelected, OpenFile);
            Bind(this, GeneralCommands.RefreshView, Refresh);
        }

        protected void OnFileOpened(FileOpenedEventArgs e)
        {
            FileOpened?.Invoke(this, e);
        }

        private void StartEditingSelected()
        {
            if (_editTarget != null)
                _editTarget.IsEditing = false;

            FileTreeItem item = (FileTreeItem)Tree.SelectedItem;

            if (item != null && item.IsEditable)
            {
                _editTarget = item;
                item.IsEditing = true;
            }
        }

        private void StopEditing()
        {
            if (_editTarget != null)
            {
                _editTarget.IsEditing = false;

                void Rename()
                {
                    FileHelper.Rename(_editTarget.Path, _editTarget.NewName);
                }

                string error = $"Error occured while renaming {_editTarget.Path} to {_editTarget.NewName}";
                PerformFileOp(Rename, error);

                _editTarget = null;
            }
        }

        public void Refresh()
        {
            Root.Refresh();

            // Well, the items system seems quite broken as of writing this.
            // After trying lots of different things, Items.Refresh works for additions (but not deletions!)

            // Note: this would reset the expansion state of the tree without the two-way binding
            // between expansion in xaml and "IsExpanded" in the FileTreeIte

            Root = Root.CloneDeep();
            Tree.Items.Refresh();
        }

        public void OpenFile()
        {
            void Action(string path)
            {
                OnFileOpened(new FileOpenedEventArgs(path));
            };

            PerformFileOp(Action, "open file");
        }

        public void AddFile()
        {
            PerformFileOp(FileHelper.AddComputer, "new file");
        }

        public void AddFolder()
        {
            PerformFileOp(FileHelper.AddNewFolder, "new folder");
        }

        public void Delete()
        {
            PerformFileOp(FileHelper.Delete, "delete");
        }

        public void OpenInFileExplorer()
        {
            PerformFileOp(FileHelper.OpenInFileExplorer, "open in file explorer");
        }

        private void PerformFileOp(Action<string> pathAction, string errorName)
        {
            FileTreeItem item = (FileTreeItem)Tree.SelectedItem;

            item.Refresh();

            if (item.Unknown)
            {
                Log.Core.Warning($"Attempted an action a location that does not exist: {item.Path}");
                return;
            }

            string relativePath = item.Path;

            string saveDirPath = System.IO.Path.GetFullPath(SaveDir);
            string fullPath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(saveDirPath), relativePath);

            void Action()
            {
                pathAction(fullPath);
            }

            PerformFileOp(Action, $"Error performing action \"{errorName}\" at path \"{fullPath}\"");
        }

        private void PerformFileOp(Action action, string errorMessage)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Log.Core.Error(errorMessage, e);
            }

            Refresh();
        }

        private void Tree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            string context = FileContexts.Root;

            if (Tree.SelectedItem != null)
                context = ((FileTreeItem)Tree.SelectedItem).Context;

            Tree.ContextMenu = Tree.TryFindResource(context) as ContextMenu;
        }

        // A workaround to get a right click to change the selected node, from SO

        private static TreeViewItem Search(DependencyObject obj)
        {
            while (obj != null)
            {
                if (obj is TreeViewItem item)
                    return item;

                obj = VisualTreeHelper.GetParent(obj);
            }

            return null;
        }

        private void Tree_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = Search(e.OriginalSource as DependencyObject);

            if (item != null)
            {
                item.Focus();
                item.IsSelected = true;
                e.Handled = true;
            }
        }

        private void FilenameEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            StopEditing();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Refresh();
        }
    }
}
