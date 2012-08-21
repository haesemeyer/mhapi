using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;

using SharpGL;


namespace MHApi.OpenGLSupport
{
    /// <summary>
    /// Interface that all opengl drawing providers
    /// need to implement
    /// </summary>
    public interface IOpenGLDrawingProvider
    {
        /// <summary>
        /// Draws the provided objects on screen
        /// </summary>
        /// <param name="gl">The opengl context used for drawing</param>
        void Draw(OpenGL gl);

        /// <summary>
        /// Sets up the projection matrix.
        /// Should be called when the render context resizes
        /// </summary>
        /// <param name="gl">The opengl context used</param>
        void Resized(OpenGL gl);

        /// <summary>
        /// Performs initialization necessary for the provider
        /// </summary>
        /// <param name="gl">The opengl context used</param>
        void Initialize(OpenGL gl);

        /// <summary>
        /// The requested translation in X-direction
        /// </summary>
        float TranslationX { get; set; }

        /// <summary>
        /// The requested translation in Y-direction
        /// </summary>
        float TranslationY { get; set; }

        /// <summary>
        /// The requested rotation around Z
        /// </summary>
        float RotationZ { get; set; }

        /// <summary>
        /// The requested rotation around Y
        /// </summary>
        float RotationY { get; set; }

        /// <summary>
        /// The requested rotation around X
        /// </summary>
        float RotationX { get; set; }

        /// <summary>
        /// Converts the provider data to a savable filestring
        /// </summary>
        /// <returns>A string representation of the provider</returns>
        string ToFileString();

        /// <summary>
        /// Populates a drawing provider from a string representation
        /// </summary>
        /// <param name="s">The string representation</param>
        void FromFileString(string s);
    }
}
