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

using ipp;

using SharpGL;
using SharpGL.Enumerations;

namespace MHApi.OpenGLSupport
{
    /// <summary>
    /// Represents the corners of an openGL quad
    /// and provides a method for drawing the quad in solid color
    /// </summary>
    public class Quad
    {
        public IppiPoint_32f Corner1 { get; set; }

        public IppiPoint_32f Corner2 { get; set; }

        public IppiPoint_32f Corner3 { get; set; }

        public IppiPoint_32f Corner4 { get; set; }

        public FColor Color { get; set; }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Quad()
        {
            Corner1 = new IppiPoint_32f();
            Corner2 = new IppiPoint_32f();
            Corner3 = new IppiPoint_32f();
            Corner4 = new IppiPoint_32f();
            Color = FColor.Black;
        }

        /// <summary>
        /// Constructs a new quad with the specified corners and color
        /// corners should be indicated in clock-wise direction
        /// </summary>
        /// <param name="corner1"></param>
        /// <param name="corner2"></param>
        /// <param name="corner3"></param>
        /// <param name="corner4"></param>
        /// <param name="color"></param>
        public Quad(IppiPoint_32f corner1, IppiPoint_32f corner2, IppiPoint_32f corner3, IppiPoint_32f corner4, FColor color)
        {
            Corner1 = corner1;
            Corner2 = corner2;
            Corner3 = corner3;
            Corner4 = corner4;
            Color = color;
        }

        /// <summary>
        /// Draws the quad with a solid fill color
        /// </summary>
        /// <param name="gl">The openGl context on which to draw the quad</param>
        public void Draw(OpenGL gl)
        {
            gl.Color(Color.R, Color.G, Color.B);
            gl.Begin(BeginMode.Quads);
            gl.Vertex(Corner1.x, Corner1.y);
            gl.Vertex(Corner2.x, Corner2.y);
            gl.Vertex(Corner3.x, Corner3.y);
            gl.Vertex(Corner4.x, Corner4.y);
            gl.End();
        }
    }
}
