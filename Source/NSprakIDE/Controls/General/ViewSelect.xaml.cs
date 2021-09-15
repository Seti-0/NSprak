using System;
using System.Collections.Generic;
using System.Linq;
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

namespace NSprakIDE.Controls.General
{
    public interface IViewItem
    {
        string Name { get; }
        bool Underline { get; }

        object GetValue();
    }

    public class ViewItem<T> : IViewItem
    {
        public string Name { get; set; }

        public string Category { get; set; }

        public bool Underline { get; set; }

        public T Value { get; set; }

        public ViewItem(string name, string category, bool underline, T value)
        {
            Name = name;
            Underline = underline;
            Value = value;
            Category = category;
        }

        public object GetValue() => Value;
    }

    public class ViewSupplier
    {
        public const string Category_Main = "Main";
        public const string ID_All = "All";
    }

    public class ViewSupplier<T> : ViewSupplier
    {
        private readonly Dictionary<string, ViewItem<T>> _items 
            = new Dictionary<string, ViewItem<T>>();

        private readonly ViewSelect _view;
        private bool _allowNoSelection;

        public bool AllowNoSelection
        {
            get => _allowNoSelection;

            set
            {
                _allowNoSelection = value;
                UpdateView();
            }
        }

        public IEnumerable<ViewItem<T>> ViewItems => _items.Values;

        public IEnumerable<T> Values => _items
            .Values
            .Select(x => x.Value)
            .Where(x => x != null);

        public ViewSelect View => _view;

        public ViewSupplier(ViewSelect view)
        {
            _view = view;
            _items.Add(ID_All, new ViewItem<T>("All", Category_Main, true, default)); ;
        }

        public void Start(T value, string id, string name, string category)
        {
            _items.Add(id, new ViewItem<T>(name, category, false, value));
            UpdateView();
        }

        public void Select(string id)
        {
            if (id == null)
                _view.Select(_items[ID_All]);
            else
                _view.Select(_items[id]);
        }

        public void End(string id)
        {
            _items.Remove(id);
            UpdateView();
        }

        private void UpdateView()
        {
            IEnumerable<ViewItem<T>> items = _items.Values;

            if (!AllowNoSelection)
                items = items.Skip(1);

            _view.UpdateOptions(items);
        }
    }

    public class DefaultViewSupplier<T> : ViewSupplier<T>
        where T : new()
    {
        public DefaultViewSupplier(ViewSelect view) : base(view) { }

        public T Start(string id, string name, string category)
        {
            T value = new T();
            Start(value, id, name, category);
            return value;
        }
    }

    public class ValueSelectedEventArgs : EventArgs
    {
        public object OldValue { get; }

        public object NewValue { get; }

        public ValueSelectedEventArgs(object oldValue, object newValue)
        {
            OldValue = oldValue;
            NewValue = newValue;
        }
    }

    // Is it possible to have a generic user control?
    //That would save many of the hoops here.
    public partial class ViewSelect : UserControl
    {
        public static DependencyProperty DescriptionProperty = DependencyProperty
            .Register(nameof(Description), typeof(string), typeof(ViewSelect));

        private IEnumerable<IViewItem> _items;
        private IViewItem _selectedItem;

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public IViewItem SelectedItem
        {
            get => (IViewItem)Selection.SelectedItem;
        }

        public event EventHandler<ValueSelectedEventArgs> Selected;

        public ViewSelect()
        {
            InitializeComponent();
            UpdateOptions(new IViewItem[0]);
        }

        public void UpdateOptions(IEnumerable<IViewItem> items)
        {
            _items = items;
            Selection.ItemsSource = items;

            // Check that the currently selected item is valid,
            // otherwise, leave it alone.
            if (_selectedItem == null || !_items.Contains(_selectedItem))
                Select(_items.FirstOrDefault());
        }

        public void Select(IViewItem newItem)
        {
            IViewItem oldItem = _selectedItem;

            if (oldItem != newItem)
            {
                _selectedItem = newItem;
                Selection.SelectedItem = newItem;
                OnSelectionChanged(oldItem, newItem);
            }
        }

        private void Selection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            IViewItem newItem = (IViewItem)Selection.SelectedItem;
            IViewItem oldItem = _selectedItem;

            if (oldItem != newItem)
            {
                _selectedItem = newItem;
                OnSelectionChanged(oldItem, newItem);
            }
        }

        protected void OnSelectionChanged(IViewItem oldItem, IViewItem newItem)
        {
            Selected?.Invoke(this, new ValueSelectedEventArgs(
                oldItem?.GetValue(), newItem?.GetValue()));
        }
    }
}
