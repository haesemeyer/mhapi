using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ipp;

namespace MHApi.Scanning
{
    public abstract class PointVoltageConverter
    {
        public abstract IppiPoint_32f this[IppiPoint p] { get; }

        public abstract bool Complete { get; protected set; }
    }
}
