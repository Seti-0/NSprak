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

        object getValue();
    }

    public class ViewItem<T> : IViewItem
    {
        public string Name { get; set; }

        public bool Underline { get; set; }

        public T Value { get; set; }

        public ViewItem(string name, bool underline, T value)
        {
            Name = name;
            Underline = underline;
            Value = value;
        }

        public object getValue() => Value;
    }

    public abstract class ViewSupplier<T>
    {
        private Dictionary<string, List<ViewItem<T>>> _categories 
            = new Dictionary<string, List<ViewItem<T>>>();

        private ViewSelect _view;

        public ViewSupplier(ViewSelect view)
        {
            _view = view;
        }

        public void StartCategory(string name)
        {
            if (_categories.ContainsKey(name))
            {
                string message = $"Category \"{name}\" already exists";
                throw new ArgumentException(message);

            }

            _categories.Add(name, new List<ViewItem<T>>());
        }

        public T CreateItem(string name, string categoryName)
        {
            List<ViewItem<T>> category;
            if (!_categories.TryGetValue(categoryName, out category))
            {
                string message = $"Unknown category \"{categoryName}\"";
                throw new ArgumentException(message);
            }

            T value = CreateItem();
            category.Add(new ViewItem<T>(name, false, value));
            UpdateView();

            return value;
        }

        public void RemoveItem(T item)
        {
            foreach (List<ViewItem<T>> category in _categories.Values)
            {
                category.RemoveAll(
                    x => EqualityComparer<T>.Default.Equals(x.Value, item));
            }
        }

        private void UpdateView()
        {
            List<ViewItem<T>> items = new List<ViewItem<T>>();
            foreach (List<ViewItem<T>> category in _categories.Values)
            {
                foreach (ViewItem<T> item in category)
                {
                    item.Underline = false;
                    items.Add(item);
                }

                items[^1].Underline = true;
            }

            _view.UpdateOptions(items);
        }

        protected abstract T CreateItem();
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
        private IEnumerable<IViewItem> _items = new IViewItem[0];
        private IViewItem _selectedItem;

        public event EventHandler<ValueSelectedEventArgs> Selected;

        public ViewSelect()
        {
            InitializeComponent();
        }

        public void UpdateOptions(IEnumerable<IViewItem> items)
        {
            Selection.ItemsSource = items;
            UpdateSelection();
        }

        public void UpdateSelection(IViewItem newItem = null)
        {
            if (_selectedItem == newItem)
                return;

            IViewItem previousSelection = _selectedItem;

            if (newItem is null)
            {
                // Check that the currently selected item is still valid,
                // otherwise, leave it alone.
                if (!_items.Contains(_selectedItem))
                    _selectedItem = null;
            }
            else _selectedItem = newItem;

            if (previousSelection != _selectedItem)
                OnSelectionChanged(previousSelection, _selectedItem);
        }

        private void Selection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSelection((IViewItem)Selection.SelectedItem);
        }

        protected void OnSelectionChanged(IViewItem oldValue, IViewItem newValue)
        {
            Selected?.Invoke(this, new ValueSelectedEventArgs(oldValue, newValue));
        }
    }
}
