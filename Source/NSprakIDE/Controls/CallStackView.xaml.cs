using NSprak.Execution;
using NSprak.Functions.Signatures;
using NSprak.Operations;
using NSprakIDE.Themes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NSprakIDE.Controls
{
    public class CallStackItem
    {
        public string Name { get; set; }

        public string Namespace { get; set; }

        public string Params { get; set; }

        public CallStackItem(FunctionSignature signature)
        {
            Name = signature.Name;
            Namespace = signature.Namespace;
            Params = $"({signature.TypeSignature})";
        }
    }

    public partial class CallStackView : UserControl
    {
        private Executor _executor;

        public Executor Target
        {
            get => _executor;
            set => _executor = value;
        }

        public CallStackView()
        {
            InitializeComponent();
        }

        public void Update()
        {
            FunctionSignature main = new FunctionSignature(
                "", "Main", new FunctionTypeSignature());

            List<FunctionSignature> signatures = new List<FunctionSignature>();
            List<int> locations = new List<int>();

            locations.Add(_executor.Instructions.Index);

            signatures.AddRange(_executor.Memory.FrameDebugInfo);
            locations.AddRange(_executor.Memory.Frames);

            signatures.Add(main);

            ListView.Items.Clear();
            for (int i = 0; i < signatures.Count; i++)
            {
                FunctionSignature signature = signatures[i];
                int location = locations[i];

                OpDebugInfo info = _executor.Executable.DebugInfo[location];

                ListView.Items.Add(new CallStackItem(signature));
            }
        }
    }
}
