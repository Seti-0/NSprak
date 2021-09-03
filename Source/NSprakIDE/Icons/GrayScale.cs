using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace NSprakIDE.Icons
{
    /// <summary>
    /// An effect that turns the input into shades of a single color.
    /// </summary>
    public class GrayscaleEffect : ShaderEffect
    {
        // Code was adapted from https://github.com/fluentribbon/Fluent.Ribbon/blob/develop/Fluent.Ribbon/Effects/GrayscaleEffect.cs
        // (Under MIT)

        public static readonly DependencyProperty InputProperty =
            RegisterPixelShaderSamplerProperty(
                nameof(Input), 
                typeof(GrayscaleEffect), 
                0);

        public static readonly DependencyProperty FilterColorProperty =
            DependencyProperty.Register(
                nameof(FilterColor), 
                typeof(Color), 
                typeof(GrayscaleEffect),
                new PropertyMetadata(
                    Color.FromArgb(255, 255, 255, 255), 
                    PixelShaderConstantCallback(0)));

        /// <summary>
        /// Implicit input
        /// </summary>
        public Brush Input
        {
            get => (Brush)GetValue(InputProperty);
            set => SetValue(InputProperty, value);
        }

        /// <summary>
        /// The color used to tint the input.
        /// </summary>
        public Color FilterColor
        {
            get => (Color)GetValue(FilterColorProperty);
            set => SetValue(FilterColorProperty, value);
        }

        public GrayscaleEffect()
        {
            PixelShader = CreatePixelShader();

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(FilterColorProperty);
        }

        private PixelShader CreatePixelShader()
        {
            string uri = "pack://application:,,,/NSprakIDE;component/Icons/Grayscale.ps";

            PixelShader pixelShader = new PixelShader { 
                UriSource = new Uri(uri, UriKind.RelativeOrAbsolute)
            };

            return pixelShader;
        }
    }
}
