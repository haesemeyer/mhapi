using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ipp;

namespace MHApi.Imaging
{
    /// <summary>
    /// Exception thrown if an Ipp call returns an error
    /// </summary>
    public class IppException : Exception
    {
        /// <summary>
        /// Exception constructor
        /// </summary>
        /// <param name="errorCode">The return code of an ipp function call</param>
        public IppException(IppStatus errorCode) : base(errorCode.ToString()) { }
        
    }

    public static class IppHelper
    {
        /// <summary>
        /// Checks an ipp function return value writes errors to the debug output
        /// </summary>
        /// <param name="retval">The function return value</param>
        /// <exception cref="IppException">Throws IppException if retval != IppStatus.ippStsNoErr</exception>
        public static void IppCheckCall(IppStatus retval)
        {
            if (retval != IppStatus.ippStsNoErr)
            {
                System.Diagnostics.Debug.WriteLine(retval);
            }
        }
    }
}
