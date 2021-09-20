using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

using NSprak.Execution;
using NSprak.Functions.Signatures;

namespace NSprakIDE.Controls.Execution
{
    public partial class FrameView : UserControl
    {
        public Executor Target { get; set; }

        public class ItemWrapper
        {
            public string Location { get; }

            public string Signature { get; }

            public ItemWrapper(int location, FunctionSignature signature)
            {
                Location = location.ToString();
                Signature = signature.ToString();
            }
        }

        public FrameView()
        {
            InitializeComponent();
        }

        public void Update()
        {
            if (Target == null)
                MainGrid.ItemsSource = null;

            else
            {
                /*
                List<ItemWrapper> items = Target
                    .Frames.Zip(Target.DebugInfo)
                    .Select(x => new ItemWrapper(x.First, x.Second))
                    .ToList();

                MainGrid.ItemsSource = items;
                */
            }
        }
    }
}
