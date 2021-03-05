using AvalonDock.Layout;
using AvalonDock.Layout.Serialization;

using System;
using System.Windows;

using NSprakIDE.Controls;
using NSprakIDE.Controls.Execution;
using NSprakIDE.Controls.Files;
using NSprakIDE.Controls.Output;
using NSprakIDE.Docking;
using NSprakIDE.Logging;

namespace NSprakIDE
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static MainWindow Instance { get; private set; }

        public const string ComputerLogCategory = "Computers";

        private NSprakSaveManager _dockingHelper;
        private ComputerDocumentManager _documentHelper;

        private OutputView _output;
        private LocalsView _locals;
        private FileView _files;

        public MainWindow()
        {
            if (Instance != null)
                // I know, this "single-instance" thing is hardly ideal. I might come back to it.
                throw new InvalidOperationException("Only one instance of this class should be created");

            Instance = this;

            InitializeComponent();

            _output = new OutputView();
            _output.Name = "Output";

            OutputLog debug = _output.StartLog(OutputView.MainCategory, "Debug");
            ILogOutput output = new DirectColoredOutput(new OutputLogWriter(debug));
            output.Begin();
            Log.ReplayAll(output);
            Log.Outputs.Add(output);

            _locals = new LocalsView();
            _locals.Name = "Locals";

            _files = new FileView();
            _files.Name = "Files";

            InitDocking();

            _output.CreateCategory(ComputerLogCategory);

            _files.FileOpened += OnOpenFile;
        }

        private void OnOpenFile(object sender, FileOpenedEventArgs e)
        {
            OpenComputerEditor(e.Path);
        }

        public void OpenComputerEditor(string filePath)
        {
            ComputerEditorEnviroment enviroment = new ComputerEditorEnviroment
            {
                OutputView = _output,
                FilePath = filePath,
                LocalsView = _locals,
            };

            _documentHelper.OpenComputerEditor(enviroment);
        }

        private void InitDocking()
        {
            _dockingHelper = new NSprakSaveManager(DockingManager);
            _dockingHelper.Init();

            _documentHelper = new ComputerDocumentManager(_dockingHelper);

            _dockingHelper.AddConsoleAnchorable("Output", _output);
            _dockingHelper.AddDebugAnchorable("Locals", _locals);
            _dockingHelper.AddDocument("Files", _files);

            _dockingHelper.SaveCurrentLayout(overwrite: false);
            _dockingHelper.SwitchToLayout(LayoutNames.Main);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _dockingHelper?.SaveCurrentLayout();
        }
    }
}
