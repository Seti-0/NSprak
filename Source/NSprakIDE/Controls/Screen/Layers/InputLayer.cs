using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace NSprakIDE.Controls.Screen.Layers
{
    public class InputLayer : ScreenLayer
    {
        // This is a dictionary acting as a set. I don't think there is
        // a concurrent set in C#.
        private readonly ConcurrentDictionary<Key, Key> _keysDown 
            = new ConcurrentDictionary<Key, Key>();

        private readonly Dictionary<string, List<Key>> _bindings 
            = new Dictionary<string, List<Key>>
        {
            { "up", new List<Key>{ Key.Up, Key.W } },
            { "left", new List<Key>{ Key.Left, Key.A } },
            { "down", new List<Key>{ Key.Down, Key.S } },
            { "right", new List<Key>{ Key.Right, Key.D } },
            { "space", new List<Key>{ Key.Space } }
        };

        public bool IsKeyPressed(string text)
        {
            if (_bindings.TryGetValue(text, out List<Key> keys))
                foreach (Key key in keys)
                    if (_keysDown.ContainsKey(key))
                        return true;

            return false;
        }

        public override void Render(DrawingContext context, Rect targetRect)
        {
            // Pass - this layer has no visuals
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            _keysDown[e.Key] = e.Key;
        }

        public override void OnKeyUp(KeyEventArgs e)
        {
            base.OnKeyUp(e);

            _keysDown.Remove(e.Key, out _);
        }
    }
}
