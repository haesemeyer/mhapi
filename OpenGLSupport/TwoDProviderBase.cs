﻿/*
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
    /// Abstract base class for all 2D (x,y-plane) drawing providers
    /// </summary>
    public abstract class TwoDProviderBase : IOpenGLDrawingProvider
    {
        /// <summary>
        /// The viewport spans dimension from -Halfsize to +Halfsize
        /// with the origin in the center
        /// </summary>
        public const float ViewportHalfSizeX = 8.0f;

        /// <summary>
        /// The viewport spans dimension from -Halfsize to +Halfsize
        /// with the origin in the center
        /// </summary>
        public const float ViewportHalfSizeY = 6.0f;

        public virtual void Draw(OpenGL gl)
        {
            //move back to origin
            gl.LoadIdentity();
            //Rotate if necessary for alignment
            gl.Rotate(0, 0, AlignmentRotationZ);
            //translate if necessary for alignment
            gl.Translate(AlignmentTranslationX, AlignmentTranslationY, 0);
            //NON ALIGNMENT based translation should occur next, followed by non-alignment based translation...
            //Need to break down into alignment rotation and image rotation...
            gl.Translate(ImageTranslationX, ImageTranslationY, 0);
            gl.Rotate(0, 0, ImageRotationZ);
        }

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
            gl.Ortho2D(-1 * ViewportHalfSizeX, ViewportHalfSizeX, ViewportHalfSizeY, -1 * ViewportHalfSizeY);

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

        /// <summary>
        /// The requested alignment translation in X-direction
        /// </summary>
        public virtual float AlignmentTranslationX
        {
            get;
            set;
        }

        /// <summary>
        /// The requested translation of the
        /// image in X-direction
        /// </summary>
        public virtual float ImageTranslationX
        {
            get;
            set;
        }

        /// <summary>
        /// The requested alignment translation in Y-direction
        /// </summary>
        public virtual float AlignmentTranslationY
        {
            get;
            set;
        }

        /// <summary>
        /// The requested translation of the
        /// image in Y-direction
        /// </summary>
        public virtual float ImageTranslationY
        {
            get;
            set;
        }

        /// <summary>
        /// The requested alignment rotation around Z
        /// </summary>
        public virtual float AlignmentRotationZ
        {
            get;
            set;
        }

        /// <summary>
        /// The requested rotation of the image around Z
        /// </summary>
        public virtual float ImageRotationZ
        {
            get;
            set;
        }

        #region NotSupported

        public float AlignmentRotationY
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

        public float ImageRotationY
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

        public float AlignmentRotationX
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

        public float ImageRotationX
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
