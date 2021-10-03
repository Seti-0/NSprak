using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using NSprak.Execution;
using NSprak.Functions.Signatures;
using NSprak.Operations;
using NSprakIDE.Themes;
using NSprakIDE.Controls.General;

namespace NSprakIDE.Controls
{
    public class CallStackContext
    {
        public Executor Executor { get; }

        public FunctionSignature Signature { get; private set; }

        public int Location { get; private set; }

        public bool HasLocation => Location != -1;

        public event EventHandler<EventArgs> Changed;

        public CallStackContext(Executor executor)
        {
            Executor = executor;
            Signature = null;
            Location = -1;
        }

        public void Update(int location, FunctionSignature signature)
        {
            if (location == Location && signature == Signature)
                return;

            Location = location;
            Signature = signature;

            OnChanged(EventArgs.Empty);
        }

        protected virtual void OnChanged(EventArgs e)
        {
            Changed?.Invoke(this, e);
        }
    }

    public class CallStackItem
    {
        public FunctionSignature Signature { get; set; }

        public int Location { get; set; }

        public string Name { get; set; }

        public string Namespace { get; set; }

        public string Params { get; set; }

        public CallStackItem(FunctionSignature signature, int location)
        {
            Signature = signature;
            Location = location;

            Name = signature.Name;
            Namespace = signature.Namespace;
            Params = $"({signature.TypeSignature})";
        }
    }

    public partial class CallStackView :
        UserControl, IViewSupplierView<CallStackContext>
    {
        public ViewSupplier<CallStackContext> Supplier { get; } 

        public CallStackContext CurrentContext
        {
            get
            {
                ViewItem<CallStackContext> viewItem = 
                    (ViewItem<CallStackContext>)ViewSelect.SelectedItem;

                return viewItem?.Value;
            }
        }

        public FunctionSignature SelectedFrame { get; private set; }

        public CallStackView()
        {
            InitializeComponent();

            Supplier = new ViewSupplier<CallStackContext>(ViewSelect);

            ListView.SelectionChanged += ListView_SelectionChanged;
        }

        public void Clear()
        {
            ListView.Items.Clear();
            CheckListSelection();
        }

        public void Update()
        {
            Clear();

            CallStackContext context = CurrentContext;
            if (context == null)
                return;

            Executor executor = context.Executor;
            if (executor.State != ExecutorState.Paused)
                return;

            FunctionSignature main = new FunctionSignature(
                "", "Main", new FunctionTypeSignature());

            List<FrameDebugInfo> frames = new List<FrameDebugInfo>();
            List<FunctionSignature> signatures = new List<FunctionSignature>();
            List<int> locations = new List<int>();

            locations.Add(executor.Instructions.Index);

            frames.AddRange(executor.Memory.FrameDebugInfo);
            signatures.AddRange(frames.Select(x => x.FunctionSignature));
            locations.AddRange(executor.Memory.Frames);

            signatures.Add(main);

            for (int i = 0; i < signatures.Count; i++)
            {
                FunctionSignature signature = signatures[i];
                int location = locations[i];

                OpDebugInfo info = executor.Executable.DebugInfo[location];

                ListView.Items.Add(new CallStackItem(signature, location));
            }

            CheckListSelection();
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckListSelection();
        }

        private void ViewSelect_Selected(object sender, ValueSelectedEventArgs e)
        {
            Update();
        }

        private void CheckListSelection()
        {
            CallStackContext context = CurrentContext;

            if (context == null)
                return; ;

            CallStackItem item = (CallStackItem)ListView.SelectedItem;

            int location;
            FunctionSignature signature;

            if (item == null)
            {
                location = context.Executor.Instructions.Index;

                if (context.Executor.Memory.FrameDebugInfo.Count > 0)
                    signature = context.Executor.Memory.FrameDebugInfo.Peek().FunctionSignature;
                else
                    // This needs to be moved to NSprak
                    signature = new FunctionSignature("", "Main", new FunctionTypeSignature());
            }
            else
            {
                location = item.Location;
                signature = item.Signature;
            }

            CurrentContext?.Update(location, signature);
        }
    }
}
