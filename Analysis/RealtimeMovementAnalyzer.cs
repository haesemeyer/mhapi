using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ipp;

namespace MHApi.Analysis
{

    /// <summary>
    /// The return value of the realtime point processing
    /// Contains information about position, speed and any bout
    /// that has been completed at this time-point
    /// </summary>
    public struct PointAnalysis
    {
        /// <summary>
        /// The bout completed at this time
        /// or null if no bout was finished
        /// </summary>
        Bout? CompletedBout;

        /// <summary>
        /// The original coordinate passed
        /// to the analysis
        /// </summary>
        IppiPoint OriginalCoordinate;

        /// <summary>
        /// The track-smoothened coordinate
        /// </summary>
        IppiPoint_32f SmoothenedCoordinate;

        /// <summary>
        /// The current instant speed
        /// </summary>
        float InstantSpeed;

        public PointAnalysis(Bout bout, IppiPoint coordinate, IppiPoint_32f smoothenedCoordinate, float instantSpeed)
        {
            CompletedBout = bout;
            OriginalCoordinate = coordinate;
            SmoothenedCoordinate = smoothenedCoordinate;
            InstantSpeed = instantSpeed;
        }
    }

    /// <summary>
    /// Class to smoothen tracks, compute instant speeds
    /// and detect bouts in realtime.
    /// </summary>
    public unsafe class RealtimeMovementAnalyzer : IDisposable
    {
        #region Fields



        #endregion
    }
}
