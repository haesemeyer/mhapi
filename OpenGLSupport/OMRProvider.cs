using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpGL;
using SharpGL.Enumerations;

namespace MHApi.OpenGLSupport
{
    public class OMRProvider:TwoDProviderBase
    {

        #region Properties

        /// <summary>
        /// The color of the first stripe in the grating
        /// </summary>
        public FColor ColorStripe1 { get; set; }


        /// <summary>
        /// The color of the second stripe in the grating
        /// </summary>
        public FColor ColorStripe2 { get; set; }

        /// <summary>
        /// The width of the two grating stripes in device coordinates
        /// </summary>
        public float StripeWidth { get; set; }

        #endregion

        /// <summary>
        /// Constructs a new OMR provider
        /// </summary>
        public OMRProvider() : this(FColor.Black,FColor.White,0.5f){}

        /// <summary>
        /// Constructs a new OMR provider
        /// </summary>
        /// <param name="colStripe1">The color of the first grating stripe</param>
        /// <param name="colStripe2">The second color in the grating</param>
        /// <param name="stripeWidth">The width of the stripes in device coordinates</param>
        public OMRProvider(FColor colStripe1, FColor colStripe2, float stripeWidth)
        {
            ColorStripe1 = colStripe1;
            ColorStripe2 = colStripe2;
            StripeWidth = stripeWidth;
        }

        #region Methods

        public override void Draw(OpenGL gl)
        {
            base.Draw(gl);

            //we draw our grating larger than the viewport
            float startx = -1.5f * ViewportHalfSizeX;
            float endx = -1 * startx;
            float starty = -1.5f * ViewportHalfSizeY;
            float endy = -1 * starty;

            //we draw the grating with stripes parallel to the Y-axis
            int nCols = (int)((endx - startx) / StripeWidth);


            gl.Begin(OpenGL.GL_QUADS);
            //loop over stripes
            for (int i = 0; i < nCols; i++)
            {
                //even columns will have Color1
                if ((i % 2 == 0))
                    gl.Color(ColorStripe1.R, ColorStripe1.G, ColorStripe1.B);
                else
                    gl.Color(ColorStripe2.R, ColorStripe2.G, ColorStripe2.B);
                //draw stripe
                gl.Vertex(i * StripeWidth + startx, starty);
                gl.Vertex(i * StripeWidth + startx, endy);
                gl.Vertex((i + 1) * StripeWidth + startx, endy);
                gl.Vertex((i + 1) * StripeWidth + startx, starty);
            }

            gl.End();
        }

        #endregion


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
