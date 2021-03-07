using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Extensions.Logging;

using AvalonDock;
using AvalonDock.Controls;
using AvalonDock.Layout;

namespace NSprakIDE.Docking
{
    public static class LayoutNames
    {
        public const string Main = "Main";
    }

    public class NSprakSaveManager : LayoutSaveManager
    {
        private LayoutAnchorablePane _outputPane;
        private LayoutAnchorablePane _debugPane;
        private LayoutDocumentPane _documentPane;

        private const string
            OutputPaneName = "OutputPane",
            DebugPaneName = "DebugPane";

        public NSprakSaveManager(DockingManager manager, string saveFolderPath = "Layouts") : base(manager, saveFolderPath)
        {
            _documentPane = new LayoutDocumentPane();

            _outputPane = new LayoutAnchorablePane();
            _outputPane.Name = OutputPaneName;

            _debugPane = new LayoutAnchorablePane();
            _debugPane.Name = DebugPaneName;
        }

        public override void SetupDefaults()
        {
            LoadLayout(LayoutNames.Main, CreateMainLayout());
        }

        private LayoutPanel CreateMainLayout()
        {
            LayoutPanel left = new LayoutPanel();
            left.Orientation = Orientation.Vertical;
            left.Children.Add(_documentPane);
            left.Children.Add(_debugPane);

            LayoutPanel result = new LayoutPanel();
            result.Orientation = Orientation.Horizontal;
            result.Children.Add(left);
            result.Children.Add(_outputPane);

            left.DockWidth = new GridLength(0.55, GridUnitType.Star);
            _outputPane.DockWidth = new GridLength(0.45, GridUnitType.Star);

            _documentPane.DockHeight = new GridLength(0.65, GridUnitType.Star);
            _debugPane.DockHeight = new GridLength(300, GridUnitType.Pixel);

            return result;
        }

        public LayoutDocument AddDocument(string title, object content)
        {
            LayoutDocument document = new LayoutDocument
            {
                Title = title,
                Content = content
            };

            if (!TryFindDocumentPane(out LayoutDocumentPane pane))
            {
                Logs.Core.LogError("Failed to find document pane");
                Logs.Core.LogDebug("Unable to add {Name}: \"{Title}\"", content.GetType().Name, title);
                return null;
            }

            pane.Children.Add(document);
            document.IsActive = true;
            return document;
        }

        public LayoutAnchorable AddConsoleAnchorable(string name, object content)
        {
            return AddAnchorable(OutputPaneName, name, content);
        }

        public LayoutAnchorable AddDebugAnchorable(string name, object content)
        {
            return AddAnchorable(DebugPaneName, name, content);
        }

        private LayoutAnchorable AddAnchorable(string paneName, string contentName, object content)
        {
            LayoutAnchorable anchorable = new LayoutAnchorable()
            {
                Title = contentName,
                Content = content
            };

            if (!TryFindAnchorable(paneName, out LayoutAnchorablePane pane))
            {
                Logs.Core.LogError("Failed to find anchorable pane: {Name}", paneName);
                Logs.Core.LogDebug("Unable to add {Type}: \"{Name}\"", content.GetType().Name, contentName);
                return null;
            }

            pane.Children.Add(anchorable);
            return anchorable;
        }
    }
}
