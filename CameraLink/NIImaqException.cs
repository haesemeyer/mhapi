using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MHApi.CameraLink
{
    public class NIImaqException : ApplicationException
    {
        public NIImaqException() : base() { }

        public NIImaqException(string message) : base(message) { }
    }
}
