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
    /// Draws a circle on a checkerboard background
    /// matching overall circle and background luminance
    /// </summary>
    public class LumNeutralCircleProvider : TwoDProviderBase
    {
        #region Properties

        FColor _foreground;

        FColor _circle;

        /// <summary>
        /// The foreground color of the foreground checkers
        /// (circle will be half the brightness)
        /// </summary>
        public FColor Foreground
        {
            get
            {
                return _foreground;
            }
            set
            {
                _foreground = value;
                _circle = new FColor(_foreground.R / 2, _foreground.G / 2, _foreground.B / 2);
            }
        }

        /// <summary>
        /// The width (in device units) of each checker
        /// </summary>
        public float CheckerWidth { get; set; }

        /// <summary>
        /// The height (in device units) of each checker
        /// </summary>
        public float CheckerHeight { get; set; }

        /// <summary>
        /// The center of the circle IN DEVICE coordinates
        /// </summary>
        public IppiPoint_32f Center { get; set; }

        /// <summary>
        /// The radius of the circle IN DEVICE units
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// The number of segments in the circle
        /// </summary>
        public ushort SegmentCount { get; set; }

        #endregion

        /// <summary>
        /// Creates a new Luminance neutral circle provider
        /// </summary>
        public LumNeutralCircleProvider()
        {
            Foreground = FColor.Red;
            CheckerWidth = 0.1f;
            CheckerHeight = 0.1f;
            Center = new IppiPoint_32f(0, 0);
            Radius = 1;
            SegmentCount = 72;
        }

        /// <summary>
        /// Creates a new Luminance neutral circle provider
        /// </summary>
        /// <param name="foreground">The color of the foreground checkers</param>
        /// <param name="checkerWidth">The width of the checkers</param>
        /// <param name="checkerHeight">The height of the checkers</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="radius">The radius of the circle</param>
        public LumNeutralCircleProvider(FColor foreground, float checkerWidth, float checkerHeight, IppiPoint_32f center, float radius)
        {
            Foreground = foreground;
            CheckerWidth = checkerWidth;
            CheckerHeight = checkerHeight;
            Center = center;
            Radius = radius;
            SegmentCount = 72;
        }

        public override void Draw(OpenGL gl)
        {
            //The checkerboard will be drawn irrespective of alignment, ie. stationary
            //move back to origin
            gl.LoadIdentity();

            //we draw our checkerboard larger than the viewport
            float startx = -1.5f * ViewportHalfSizeX;
            float endx = -1 * startx;
            float starty = -1.5f * ViewportHalfSizeY;
            float endy = -1 * starty;

            int nCols = (int)((endx - startx) / CheckerWidth);
            int nRows = (int)((endy - starty) / CheckerHeight);


            gl.Begin(OpenGL.GL_QUADS);
            //loop over checkers
            for (int i = 0; i < nCols; i++)
                for (int j = 0; j < nRows; j++)
                {
                    //in even rows, even columns will have the foreground color the other checkers are black
                    if ((i % 2 == 0) ^ (j % 2 == 1))
                        gl.Color(_foreground.R, _foreground.G, _foreground.B);
                    else
                        gl.Color(0f, 0f, 0f);
                    //draw checker
                    gl.Vertex(i * CheckerWidth + startx, j * CheckerHeight + starty);
                    gl.Vertex(i * CheckerWidth + startx, (j + 1) * CheckerHeight + starty);
                    gl.Vertex((i + 1) * CheckerWidth + startx, (j + 1) * CheckerHeight + starty);
                    gl.Vertex((i + 1) * CheckerWidth + startx, j * CheckerHeight + starty);
                }

            gl.End();

            //Take care of alignment, rotation and translation for the circle
            base.Draw(gl);

            //approximate circle using a triangle fan
            //the more triangles are drawn (the lower the degree step)
            //the smoother the circle will be
            float angleStep = (float)(2 * Math.PI / SegmentCount);

            //To draw the circle we first compute coordinates around
            //the origin and then translate the coordinates according
            //to the circles center
            gl.Begin(BeginMode.TriangleFan);

            for (float angle = 0; angle <= (float)(2 * Math.PI); angle += angleStep)
            {
                gl.Color(_circle.R, _circle.G, _circle.B);
                gl.Vertex(Center.x + Math.Sin(angle) * Radius, Center.y + Math.Cos(angle) * Radius);
            }

            gl.End();
        }

        #region String representation
        public override string ToFileString()
        {
            throw new NotImplementedException();
        }

        public override void FromFileString(string s)
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
