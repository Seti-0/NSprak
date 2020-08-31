using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using NSprak.Execution;
using NSprak.Language;
using NSprak.Language.Values;

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

    /// <summary>
    /// Interaction logic for LocalsView.xaml
    /// </summary>
    public partial class LocalsView : UserControl
    {
        public Executor Target { get; set; }

        public LocalsView()
        {
            InitializeComponent();
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
            if (Target == null)
            {
                Clear();
                return;
            }
            else
            {
                List<LocalWrapper> locals;
                List<ValueWrapper> stack;

                locals = Target
                    .Memory
                    .CurrentScope
                    .ListVariables()
                    .Select(x => new LocalWrapper(x.Key, x.Value))
                    .ToList();

                stack = Target
                    .Memory
                    .Values
                    .Select(x => new ValueWrapper(x))
                    .Reverse()
                    .ToList();

                LocalsGrid.ItemsSource = locals;
                ValuesGrid.ItemsSource = stack;
            }
        }
    }
}
