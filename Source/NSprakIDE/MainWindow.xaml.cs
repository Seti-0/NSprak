using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Linq;

using NSprakIDE.Controls;
using NSprakIDE.Logging;
using NSprakIDE.Controls.Output;
using NSprakIDE.Controls.General;

namespace NSprakIDE
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public const string ComputerLogCategory = "Computers";

        public MainWindow()
        {
            if (Instance != null)
                // I know, this "single-instance" thing is hardly ideal. I might come back to it.
                throw new InvalidOperationException("Only one instance of this class should be created");

            Instance = this;

            InitializeComponent();

            OutputLog debug = LogView.Supplier.Start(
                "MainWindow_Output", "Debug", ViewSupplier.Category_Main);

            ILogEventSink output = new Output(new OutputLogWriter(debug));
            Logs.AddSink(output);

            ILogEventSink status = new StatusOutput(StatusView, this);
            Logs.AddSink(status);

            FileView.FileOpened += OnOpenFile;

            SetupViewHiding(ScreenView, ScreenTab, OutputTabs);
            SetupViewHiding(MemoryView, MemoryTab, InfoTabs);

            foreach (FileOpenedEventArgs item in FileView.EnumerateOpenedFiles())
                OnOpenFile(FileView, item);
        }

        public void ShowLogView()
        {
            OutputTabs.SelectedItem = LogTab;
        }

        private void OnOpenFile(object sender, FileOpenedEventArgs e)
        {
            OpenComputerEditor(e.SourcePath, e.TempPath);
        }

        private void OpenComputerEditor(string filePath, string tempPath)
        {
            // Start at index 1, the tab at index 0 is the FileView!
            for (int i = 1; i < DocumentView.Items.Count; i++)
            {
                TabItem tab = (TabItem)DocumentView.Items[i];
                ComputerEditor previousEditor = (ComputerEditor)tab.Content;
                if (previousEditor.Environment.FilePath == filePath)
                {
                    // Don't open the same file twice, just switch to
                    // the already open tab.
                    DocumentView.SelectedIndex = i;
                    return;
                }
            }

            string id = "ComputerEditor -- " + Path
                .GetRelativePath(Environment.CurrentDirectory, filePath);

            string name = Path.GetFileNameWithoutExtension(filePath);

            // The need for all this could be removed by passing the computer
            // parameter to the aux. components, rather than the other way
            // round.
            ComputerEditorEnviroment enviroment = new ComputerEditorEnviroment
            {
                Name = name,
                GivenID = id,
                FilePath = filePath,
                TempPath = tempPath,
                MessageView = MessageView,
                ScreenView = ScreenView,
                MemoryView = MemoryView
            };

            ComputerEditor editor = new ComputerEditor(enviroment);

            TabItem newTab = new TabItem
            {
                Header = name,
                Content = editor,
                Style = (Style)FindResource("DocumentTabItem")
            };

            void UpdateHeader()
            {
                string header = name;
                if (editor.HasChanges)
                    header += "*";
                newTab.Header = header;
            }

            UpdateHeader();
            editor.HasChangesChanged += (obj, e) => UpdateHeader();

            void OnTabSelected()
            {
                MessageView.Supplier.Select(id);
                ScreenView.Supplier.Select(id);
                MemoryView.Supplier.Select(id);

                OutputTabs.SelectedItem = ScreenTab;
            }
            newTab.MouseUp += (s, e) => OnTabSelected();
            
            int index = DocumentView.Items.Add(newTab);
            DocumentView.SelectedIndex = index;
            OnTabSelected();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // I don't know of a way of extracting the button's parent
            // tab item directly, silly as that seems. (Is it possible to
            // do so when a control template is the source of the event?)
            for (int i = 0; i < DocumentView.Items.Count; i++)
            {
                TabItem item = (TabItem)DocumentView.Items[i];
                if (item.IsMouseOver)
                {
                    ComputerEditor editor = (ComputerEditor)item.Content;
                    
                    if (editor.HasChanges)
                    {
                        string title = "Confirmation";
                        string message = "Do you want to save this file?";
                        DialogType type = DialogType.YesNoCancel;

                        Dialog dialog = new Dialog(title, message, type);
                        dialog.ShowDialog();
                        DialogResponse response = dialog.Response;

                        if (response == DialogResponse.Cancel)
                            return;

                        if (response == DialogResponse.Yes)
                            editor.Save();
                    }

                    DocumentView.Items.RemoveAt(i);
                    editor.Dispose();
                    break;
                }
            }
        }

        private void SetupViewHiding<T>(
            IViewSupplierView<T> view, 
            TabItem tab, TabControl tabs)
        {
            bool available = false;

            tabs.Items.Remove(tab);

            view.Supplier.ItemsChanged += (obj, e) =>
            {
                bool newValue = view.Supplier.Values.Any();
                if (newValue != available)
                {
                    available = newValue;
                    if (available)
                    {
                        tabs.Items.Add(tab);
                        tabs.SelectedItem = tab;
                    }
                    else
                        tabs.Items.Remove(tab);
                }
            };
        }
    }
}
