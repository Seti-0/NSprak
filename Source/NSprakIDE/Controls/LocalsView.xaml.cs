using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using NSprak.Execution;
using NSprak.Language;
using NSprak.Language.Values;
using NSprakIDE.Controls.General;

namespace NSprakIDE.Controls
{
    public class LocalWrapper
    {
        public string Name { get; }

        public string Value { get; }

        public string SprakType { get; }

        public LocalWrapper(string name, Value value)
        {
            Name = name;
            Value = value?.ToString();
            SprakType = value?.Type?.InternalName;
        }
    }

    public class ValueWrapper
    {
        public string Value { get; }

        public string SprakType { get; }

        public ValueWrapper(Value value)
        {
            Value = value?.ToString();
            SprakType = value?.Type?.InternalName;
        }
    }

    public partial class LocalsView : UserControl, IViewSupplierView<Executor>
    {
        public ViewSupplier<Executor> Supplier { get; }

        public LocalsView()
        {
            InitializeComponent();
            Supplier = new ViewSupplier<Executor>(ViewSelect)
            {
                AllowNoSelection = false
            };
        }

        public void Clear()
        {
            LocalsGrid.ItemsSource = null;
            ValuesGrid.ItemsSource = null;

            LocalsGrid.Items.Clear();
            ValuesGrid.Items.Clear();
        }

        public void Update()
        {
            ViewItem<Executor> item = (ViewItem<Executor>)ViewSelect
                .SelectedItem;

            if (item == null)
            {
                ValuesGrid.Visibility = Visibility.Collapsed;
                Clear();
                return;
            }
            else
            {
                Executor executor = item.Value;

                bool opMode = executor.StepMode == ExecutorStepMode.Operation;
                ValuesGrid.Visibility = opMode ? 
                    Visibility.Visible : Visibility.Collapsed;

                List<LocalWrapper> locals;
                List<ValueWrapper> stack;

                locals = executor
                    .Memory
                    .CurrentScope
                    .ListVariables()
                    .Select(x => new LocalWrapper(x.Key, x.Value))
                    .ToList();

                stack = executor
                    .Memory
                    .Values
                    .Select(x => new ValueWrapper(x))
                    .Reverse()
                    .ToList();

                LocalsGrid.ItemsSource = locals;
                ValuesGrid.ItemsSource = stack;
            }
        }

        private void ViewSelect_Selected(object sender, ValueSelectedEventArgs e)
        {
            Update();
        }
    }
}
