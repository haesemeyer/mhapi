using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;

namespace MHApi.OpenGLSupport
{
    /// <summary>
    /// Represents a floating point color
    /// as used by open gl
    /// </summary>
    public struct FColor
    {
        public float R;

        public float G;

        public float B;

        /// <summary>
        /// Constructs a new FColor with the specified RGB values
        /// </summary>
        /// <param name="r">The red component</param>
        /// <param name="g">The green component</param>
        /// <param name="b">The blue component</param>
        public FColor(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        /// <summary>
        /// Constructs a new grey-level "color"
        /// </summary>
        /// <param name="greylevel">The grey-level to use</param>
        public FColor(float greylevel)
        {
            R = G = B = greylevel;
        }

        /// <summary>
        /// Constructs a new color from a WPF color
        /// </summary>
        /// <param name="color">The color to represent in floating point</param>
        public FColor(Color color)
        {
            R = color.R / 255f;
            G = color.G / 255f;
            B = color.B / 255f;
        }

        /// <summary>
        /// Transforms the FColor into a System.Windows.Media.Color object
        /// </summary>
        /// <returns>The corresponding System.Windows.Media Color</returns>
        public Color ToWPFColor()
        {
            var retval = new Color();
            retval.A = 255;
            retval.R = (byte)(R * 255);
            retval.G = (byte)(G * 255);
            retval.B = (byte)(B * 255);
            return retval;
        }

        /// <summary>
        /// Constructs a new FColor object based on a color
        /// described in hsv space
        /// </summary>
        /// <param name="h">The hue of the color [0,1]</param>
        /// <param name="s">The saturation of the color [0,1]</param>
        /// <param name="v">The value of the color [0,1]</param>
        /// <returns>A new FColor object</returns>
        public static FColor FromHSV(float h, float s, float v)
        {
            float r, g, b;
            int i = (int)Math.Floor(h * 6);
            var f = h * 6 - i;
            var p = v * (1 - s);
            var q = v * (1 - f * s);
            var t = v * (1 - (1 - f) * s);
            switch (i % 6)
            {
                case 0: r = v; g = t; b = p; break;
                case 1: r = q; g = v; b = p; break;
                case 2: r = p; g = v; b = t; break;
                case 3: r = p; g = q; b = v; break;
                case 4: r = t; g = p; b = v; break;
                default: r = v; g = p; b = q; break;
            }
            return new FColor(r, g, b);
        }

        public static FColor Red
        {
            get
            {
                return new FColor(1, 0, 0);
            }
        }

        public static FColor Green
        {
            get
            {
                return new FColor(0, 1, 0);
            }
        }

        public static FColor Blue
        {
            get
            {
                return new FColor(0, 0, 1);
            }
        }

        public static FColor Yellow
        {
            get
            {
                return new FColor(1, 1, 0);
            }
        }

        public static FColor Magenta
        {
            get
            {
                return new FColor(1, 0, 1);
            }
        }

        public static FColor Cyan
        {
            get
            {
                return new FColor(0, 1, 1);
            }
        }

        public static FColor White
        {
            get
            {
                return new FColor(1, 1, 1);
            }
        }

        public static FColor Black
        {
            get
            {
                return new FColor();
            }
        }

        public static FColor Grey50
        {
            get
            {
                return new FColor(0.5f, 0.5f, 0.5f);
            }
        }
    }
}
