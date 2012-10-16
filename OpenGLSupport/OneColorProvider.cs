using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.Enumerations;

namespace MHApi.OpenGLSupport
{
    public class OneColorProvider : TwoDProviderBase
    {

        public FColor Color { get; set; }

        /// <summary>
        /// Default constructor - Black color
        /// </summary>
        public OneColorProvider()
        {
            Color = FColor.Black;
        }

        public OneColorProvider(FColor color)
        {
            Color = color;
        }

        public override void Draw(SharpGL.OpenGL gl)
        {
            //Process alignment in baseclass
            base.Draw(gl);

            //Draw the square on screen
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(Color.R, Color.G, Color.B);
            gl.Vertex(-15f, -15f);
            gl.Vertex(-15f, 15f);
            gl.Vertex(15f, 15f);
            gl.Vertex(15f, -15f);
            gl.End();
            gl.Flush();
        }

        /// <summary>
        /// Converts the provider data to a savable filestring
        /// </summary>
        /// <returns>A string representation of the provider</returns>
        public override string ToFileString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("OneColor");
            b.Append(";");
            b.Append(Color.R);
            b.Append(";");
            b.Append(Color.G);
            b.Append(";");
            b.Append(Color.B);
            return b.ToString();
        }

        /// <summary>
        /// User friendlier string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("OneColor:");
            var c = Color.ToWPFColor();
            b.Append(string.Format("S1({0};{1};{2})", c.R, c.G, c.B));
            return b.ToString();
        }

        /// <summary>
        /// Populates a drawing provider from a string representation
        /// </summary>
        /// <param name="s">The string representation</param>
        public override void FromFileString(string s)
        {
            string[] parts = s.Split(';');
            if (parts.Length != 4 || parts[0]!="OneColor")
                throw new ApplicationException("Provided string does not represent a OneColorProvider");
            Color = new FColor(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
        }
    }
}
