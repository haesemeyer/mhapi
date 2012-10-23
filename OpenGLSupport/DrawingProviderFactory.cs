using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MHApi.OpenGLSupport
{
    public static class DrawingProviderFactory
    {

        /// <summary>
        /// Creates the appropriate OpenGLDrawingProvider from a file string
        /// </summary>
        /// <param name="providerFileString">The file string to parse</param>
        /// <returns>The OpenGLDrawingProvider</returns>
        public static IOpenGLDrawingProvider InstantiateFromString(string providerFileString)
        {
            //The elements in the provider file-strings are separated by semicolons
            string[] providerElements = providerFileString.Split(';');
            if (providerElements.Length < 1)
                throw new ApplicationException("String does not represent an OpenGLDrawingProvider");
            else if (providerElements.Length == 1)
            {
                //this may be an empty provider!
                if (providerElements[0] == "?")
                    return null;
                else
                    throw new ApplicationException("String does not represent an OpenGLDrawingProvider");
            }
            //Check which provider we are dealing with, instantiate it and let it populate itself
            IOpenGLDrawingProvider retval;
            switch (providerElements[0])
            {
                case "OneColor":
                    retval = new OneColorProvider();
                    retval.FromFileString(providerFileString);
                    return retval;
                case "FourSquare":
                    retval = new FourSquareProvider();
                    retval.FromFileString(providerFileString);
                    return retval;
                case "Circle":
                    retval = new CircleProvider();
                    retval.FromFileString(providerFileString);
                    return retval;
                case "LinGrad":
                    retval = new LinGradientProvider();
                    retval.FromFileString(providerFileString);
                    return retval;
                default:
                    throw new ApplicationException("Could not recognize drawing provider");
            }
        }

        /// <summary>
        /// Allows to copy arbitrary (but known to the factory) OpenGL drawing providers
        /// Effectively implements generic copy constructor
        /// </summary>
        /// <param name="toCopy">The drawing provider we want to copy</param>
        /// <returns>A new drawing provider with the same values as the current provider</returns>
        public static IOpenGLDrawingProvider Copy(IOpenGLDrawingProvider toCopy)
        {
            if (toCopy == null)
                return null;
            else if (toCopy is OneColorProvider)
            {
                OneColorProvider tc = toCopy as OneColorProvider;
                OneColorProvider retval = new OneColorProvider(tc.Color);
                retval.AlignmentRotationZ = tc.AlignmentRotationZ;
                retval.AlignmentTranslationX = tc.AlignmentTranslationX;
                retval.AlignmentTranslationY = tc.AlignmentTranslationY;
                retval.ImageRotationZ = tc.ImageRotationZ;
                retval.ImageTranslationX = tc.ImageTranslationX;
                retval.ImageTranslationY = tc.ImageTranslationY;
                return retval;
            }
            else if (toCopy is FourSquareProvider)
            {
                FourSquareProvider tc = toCopy as FourSquareProvider;
                FourSquareProvider retval = new FourSquareProvider(tc.ColorSquare1, tc.ColorSquare2, tc.ColorSquare3, tc.ColorSquare4);
                retval.AlignmentRotationZ = tc.AlignmentRotationZ;
                retval.AlignmentTranslationX = tc.AlignmentTranslationX;
                retval.AlignmentTranslationY = tc.AlignmentTranslationY;
                retval.ImageRotationZ = tc.ImageRotationZ;
                retval.ImageTranslationX = tc.ImageTranslationX;
                retval.ImageTranslationY = tc.ImageTranslationY;
                return retval;
            }
            else if (toCopy is CircleProvider)
            {
                CircleProvider tc = toCopy as CircleProvider;
                CircleProvider retval = new CircleProvider(tc.Color, tc.Center, tc.Radius, tc.SegmentCount);
                retval.AlignmentRotationZ = tc.AlignmentRotationZ;
                retval.AlignmentTranslationX = tc.AlignmentTranslationX;
                retval.AlignmentTranslationY = tc.AlignmentTranslationY;
                retval.ImageRotationZ = tc.ImageRotationZ;
                retval.ImageTranslationX = tc.ImageTranslationX;
                retval.ImageTranslationY = tc.ImageTranslationY;
                return retval;
            }
            else if (toCopy is LinGradientProvider)
            {
                LinGradientProvider tc = toCopy as LinGradientProvider;
                LinGradientProvider retval = new LinGradientProvider(tc.Topleft, tc.Width, tc.Height, tc.ColStart, tc.ColEnd);
                retval.AlignmentRotationZ = tc.AlignmentRotationZ;
                retval.AlignmentTranslationX = tc.AlignmentTranslationX;
                retval.AlignmentTranslationY = tc.AlignmentTranslationY;
                retval.ImageRotationZ = tc.ImageRotationZ;
                retval.ImageTranslationX = tc.ImageTranslationX;
                retval.ImageTranslationY = tc.ImageTranslationY;
                return retval;
            }
            else
            {
                //we should never end up here
                System.Diagnostics.Debug.Assert(false, "Tried to copy unkown provider");
                return null;
            }
        }
    }
}
