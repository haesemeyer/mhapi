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
    /// <summary>
    /// Draws a 2-color checkerboard with given checker-size
    /// </summary>
    public class CheckerBoardProvider : TwoDProviderBase
    {
        #region Properties

        /// <summary>
        /// The first color of the checkerboard
        /// </summary>
        public FColor Color1 { get; set; }

        /// <summary>
        /// The second color of the checkerboard
        /// </summary>
        public FColor Color2 { get; set; }

        /// <summary>
        /// The width (in device units) of each checker
        /// </summary>
        public float CheckerWidth { get; set; }

        /// <summary>
        /// The height (in device units) of each checker
        /// </summary>
        public float CheckerHeight { get; set; }

        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public CheckerBoardProvider() : this(FColor.Red,FColor.Black,0.5f,0.5f) {}

        public CheckerBoardProvider(FColor color1, FColor color2, float checkerWidth, float checkerHeight)
        {
            Color1 = color1;
            Color2 = color2;
            CheckerWidth = checkerWidth;
            CheckerHeight = checkerHeight;
        }

        public override void Draw(OpenGL gl)
        {
            base.Draw(gl);

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
                    //in even rows, even columns will have Color1
                    if ((i % 2 == 0) ^ (j % 2 == 1))
                        gl.Color(Color1.R, Color1.G, Color1.B);
                    else
                        gl.Color(Color2.R, Color2.G, Color2.B);
                    //draw checker
                    gl.Vertex(i * CheckerWidth + startx, j * CheckerHeight + starty);
                    gl.Vertex(i * CheckerWidth + startx, (j + 1) * CheckerHeight + starty);
                    gl.Vertex((i + 1) * CheckerWidth + startx, (j + 1) * CheckerHeight + starty);
                    gl.Vertex((i + 1) * CheckerWidth + startx, j * CheckerHeight + starty);
                }

            gl.End();
        }

        #region StringRepresentation
        

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
