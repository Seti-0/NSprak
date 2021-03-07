using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Documents;

using NSprakIDE.Controls.Output;

namespace NSprakIDE.Controls
{
    /*
     
    This page of code is one of the laziest I've ever written - to come back to when
    other things are further along.

     */

    public class OutputSelectionItem
    {
        public string Value { get; set; }

        public bool Underline { get; set; }

        public OutputSelectionItem(string value, bool underline)
        {
            Value = value;
            Underline = underline;
        }
    }

    /// <summary>
    /// Interaction logic for OutputView.xaml
    /// </summary>
    public partial class OutputView : UserControl
    {
        public const string MainCategory = "Main";

        private Dictionary<string, List<OutputLog>> _categories = new Dictionary<string, List<OutputLog>>();
        private Dictionary<string, OutputLog> _logs = new Dictionary<string, OutputLog>();

        private OutputSelectionItem _selectedItem;
        private List<OutputSelectionItem> _options = new List<OutputSelectionItem>();

        private OutputLog _selectedLog;

        public OutputView()
        {
            InitializeComponent();

            CreateCategory(MainCategory);
            UpdateOptions();
        }

        public void CreateCategory(string name)
        {
            if (_categories.ContainsKey(name))
                throw new ArgumentException($"Category \"{name}\" already exists");

            _categories.Add(name, new List<OutputLog>());
            UpdateOptions();
        }

        public OutputLog StartLog(string categoryName, string name)
        {
            if (_logs.ContainsKey(name))
                throw new ArgumentException($"Log \"{name}\" already exists");

            if (_categories.TryGetValue(categoryName, out List<OutputLog> category))
            {
                OutputLog result = new OutputLog(name, categoryName, this);
                
                category.Add(result);
                _logs.Add(name, result);

                UpdateOptions();
                return result;
            }
            else throw new ArgumentException($"Reference to non-existent category \"{categoryName}\"");
        }

        public void EndLog(string name)
        {
            if (_logs.TryGetValue(name, out OutputLog log))
                _categories[log.Category].Remove(log);
        }

        public void Select(string name)
        {
            if (name == null)
            {
                _selectedLog = null;
                UpdateSelection();
            }
            else if (_selectedLog.Name != name)
            {
                _selectedLog = _logs[name];
                UpdateSelection();
            }
        }

        public void UpdateSelection()
        {
            if (_options.Count == 0)
            {
                _selectedLog = null;
                _selectedItem = null;
                RichText.Document = new FlowDocument();
                OutputSelection.SelectedItem = null;
                return;
            }

            if (_selectedLog != null)
            {
                foreach (OutputSelectionItem item in _options)
                    if (item.Value == _selectedLog.Name)
                        _selectedItem = item;

                if (_selectedItem == null)
                    _selectedLog = null;
            }
            else
            {
                _selectedItem = _options.First();
                _selectedLog = _logs[_selectedItem.Value];
            }

            RichText.Document = _selectedLog.Document;
            OutputSelection.SelectedItem = _selectedItem;
            RichText.ScrollToEnd();
        }

        private void UpdateOptions()
        {
            _options = new List<OutputSelectionItem>();
            
            foreach (List<OutputLog> category in _categories.Values)
            {
                _options.AddRange(category.Select(x => new OutputSelectionItem(x.Name, false)));

                if (category.Count > 0)
                    _options[_options.Count - 1].Underline = true;
            }

            OutputSelection.ItemsSource = _options;
            UpdateSelection();
        }

        private void OutputSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Select(((OutputSelectionItem)OutputSelection.SelectedItem)?.Value);
        }
    }
}
