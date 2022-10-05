using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using NSprakIDE.Commands;
using NSprakIDE.Controls.Files;
using NSprakIDE.Controls.General;

using FolderDialog = System.Windows.Forms.FolderBrowserDialog;
using FolderDialogResult = System.Windows.Forms.DialogResult;

namespace NSprakIDE.Controls
{
    using static CommandHelper;

    public class FileOpenedEventArgs
    {
        public string SourcePath { get; }

        public string TempPath { get; }

        public FileOpenedEventArgs(string sourcePath, string tempPath)
        {
            SourcePath = sourcePath;
            TempPath = tempPath;
        }
    }

    public partial class FileView : UserControl
    {
        private const string TempDir = "Editing";

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

            Refresh();
            SetupBindings();
        }

        public IEnumerable<FileOpenedEventArgs> EnumerateOpenedFiles()
        {
            string workspace = SaveData.Instance.OpenFolder;
            if (workspace == null)
                return Enumerable.Empty<FileOpenedEventArgs>();

            string saveRoot = Path.GetFullPath(workspace);
            string tempRoot = Path.GetFullPath(TempDir);

            List<FileOpenedEventArgs> events = new List<FileOpenedEventArgs>();
            
            if (Directory.Exists(tempRoot))
                AddOpenFiles(tempRoot, events);
            
            return events;

            void AddOpenFiles(string directory, List<FileOpenedEventArgs> target)
            {
                foreach (string tempPath in Directory.EnumerateFiles(directory))
                {
                    string relative = Path.GetRelativePath(tempRoot, tempPath);
                    string savePath = Path.Combine(saveRoot, relative);

                    target.Add(new FileOpenedEventArgs(savePath, tempPath));
                }

                foreach (string directoryPath in Directory.EnumerateDirectories(directory))
                    AddOpenFiles(directoryPath, target);
            }
        }

        private void SetupBindings()
        {
            Bind(this, FileCommands.OpenFolder, OpenFolder);

            bool workspaceOpen() => SaveData.Instance.OpenFolder != null;

            Bind(this, FileCommands.AddFile, AddFile, workspaceOpen);
            Bind(this, FileCommands.AddFolder, AddFolder, workspaceOpen);
            Bind(this, FileCommands.OpenInFileExplorer, OpenInFileExplorer, workspaceOpen);

            bool isEditing() => workspaceOpen() && _editTarget != null;

            Bind(this, GeneralCommands.Rename, StartEditingSelected, workspaceOpen);
            Bind(this, GeneralCommands.Escape, StopEditing, isEditing);

            Bind(this, ApplicationCommands.Delete, Delete, workspaceOpen);

            Bind(this, FileCommands.OpenSelected, OpenFile, workspaceOpen);
            Bind(this, GeneralCommands.RefreshView, Refresh, workspaceOpen);
        }

        protected void OnFileOpened(FileOpenedEventArgs e)
        {
            FileOpened?.Invoke(this, e);
        }

        private void StartEditingSelected()
        {
            if (_editTarget != null)
                return;

            FileTreeItem item = (FileTreeItem)Tree.SelectedItem;

            if (item != null && item.IsEditable)
            {
                _editTarget = item;
                item.IsEditing = true;
            }
        }

        private void StopEditing()
        {
            // This is a funny one.
            
            // But updating the tree causes loss of focus, which in turn
            // causes the tree to update and everything crashes down.
            // (See the 'Refresh' method to see why that nonsense happens)

            // The easiest solution I can see is just to lose focus from
            // the get go and trigger the update that way.
            // (See FilenameEditor_LostFocus)

            Tree.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
        }

        private void FilenameEditor_LostFocus(object sender, RoutedEventArgs e)
        {
            ApplyEdit();
        }

        private void ApplyEdit()
        {
            if (_editTarget != null)
            {
                if (_editTarget.NewName != _editTarget.Name)
                {
                    void Rename(string path)
                    {
                        FileHelper.Rename(_editTarget.Path, _editTarget.NewName);
                    }

                    string errorName = $"renaming to '{_editTarget.NewName}'";
                    string infoName = $"Renaming to '{_editTarget.NewName}'";
                    PerformFileOp(Rename, errorName, infoName, _editTarget);
                }
                else
                {
                    Refresh();
                }

                _editTarget = null;
            }
        }

        public void OpenFolder()
        {
            using FolderDialog dialog = new FolderDialog();

            dialog.SelectedPath = Directory.GetCurrentDirectory();
            if (SaveData.Instance.OpenFolder != null)
                dialog.SelectedPath = SaveData.Instance.OpenFolder;

            FolderDialogResult result = dialog.ShowDialog();
            if (result != FolderDialogResult.OK || string.IsNullOrWhiteSpace(dialog.SelectedPath))
                return;

            Logs.Core.LogInformation("Selected workspace: " + dialog.SelectedPath);
            if (!MainWindow.Instance.CloseAllDocuments())
            {
                string msg = "Workspace change cannot proceed unless all files being edited are closed.";
                Logs.Core.LogInformation(msg);
                return;
            }

            SaveData.Instance.OpenFolder = dialog.SelectedPath;
            SaveData.Save();
            Refresh();
        }

        public void Refresh()
        {
            if (Root == null || Root.Path != SaveData.Instance.OpenFolder)
            {
                string openFolder = SaveData.Instance.OpenFolder;
                if (openFolder == null)
                {
                    Root = null;
                    Tree.Visibility = Visibility.Hidden;
                    return;
                }

                Root = new FileTreeItem(openFolder, parent: null, isFile: false);
                Root.IsExpanded = true;
                Root.IsSelected = true;
                Tree.Visibility = Visibility.Visible;
            }

            Root.Refresh();

            // Well, the items system seems quite broken as of writing this.
            // After trying lots of different things, Items.Refresh works for additions (but not deletions!)

            // Note: this would reset the expansion state of the tree without the two-way binding
            // between expansion in xaml and "IsExpanded" in the FileTreeItem

            Root = Root.CloneDeep();
            //Tree.Items.Refresh();

            // It seems that the selection isn't focused by default after the 
            // tree is reconstructed, this is to fix that.

            Tree.Dispatcher.BeginInvoke(new Action(() =>
            {
                (Tree.ItemContainerGenerator
                    .ContainerFromItem(Tree.SelectedItem) as TreeViewItem)
                    ?.Focus();
            }));
        }

        public void OpenFile()
        {
            FileTreeItem item = (FileTreeItem)Tree.SelectedItem;

            if (item.IsEditing)
                StopEditing();

            else if (item.IsFile)
            {
                void Action(string savePath, string tempPath)
                {
                    OnFileOpened(new FileOpenedEventArgs(savePath, tempPath));
                };

                PerformFileOp(Action, "open file", "Opening file");
            }
        }

        public void AddFile()
        {
            // It would be nice if we could start editing the new file,
            // but I am not sure how to do that right now.
            CheckRoot();
            PerformFileOp(FileHelper.AddComputer, "new file", "Creating new file in");
        }

        public void AddFolder()
        {
            CheckRoot();
            PerformFileOp(FileHelper.AddNewFolder, "new folder", "Creating new folder in");
        }

        public void Delete()
        {
            string title = "Confirmation";
            string message = "Are you sure you wish to delete this file?";

            bool? result = new Dialog(title, message, DialogType.YesNo).ShowDialog();

            if (result.HasValue && result.Value)
                PerformFileOp(FileHelper.Delete, "delete", "Deleting");                
        }

        public void OpenInFileExplorer()
        {
            PerformFileOp(FileHelper.OpenInFileExplorer, "open", "Opening in file explorer");
        }

        private void CheckRoot()
        {
            string openFolder = SaveData.Instance.OpenFolder;
            if (openFolder != null)
            {
                string fullPath = Path.GetFullPath(openFolder);
                FileHelper.EnsureDirectory(fullPath);
            }
        }

        private void PerformFileOp(Action<string> pathAction,
            string errorName, string infoName, FileTreeItem item = null)
            => PerformFileOp((path, _) => pathAction(path), errorName, infoName, item);

        private void PerformFileOp(Action<string, string> pathAction, 
            string errorName, string infoName, FileTreeItem item = null)
        {
            if (item == null)
                item = (FileTreeItem)Tree.SelectedItem;

            item.Refresh();

            Logs.Core.LogInformation("(FileOp) " + infoName + ": " + item.Path);

            string workspaceFolder = SaveData.Instance.OpenFolder;
            if (workspaceFolder == null)
            {
                string msg = "Attempted to perform file op without there being an open folder.";
                Logs.Core.LogWarning(msg);
                return;
            }

            if (item.Unknown)
            {
                string msg = $"Attempted an action a location that does not exist: {item.Path}";
                Logs.Core.LogWarning(msg);
                return;
            }

            string relativePath = Path.GetRelativePath(workspaceFolder, item.Path);
            
            string saveDirPath = Path.GetFullPath(workspaceFolder);
            string fullPath = Path.Combine(saveDirPath, relativePath);

            string tempDirPath = Path.GetFullPath(TempDir);
            string tempPath = Path.Combine(tempDirPath, relativePath);

            try
            {
                pathAction(fullPath, tempPath);
            }
            catch (Exception e)
            {
                string msg = $"Error performing action \"{errorName}\" at path \"{fullPath}\"";
                Logs.Core.LogError(msg, e);
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

        private void Tree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem item = Search(e.OriginalSource as DependencyObject);

            if (item != null)
            {
                item.Focus();
                item.IsSelected = true;
                OpenFile();
                e.Handled = true;
            }
        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox target = (TextBox)sender;
            
            void Action()
            {
                Thread.Sleep(2000);
                target.Dispatcher.Invoke(() => target.SelectAll());
            }

            Task.Run(Action);
        }

        private void ContentPresenter_GotFocus(object sender, RoutedEventArgs e)
        {
            ContentPresenter parent = (ContentPresenter)sender;
            TextBox input = (TextBox)VisualTreeHelper.GetChild(parent, 0);
            input.Dispatcher.BeginInvoke(new Action(() => input.SelectAll()));
        }
    }
}
