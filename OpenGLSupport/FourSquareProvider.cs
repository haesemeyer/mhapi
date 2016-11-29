/*
Copyright 2016 Martin Haesemeyer
   Licensed under the MIT License, see License.txt.
   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
   IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
   FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
   AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
   LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
   OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
   SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpGL;
using SharpGL.Enumerations;

namespace MHApi.OpenGLSupport
{
    public class FourSquareProvider : TwoDProviderBase
    {
        /// <summary>
        /// Default constructor
        /// </summary>
        public FourSquareProvider()
        {
            ColorSquare1 = new FColor(1, 0.2f, 0.2f);
            ColorSquare2 = new FColor(0.3f, 0.3f, 0.3f);
            ColorSquare3 = new FColor(0.5f, 0.6f, 0.1f);
            ColorSquare4 = new FColor(0.2f, 0.1f, 1f);
        }

        /// <summary>
        /// Constructs a new four-square provider with the indicated colors
        /// </summary>
        /// <param name="square1">Color of square 1</param>
        /// <param name="square2">Color of square 2</param>
        /// <param name="square3">Color of square 3</param>
        /// <param name="square4">Color of square 4</param>
        public FourSquareProvider(FColor square1, FColor square2, FColor square3, FColor square4)
        {
            ColorSquare1 = square1;
            ColorSquare2 = square2;
            ColorSquare3 = square3;
            ColorSquare4 = square4;
        }

        public override void Draw(SharpGL.OpenGL gl)
        {
            //Process alignment in baseclass
            base.Draw(gl);

            //Draw the color "squares" on screen - make bigger than the viewport
            //so that alignment translation and rotation don't give edges
            gl.Begin(OpenGL.GL_QUADS);

            //first square - top left
            gl.Color(ColorSquare1.R, ColorSquare1.G, ColorSquare1.B);
            gl.Vertex(-15f, -15f);
            gl.Vertex(-15f, 0f);
            gl.Vertex(0f, 0f);
            gl.Vertex(0f, -15f);

            //second square - bottom left
            gl.Color(ColorSquare2.R, ColorSquare2.G, ColorSquare2.B);
            gl.Vertex(-15f, 0f);
            gl.Vertex(-15f, 15f);
            gl.Vertex(0f, 15f);
            gl.Vertex(0f, 0f);

            //third square - top right
            gl.Color(ColorSquare3.R, ColorSquare3.G, ColorSquare3.B);
            gl.Vertex(0f, -15f);
            gl.Vertex(0f, 0f);
            gl.Vertex(15f, 0f);
            gl.Vertex(15f, -15f);

            //fourth square - bottom right
            gl.Color(ColorSquare4.R, ColorSquare4.G, ColorSquare4.B);
            gl.Vertex(0f, 0f);
            gl.Vertex(0f, 15f);
            gl.Vertex(15f, 15f);
            gl.Vertex(15f, 0f);

            gl.End();
        }

        /// <summary>
        /// Converts the provider data to a savable filestring
        /// </summary>
        /// <returns>A string representation of the provider</returns>
        public override string ToFileString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("FourSquare");
            b.Append(";");
            b.Append(ColorSquare1.R);
            b.Append(";");
            b.Append(ColorSquare1.G);
            b.Append(";");
            b.Append(ColorSquare1.B);
            b.Append(";");
            b.Append(ColorSquare2.R);
            b.Append(";");
            b.Append(ColorSquare2.G);
            b.Append(";");
            b.Append(ColorSquare2.B);
            b.Append(";");
            b.Append(ColorSquare3.R);
            b.Append(";");
            b.Append(ColorSquare3.G);
            b.Append(";");
            b.Append(ColorSquare3.B);
            b.Append(";");
            b.Append(ColorSquare4.R);
            b.Append(";");
            b.Append(ColorSquare4.G);
            b.Append(";");
            b.Append(ColorSquare4.B);
            return b.ToString();
        }

        /// <summary>
        /// User friendlier string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("FourSquare:");
            var c = ColorSquare1.ToWPFColor();
            b.Append(string.Format("S1({0};{1};{2}) ", c.R, c.G, c.B));
            c = ColorSquare2.ToWPFColor();
            b.Append(string.Format("S2({0};{1};{2}) ", c.R, c.G, c.B));
            c = ColorSquare3.ToWPFColor();
            b.Append(string.Format("S3({0};{1};{2}) ", c.R, c.G, c.B));
            c = ColorSquare4.ToWPFColor();
            b.Append(string.Format("S4({0};{1};{2})", c.R, c.G, c.B));
            return b.ToString();
        }

        /// <summary>
        /// Populates a drawing provider from a string representation
        /// </summary>
        /// <param name="s">The string representation</param>
        public override void FromFileString(string s)
        {
            string[] parts = s.Split(';');
            if (parts.Length != 13 || parts[0] != "FourSquare")
                throw new ApplicationException("Provided string does not represent a FourSquareProvider");
            ColorSquare1 = new FColor(float.Parse(parts[1]),float.Parse(parts[2]),float.Parse(parts[3]));
            ColorSquare2 = new FColor(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]));
            ColorSquare3 = new FColor(float.Parse(parts[7]), float.Parse(parts[8]), float.Parse(parts[9]));
            ColorSquare4 = new FColor(float.Parse(parts[10]), float.Parse(parts[11]), float.Parse(parts[12]));
        }

        #region Properties

        /// <summary>
        /// The color of the first square (top left)
        /// </summary>
        public FColor ColorSquare1 { get; set; }

        /// <summary>
        /// The color of the second square (bottom left)
        /// </summary>
        public FColor ColorSquare2 { get; set; }

        /// <summary>
        /// The color of the third square (top right)
        /// </summary>
        public FColor ColorSquare3 { get; set; }

        /// <summary>
        /// The color of the fourth square (bottom right)
        /// </summary>
        public FColor ColorSquare4 { get; set; }

        #endregion

    }
}
