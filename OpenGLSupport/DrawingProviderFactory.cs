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
            switch (providerElements[1])
            {
                case "OneColor":
                    retval = new OneColorProvider();
                    retval.FromFileString(providerFileString);
                    return retval;
                case "FourSquare":
                    retval = new FourSquareProvider();
                    retval.FromFileString(providerFileString);
                    return retval;
                default:
                    throw new ApplicationException("Could not recognize drawing provider");
            }
        }
    }
}
