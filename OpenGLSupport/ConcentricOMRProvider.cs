using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpGL;
using SharpGL.Enumerations;

namespace MHApi.OpenGLSupport
{

    /// <summary>
    /// Class to draw concentric circles on screen wich
    /// can move as a concentric OMR grating
    /// </summary>
    public class ConcentricOMRProvider : TwoDProviderBase
    {
        #region Fields

        /// <summary>
        /// Used to implement concentric movement
        /// The diameter "offset" of the inner two circles
        /// Oscillates within [-StripeWidth, StripeWidth]
        /// </summary>
        volatile float _innerDiameter;

        #endregion

        #region Properties

        /// <summary>
        /// The color of the first circular stripe in the grating
        /// </summary>
        public FColor ColorStripe1 { get; set; }


        /// <summary>
        /// The color of the second circular stripe in the grating
        /// </summary>
        public FColor ColorStripe2 { get; set; }

        /// <summary>
        /// The width of the two grating stripes in device coordinates
        /// </summary>
        public float StripeWidth { get; set; }

        /// <summary>
        /// The number of segments to draw for each circle
        /// </summary>
        public ushort SegmentCount { get; set; }


        #endregion

        /// <summary>
        /// Constructs a new ConcentricOMRProvider
        /// </summary>
        /// <param name="color1">The color of the first stripe</param>
        /// <param name="color2">The color of the second stripe</param>
        /// <param name="stripeWidth">The width (radius-difference) of each stripe</param>
        public ConcentricOMRProvider(FColor color1, FColor color2, float stripeWidth)
        {
            ColorStripe1 = color1;
            ColorStripe2 = color2;
            StripeWidth = stripeWidth;
            SegmentCount = 72;
        }

        #region Methods

        /// <summary>
        /// Shrinks the OMR grating towards the center by StepSize
        /// </summary>
        /// <param name="stepSize"> In device coordinates the radius reduction of the circles</param>
        public void Shrink(float stepSize)
        {
            if (stepSize < 0)
                throw new ArgumentOutOfRangeException("stepSize");
            //this will be called from a different thread then the drawing
            //we need to make sure that overflows of Shrink are never drawn
            var innerDiam = _innerDiameter;
            innerDiam -= stepSize;
            if (innerDiam < -1 * StripeWidth)
                innerDiam += 2 * StripeWidth;

            _innerDiameter = innerDiam;
        }

        /// <summary>
        /// Inflates the OMR grating away from the center by StepSize
        /// </summary>
        /// <param name="stepSize">In device coordinates the radius reduction of the circles</param>
        public void Inflate(float stepSize)
        {
            if (stepSize < 0)
                throw new ArgumentOutOfRangeException("stepSize");
            //this will be called from a different thread then the drawing
            //we need to make sure that overflows of Inflate are never drawn
            var innerDiam = _innerDiameter;
            innerDiam += stepSize;
            if (innerDiam > StripeWidth)
                innerDiam -= 2 * StripeWidth;

            _innerDiameter = innerDiam;
        }

        public override void Draw(SharpGL.OpenGL gl)
        {
            base.Draw(gl);

            //approximate circle using a triangle fan
            //the more triangles are drawn (the lower the degree step)
            //the smoother the circle will be
            float angleStep = (float)(2 * Math.PI / SegmentCount);

            //All circles will be drawn concentric around the origin
            //starting with the outermost circle (otherwise we would overpaint
            //the inner circles)

            // we draw the circles extending to a larger area than the viewport
            float radiusMax = Math.Max(1.5f * ViewportHalfSizeX, 1.5f * ViewportHalfSizeY);
            int nCircles = (int)(radiusMax / StripeWidth);



            //prevent changes to our inner diameter while we draw
            float innerDiam = _innerDiameter;
                

                //loop over circles
            for (int i = nCircles - 1; i >= 0; i--)
            {
                float Radius = i * StripeWidth + innerDiam;
                if (Radius <= 0)//don't draw inner circles if they are currently collapsed into the center
                    break;
                FColor Color;
                if (i % 2 == 0)
                    Color = ColorStripe1;
                else
                    Color = ColorStripe2;
                gl.Begin(BeginMode.TriangleFan);
                for (float angle = 0; angle <= (float)(2 * Math.PI); angle += angleStep)
                {
                    gl.Color(Color.R, Color.G, Color.B);
                    gl.Vertex(Math.Sin(angle) * Radius, Math.Cos(angle) * Radius);
                }
                gl.End();
            }


        }

        #endregion

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
