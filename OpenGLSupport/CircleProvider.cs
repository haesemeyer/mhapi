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
    public class CircleProvider : TwoDProviderBase
    {

        #region Properties

        /// <summary>
        /// The color of the circle
        /// </summary>
        public FColor Color { get; set; }

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

        #region Constructors

        /// <summary>
        /// Constructs a new circle provider with
        /// default properties
        /// </summary>
        public CircleProvider()
        {
            Color = FColor.Black;
            Center = new IppiPoint_32f(0, 0);
            Radius = 1;
            SegmentCount = 72;
        }

        /// <summary>
        /// Constructs a new circle provider
        /// </summary>
        /// <param name="color">The (fill) color of the circle</param>
        /// <param name="center">The circles center in device coordinates</param>
        /// <param name="radius">The circles radius in device units</param>
        public CircleProvider(FColor color, IppiPoint_32f center, float radius)
        {
            Color = color;
            Center = center;
            Radius = radius;
            SegmentCount = 72;
        }

        /// <summary>
        /// Constructs a new circle provider
        /// </summary>
        /// <param name="color">The (fill) color of the circle</param>
        /// <param name="center">The circles center in device coordinates</param>
        /// <param name="radius">The circles radius in device units</param>
        /// <param name="segmentCount">The number of segments to draw - more=smoother circle</param>
        public CircleProvider(FColor color, IppiPoint_32f center, float radius, ushort segmentCount)
        {
            Color = color;
            Center = center;
            Radius = radius;
            SegmentCount = segmentCount;
        }

        #endregion

        public override void Draw(SharpGL.OpenGL gl)
        {
            base.Draw(gl);

            //approximate circle using a triangle fan
            //the more triangles are drawn (the lower the degree step)
            //the smoother the circle will be
            float angleStep = (float)(2 * Math.PI / SegmentCount);

            //To draw the circle we first compute coordinates around
            //the origin and then translate the coordinates according
            //to the circles center
            gl.Begin(BeginMode.TriangleFan);

            for (float angle = 0; angle <= (float)(2*Math.PI); angle += angleStep)
            {
                gl.Color(Color.R, Color.G, Color.B);
                gl.Vertex(Center.x + Math.Sin(angle) * Radius, Center.y + Math.Cos(angle) * Radius);
            }

            gl.End();
        }

        /// <summary>
        /// User friendlier string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("Circle:");
            var c = Color.ToWPFColor();
            b.Append(string.Format("Col({0},{1},{3}) ", c.R, c.G, c.B));
            b.Append(string.Format("Rad:{0} ", Radius));
            b.Append(string.Format("Centroid: {0},{1}", Center.x, Center.y));
            return b.ToString();
        }

        /// <summary>
        /// Converts the provider data to a savable filestring
        /// </summary>
        /// <returns>A string representation of the provider</returns>
        public override string ToFileString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("Circle");
            b.Append(";");
            b.Append(Color.R);
            b.Append(";");
            b.Append(Color.G);
            b.Append(";");
            b.Append(Color.B);
            b.Append(";");
            b.Append(Radius);
            b.Append(";");
            b.Append(Center.x);
            b.Append(";");
            b.Append(Center.y);
            b.Append(";");
            b.Append(SegmentCount);
            return b.ToString();
        }

        /// <summary>
        /// Populates a drawing provider from a string representation
        /// </summary>
        /// <param name="s">The string representation</param>
        public override void FromFileString(string s)
        {
            string[] parts = s.Split(';');
            if (parts.Length != 8 || parts[0] != "Circle")
                throw new ApplicationException("Provided string does not represent a CircleProvider");
            Color = new FColor(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
            Radius = float.Parse(parts[4]);
            Center = new IppiPoint_32f(float.Parse(parts[5]), float.Parse(parts[6]));
            SegmentCount = ushort.Parse(parts[7]);
        }
    }
}
