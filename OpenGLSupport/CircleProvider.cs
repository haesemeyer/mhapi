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
                gl.Vertex(Center.x + Math.Sin(angle) * Radius, Center.y + Math.Cos(angle) * Radius);
            }

            gl.End();
        }

        public override string ToFileString()
        {
            throw new NotImplementedException();
        }

        public override void FromFileString(string s)
        {
            throw new NotImplementedException();
        }
    }
}
