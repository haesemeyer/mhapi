using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SharpGL;
using SharpGL.Enumerations;


namespace MHApi.OpenGLSupport
{

    /// <summary>
    /// Abstract base class for all 2D (x,y-plane) drawing providers
    /// </summary>
    public abstract class TwoDProviderBase : IOpenGLDrawingProvider
    {
        public abstract void Draw(OpenGL gl);

        public virtual void Resized(OpenGL gl)
        {
            gl.MatrixMode(MatrixMode.Projection);
            //Load identity matrix
            gl.LoadIdentity();
            //Set up orthogonal (2D) projection
            //use coordinate system independent of the actual window size
            //but constrained by the projectors aspect ratio!
            //with the origin in the middle (for rotation ease)
            //x pointing right, y pointing down
            gl.Ortho2D(-8.0f, 8.0f, 6.0f, -6.0f);

            //Load modelview
            gl.MatrixMode(MatrixMode.Modelview);
        }

        public virtual void Initialize(OpenGL gl)
        {
            //Disable depth testing since we only draw 2D
            gl.Disable(OpenGL.GL_DEPTH_TEST);
            //set the clear (=background) color - black
            gl.ClearColor(0, 0, 0, 0);
        }

        public virtual float TranslationX
        {
            get;
            set;
        }

        public virtual float TranslationY
        {
            get;
            set;
        }

        public virtual float RotationZ
        {
            get;
            set;
        }

        #region NotSupported

        public float RotationY
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public float RotationX
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        public abstract string ToFileString();

        public abstract void FromFileString(string s);
    }
}
