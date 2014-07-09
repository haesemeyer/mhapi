using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL;
using SharpGL.Enumerations;

namespace MHApi.OpenGLSupport
{
    public class PatternTrainProvider : TwoDProviderBase
    {

        /// <summary>
        /// The color displayed in the illuminated checkers
        /// (and half intensity is displayed on other side)
        /// </summary>
        FColor _colOn;

        /// <summary>
        /// The width of the checkers in device units
        /// </summary>
        float _checkWidth;

        /// <summary>
        /// The width of the checkers in device units
        /// </summary>
        public float CheckerWidth
        {
            get
            {
                return _checkWidth;
            }
            set
            {
                _checkWidth = value;
            }
        }

        /// <summary>
        /// The height of the checkers in device units
        /// </summary>
        float _checkHeight;

        /// <summary>
        /// The height of the checkers in device units
        /// </summary>
        public float CheckerHeight
        {
            get
            {
                return _checkHeight;
            }
            set
            {
                _checkHeight = value;
            }
        }

        /// <summary>
        /// The width of the middle strip in device units
        /// </summary>
        float _midStripeWidth;

        /// <summary>
        /// The width of the middle strip in device units
        /// </summary>
        public float MiddleStripeWidth
        {
            get
            {
                return _midStripeWidth;
            }
            set
            {
                _midStripeWidth = value;
            }
        }

        /// <summary>
        /// Creates a new pattern train provider
        /// </summary>
        /// <param name="colorOn">The on-color of the checkers - solid colored half will have ~half luminance and ~same color</param>
        /// <param name="checkerWidth">The width of each checker</param>
        /// <param name="checkerHeight">The height of each checker</param>
        /// <param name="midStripeWidth">The width of the middle alignment strip</param>
        public PatternTrainProvider(FColor colorOn, float checkerWidth, float checkerHeight, float midStripeWidth)
        {
            _colOn = colorOn;
            _checkWidth = checkerWidth;
            _checkHeight = checkerHeight;
            _midStripeWidth = midStripeWidth;
        }

        public override void Draw(OpenGL gl)
        {
            //Process alignment in base classs
            base.Draw(gl);

            //Calculate checkerboard on right half of screen
            //we draw our checkerboard larger than the viewport
            float startx = 0;
            float endx = 1.5f * ViewportHalfSizeX;
            float starty = -1.5f * ViewportHalfSizeY;
            float endy = -1 * starty;

            int nCols = (int)((endx - startx) / _checkWidth);
            int nRows = (int)((endy - starty) / _checkHeight);

            gl.Begin(OpenGL.GL_QUADS);

            //loop over checkers
            for (int i = 0; i < nCols; i++)
                for (int j = 0; j < nRows; j++)
                {
                    //in even rows, even columns will have foreground color
                    if ((i % 2 == 0) ^ (j % 2 == 1))
                        gl.Color(_colOn.R, _colOn.G, _colOn.B);
                    else
                        gl.Color(0f, 0f, 0f);
                    //draw checker
                    gl.Vertex(i * CheckerWidth + startx, j * CheckerHeight + starty);
                    gl.Vertex(i * CheckerWidth + startx, (j + 1) * CheckerHeight + starty);
                    gl.Vertex((i + 1) * CheckerWidth + startx, (j + 1) * CheckerHeight + starty);
                    gl.Vertex((i + 1) * CheckerWidth + startx, j * CheckerHeight + starty);
                }


            //Draw the left half of screen
            //in same color as checkers but half
            //luminance (NOTE: HALFING RGB VALUES DOES AT BEST APPROXIMATE THIS!!!) 
            //first square - top left
            gl.Color(_colOn.R / 2, _colOn.G / 2, _colOn.B / 2);
            gl.Vertex(-15f, -15f);
            gl.Vertex(-15f, 15f);
            gl.Vertex(0f, 15f);
            gl.Vertex(0f, -15f);

            //Draw middle strip on top - hopefully hiding the rest
            gl.Color(0f, 0f, 0f);
            gl.Vertex(-1 * MiddleStripeWidth / 2, -15f);
            gl.Vertex(-1 * MiddleStripeWidth / 2, 15f);
            gl.Vertex(MiddleStripeWidth / 2, 15f);
            gl.Vertex(MiddleStripeWidth / 2, -15f);


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
