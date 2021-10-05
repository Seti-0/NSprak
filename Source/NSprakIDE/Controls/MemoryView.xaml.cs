using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using System.Windows;
using System.Windows.Controls;

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

        public FrameDebugInfo Frame { get; private set; }

        public event EventHandler<EventArgs> Changed;

        public MemoryViewContext(Executor executor)
        {
            Executor = executor;
        }

        public void Update(FrameDebugInfo info)
        {
            Frame = info;

            // Ideally this would only be triggered if the value of the frame
            // has changed, but it's a minor detail and requires value equality
            // for FrameDebugInfo, which is tricky since it depends on the 
            // InstructionEnumerator.
            OnChanged(EventArgs.Empty);
        }

        protected virtual void OnChanged(EventArgs e)
        {
            Changed?.Invoke(this, e);
        }
    }

    public class CallStackItem
    {
        public FrameDebugInfo Info { get; }

        public int Location { get; set; }

        public string Name { get; set; }

        public string Namespace { get; set; }

        public string Params { get; set; }

        public CallStackItem(FrameDebugInfo info)
        {
            Info = info;

            Location = info.Location;
            Name = info.FunctionSignature.Name;
            Namespace = info.FunctionSignature.Namespace;
            Params = $"({info.FunctionSignature.TypeSignature})";
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
            FrameList.ItemsSource = null;
            FrameList.Items.Clear();
            
            CheckFrameSelection(SelectedContext);
            ClearVariablesView();

            ValuesList.ItemsSource = null;
            ValuesList.Items.Clear();
        }

        private void ClearVariablesView()
        {
            LocalsList.ItemsSource = null;
            LocalsList.Items.Clear();
        }

        public void Update()
        {
            Clear();
            MemoryViewContext context = SelectedContext;

            if (context == null)
            {
                ValuesSection.Visibility = Visibility.Collapsed;
                return;
            }

            Executor executor = context.Executor;
            if (executor.State != ExecutorState.Paused)
                return;

            // Call Stack

            FrameList.ItemsSource = executor
                .Memory
                .FrameDebugInfo
                .Select(x => new CallStackItem(x))
                .ToList();

            // This will also update the variables list.
            CheckFrameSelection(context);

            // Value Stack (Visible for operations view only)

            if (executor.StepMode == ExecutorStepMode.Operation)
            {
                ValuesSection.Visibility = Visibility.Visible;

                List<ValueWrapper> stack;

                stack = executor
                    .Memory
                    .Values
                    .Select(x => new ValueWrapper(x))
                    .Reverse()
                    .ToList();

                ValuesList.ItemsSource = stack;
            }
            else ValuesSection.Visibility = Visibility.Collapsed;
        }

        private void UpdateVariablesView(MemoryViewContext context)
        {
            ClearVariablesView();

            if (context == null || context.Executor.State != ExecutorState.Paused)
                return;

            List<LocalWrapper> locals;

            locals = context
                .Frame
                .Scope
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
            CheckFrameSelection(SelectedContext);
        }

        private void CheckFrameSelection(MemoryViewContext context)
        {
            if (context == null)
                return;

            CallStackItem item = (CallStackItem)FrameList.SelectedItem;

            if (item == null)
                context.Update(context.Executor.Memory.FrameDebugInfo.Peek());
            
            else 
            {
                FrameDebugInfo info = item.Info;
                context.Update(info);
            }

            UpdateVariablesView(context);
        }
    }
}
