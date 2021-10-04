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
using NSprakIDE.Controls.General;
using NSprak.Operations;
using NSprak.Language;

namespace NSprakIDE.Controls
{
    public class MemoryViewContext
    {
        public Executor Executor { get; }

        public FunctionSignature Signature { get; private set; }

        public int Location { get; private set; }

        public bool HasLocation => Location != -1;

        public event EventHandler<EventArgs> Changed;

        public MemoryViewContext(Executor executor)
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

    public class LocalWrapper
    {
        public string Name { get; set; }

        public string Value { get; set; }

        public string SprakType { get; set; }

        public LocalWrapper(string name, Value value)
        {
            Name = name;
            Value = value?.ToString();
            SprakType = value?.Type?.InternalName;
        }
    }

    public class ValueWrapper
    {
        public string Value { get; set; }

        public string SprakType { get; set; }

        public ValueWrapper(Value value)
        {
            Value = value?.ToString();
            SprakType = value?.Type?.InternalName;
        }
    }

    public partial class MemoryView 
        : UserControl, IViewSupplierView<MemoryViewContext>
    {
        public ViewSupplier<MemoryViewContext> Supplier { get; }

        public MemoryViewContext SelectedContext
        {
            get
            {
                ViewItem<MemoryViewContext> contextItem
                    = (ViewItem<MemoryViewContext>)ViewSelect.SelectedItem;

                return contextItem?.Value;
            }
        } 

        public MemoryView()
        {
            InitializeComponent();

            Supplier = new ViewSupplier<MemoryViewContext>(ViewSelect);

            FrameList.SelectionChanged += FrameList_SelectionChanged;
        }

        public void Clear()
        {
            FrameList.Items.Clear();
            CheckFrameSelection();

            LocalsList.ItemsSource = null;
            LocalsList.Items.Clear();

            ValuesList.ItemsSource = null;
            ValuesList.Items.Clear();
        }

        public void Update()
        {
            Clear();
            MemoryViewContext context = SelectedContext;

            if (context == null)
            {
                ValuesList.Visibility = Visibility.Collapsed;
                return;
            }

            Executor executor = context.Executor;
            if (executor.State != ExecutorState.Paused)
                return;

            // Call Stack

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

                FrameList.Items.Add(new CallStackItem(signature, location));
            }

            CheckFrameSelection();

            // Values Grid

            if (executor.StepMode == ExecutorStepMode.Operation)
            {
                ValuesSection.Visibility = Visibility.Visible;

                List <ValueWrapper> stack;

                stack = executor
                    .Memory
                    .Values
                    .Select(x => new ValueWrapper(x))
                    .Reverse()
                    .ToList();

                ValuesList.ItemsSource = stack;
            }
            else
            {
                ValuesSection.Visibility = Visibility.Collapsed;
            }

            List<LocalWrapper> locals;

            locals = executor
                .Memory
                .CurrentScope
                .ListVariables()
                .Select(x => new LocalWrapper(x.Key, x.Value))
                .ToList();

            LocalsList.ItemsSource = locals;
        }

        private void ViewSelect_Selected(object sender, ValueSelectedEventArgs e)
        {
            Update();
        }

        private void FrameList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CheckFrameSelection();
        }

        private void CheckFrameSelection()
        {
            MemoryViewContext context = SelectedContext;

            if (context == null)
                return;

            CallStackItem item = (CallStackItem)FrameList.SelectedItem;

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

            context.Update(location, signature);
        }
    }
}
