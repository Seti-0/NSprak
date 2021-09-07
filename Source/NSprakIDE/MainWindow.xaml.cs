using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

using Serilog;
using Serilog.Core;

using Microsoft.Extensions.Logging;

using NSprakIDE.Controls;
using NSprakIDE.Logging;
using NSprakIDE.Controls.Output;

namespace NSprakIDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

            OutputLog debug = OutputView.Supplier.Start("Debug");
            ILogEventSink output = new Output(new OutputLogWriter(debug));

            Serilog.ILogger logger = new LoggerConfiguration()
                .WriteTo.Sink(output)
                .MinimumLevel.Debug()
                .CreateLogger();

            Logs.Factory.AddSerilog(logger);

            OutputView.Supplier.StartCategory(ComputerLogCategory);

            FileView.FileOpened += OnOpenFile;
        }

        private void OnOpenFile(object sender, FileOpenedEventArgs e)
        {
            OpenComputerEditor(e.Path);
        }

        public void OpenComputerEditor(string filePath)
        {
            // Start at index 1, the tab at index 0 is the FileView!
            for (int i = 1; i < DocumentView.Items.Count; i++)
            {
                TabItem tab = (TabItem)DocumentView.Items[i];
                ComputerEditor previousEditor = (ComputerEditor)tab.Content;
                if (previousEditor.Enviroment.FilePath == filePath)
                {
                    // Don't open the same file twice, just switch to
                    // the already open tab.
                    DocumentView.SelectedIndex = i;
                    return;
                }
            }

            string name = System.IO.Path.GetFileNameWithoutExtension(filePath);

            ComputerEditorEnviroment enviroment = new ComputerEditorEnviroment
            {
                Output = OutputView.Supplier.Start(name, ComputerLogCategory),
                FilePath = filePath,
                LocalsView = LocalsView,
                MessageView = MessageView
            };

            ComputerEditor editor = new ComputerEditor(enviroment);

            TabItem newTab = new TabItem();
            newTab.Header = name;
            newTab.Content = editor;
            newTab.Style = (Style)FindResource("DocumentTabItem");

            editor.HasChangesChanged += (obj, e) =>
            {
                string header = name;
                if (editor.HasChanges)
                    header += "*";
                newTab.Header = header;
            };

            void OnTabSelected()
            {
                OutputView.Supplier.Select(enviroment.Output);
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
                    DocumentView.Items.RemoveAt(i);
                    editor.Dispose();
                    break;
                }
            }
        }
    }
}
