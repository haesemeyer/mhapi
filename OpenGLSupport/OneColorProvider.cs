using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.Enumerations;

namespace MHApi.OpenGLSupport
{
    public class OneColorProvider : IOpenGLDrawingProvider
    {

        public FColor Color { get; set; }

        /// <summary>
        /// Default constructor - Black color
        /// </summary>
        public OneColorProvider()
        {
            Color = FColor.Black;
        }

        public OneColorProvider(FColor color)
        {
            Color = color;
        }

        public void Draw(SharpGL.OpenGL gl)
        {
            //move back to origin
            gl.LoadIdentity();

            //Draw the square on screen
            gl.Begin(OpenGL.GL_QUADS);
            gl.Color(Color.R, Color.G, Color.B);
            gl.Vertex(-15f, -15f);
            gl.Vertex(-15f, 15f);
            gl.Vertex(15f, 15f);
            gl.Vertex(15f, -15f);
            gl.End();
            gl.Flush();
        }

        public void Resized(SharpGL.OpenGL gl)
        {
            gl.MatrixMode(MatrixMode.Projection);
            //Load identity matrix
            gl.LoadIdentity();
            //Set up orthogonal (2D) projection
            //use coordinate system independent of the actual window size
            //with the origin in the middle (for rotation ease)
            //x pointing right, y pointing down
            gl.Ortho2D(-10.0f, 10.0f, 10.0f, -10.0f);

            //Load modelview
            gl.MatrixMode(MatrixMode.Modelview);
        }

        public void Initialize(SharpGL.OpenGL gl)
        {
            //Disable depth testing since we only draw 2D
            gl.Disable(OpenGL.GL_DEPTH_TEST);
            //set the clear (=background) color - black
            gl.ClearColor(0, 0, 0, 0);
        }

        public float TranslationX
        {
            get;
            set;
        }

        public float TranslationY
        {
            get;
            set;
        }

        public float RotationZ
        {
            get;
            set;
        }

        #region NotSupported

        public float RotationY
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        public float RotationX
        {
            get
            {
                throw new NotSupportedException();
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        #endregion

        /// <summary>
        /// Converts the provider data to a savable filestring
        /// </summary>
        /// <returns>A string representation of the provider</returns>
        public string ToFileString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("OneColor");
            b.Append(";");
            b.Append(Color.R);
            b.Append(";");
            b.Append(Color.G);
            b.Append(";");
            b.Append(Color.B);
            return b.ToString();
        }

        /// <summary>
        /// User friendlier string representation
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("OneColor:");
            var c = Color.ToWPFColor();
            b.Append(string.Format("S1({0};{1};{2})", c.R, c.G, c.B));
            return b.ToString();
        }

        /// <summary>
        /// Populates a drawing provider from a string representation
        /// </summary>
        /// <param name="s">The string representation</param>
        public void FromFileString(string s)
        {
            string[] parts = s.Split(';');
            if (parts.Length != 4 || parts[0]!="OneColor")
                throw new ApplicationException("Provided string does not represent a OneColorProvider");
            Color = new FColor(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3]));
        }
    }
}
