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

using ipp;

namespace MHApi.OpenGLSupport
{

    /// <summary>
    /// Drawing provider to draw a simple linear gradient
    /// with defined border coordinates and colors
    /// </summary>
    public class LinGradientProvider : TwoDProviderBase
    {
        #region Properties

        /// <summary>
        /// The topleft corner of the gradient rectangle
        /// </summary>
        public IppiPoint_32f Topleft { get; set; }

        /// <summary>
        /// The width of the rectangle - gradient direction
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        /// The height of the rectangle - orthogonal to gradient direction
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// The starting (low-x) color of the gradient
        /// </summary>
        public FColor ColStart { get; set; }

        /// <summary>
        /// The ending (high-x) color of the gradient
        /// </summary>
        public FColor ColEnd { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public LinGradientProvider()
        {
            Topleft = new IppiPoint_32f(-8, -8);
            Width = 16;
            Height = 12;
            ColStart = FColor.Black;
            ColEnd = FColor.White;
        }

        /// <summary>
        /// Constructs a new linear gradient provider
        /// </summary>
        /// <param name="topleft">The topleft corner of the gradient rectangle</param>
        /// <param name="width">The width of the gradient rectangle</param>
        /// <param name="height">The height of the gradient rectangle</param>
        /// <param name="colStart">The starting color of the gradient</param>
        /// <param name="colEnd">The ending color of the gradient</param>
        public LinGradientProvider(IppiPoint_32f topleft, float width, float height, FColor colStart, FColor colEnd)
        {
            Topleft = topleft;
            Width = width;
            Height = height;
            ColStart = colStart;
            ColEnd = colEnd;
        }

        #endregion

        /// <summary>
        /// Draws our gradient
        /// </summary>
        /// <param name="gl">The openGL drawing object</param>
        public override void Draw(OpenGL gl)
        {
            //Process alignment, rotation and translation in baseclass
            base.Draw(gl);

            gl.Begin(BeginMode.Quads);

            //Our gradient increases linearly with increasing x-coordinates
            //Therefore we draw a quad with the two left vertices being ColStart
            //and the two right vertices being ColEnd
            gl.Color(ColStart.R, ColStart.G, ColStart.B);
            gl.Vertex(Topleft.x, Topleft.y);
            gl.Vertex(Topleft.x, Topleft.y + Height);
            gl.Color(ColEnd.R, ColEnd.G, ColEnd.B);
            gl.Vertex(Topleft.x + Width, Topleft.y + Height);
            gl.Vertex(Topleft.x + Width, Topleft.y);

            gl.End();
        }

        public override string ToString()
        {
            return ToFileString();
        }

        public override string ToFileString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("LinGrad");
            sb.Append(";");
            sb.Append(Topleft.x);
            sb.Append(";");
            sb.Append(Topleft.y);
            sb.Append(";");
            sb.Append(Width);
            sb.Append(";");
            sb.Append(Height);
            sb.Append(";");
            sb.Append(ColStart.R);
            sb.Append(";");
            sb.Append(ColStart.G);
            sb.Append(";");
            sb.Append(ColStart.B);
            sb.Append(";");
            sb.Append(ColEnd.R);
            sb.Append(";");
            sb.Append(ColEnd.G);
            sb.Append(";");
            sb.Append(ColEnd.B);
            return sb.ToString();
        }

        public override void FromFileString(string s)
        {
            string[] parts = s.Split(';');
            if (parts.Length != 11 || parts[0] != "LinGrad")
                throw new ApplicationException("Provided string does not represent a FourSquareProvider");
            Topleft = new IppiPoint_32f(float.Parse(parts[1]), float.Parse(parts[2]));
            Width = float.Parse(parts[3]);
            Height = float.Parse(parts[4]);
            ColStart = new FColor(float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7]));
            ColEnd = new FColor(float.Parse(parts[8]), float.Parse(parts[9]), float.Parse(parts[10]));
        }
    }
}
