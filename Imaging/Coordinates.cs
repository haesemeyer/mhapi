using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ipp;

namespace MHApi.Imaging
{
    /// <summary>
    /// Coordinate conversions
    /// </summary>
    public static class Coordinates
    {
        public enum Rotation { None, Clock90, Clock180, CounterClock90 };

        /// <summary>
        /// Rotates a coordinate within a given reference frame
        /// </summary>
        /// <param name="original">The original coordinate</param>
        /// <param name="size">The dimensions of the reference frame</param>
        /// <param name="rot">The desired rotation</param>
        /// <returns>The rotated coordinate</returns>
        public static IppiPoint Rotate(IppiPoint original, IppiSize size, Rotation rot)
        {
            switch (rot)
            {
                case Rotation.None:
                    return original;
                case Rotation.Clock90:
                    return new IppiPoint(size.height - original.y, original.x);
                case Rotation.Clock180:
                    return new IppiPoint(size.width - original.x, size.height - original.y);
                default:
                    return new IppiPoint(original.y, size.width - original.x);
            }
        }
    }
}
