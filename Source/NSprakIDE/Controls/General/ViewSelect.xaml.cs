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
        public const string MainCategory = "Main";

        private Dictionary<string, List<ViewItem<T>>> _categories 
            = new Dictionary<string, List<ViewItem<T>>>();

        private ViewSelect _view;

        public ViewSupplier(ViewSelect view)
        {
            _view = view;
            StartCategory(MainCategory);
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

        public T Start(string name, string categoryName = MainCategory)
        {
            List<ViewItem<T>> category;
            if (!_categories.TryGetValue(categoryName, out category))
            {
                string message = $"Unknown category \"{categoryName}\"";
                throw new ArgumentException(message);
            }

            T value = Create(name, categoryName);
            category.Add(new ViewItem<T>(name, false, value));
            UpdateView();

            return value;
        }

        public void Select(T value)
        {
            // I should really make a lookup instead of having this
            // search each time a tab is clicked. Not that there are ever 
            // many items, but it would be neater.
            IViewItem item = null;
            foreach (List<ViewItem<T>> category in _categories.Values)
                foreach (ViewItem<T> option in category)
                    if (EqualityComparer<T>.Default.Equals(option.Value, value))
                    {
                        item = option;
                        break;
                    }

            _view.Select(item);
        }

        public void End(T item)
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

        protected abstract T Create(string name, string category);
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
        private IEnumerable<IViewItem> _items;
        private IViewItem _selectedItem;

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
                oldItem?.getValue(), newItem?.getValue()));
        }
    }
}
