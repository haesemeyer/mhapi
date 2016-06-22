﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Diagnostics;

namespace MHApi.Utilities
{
    /// <summary>
    /// Helper class to time (repeating)
    /// sections of code by name
    /// </summary>
    public static class SimpleProfiler
    {
        private struct TimeData
        {
            /// <summary>
            /// Number of total elapsed ticks
            /// </summary>
            public double Elapsed;

            /// <summary>
            /// Number of measurement iterations
            /// </summary>
            public double Iterations;

            /// <summary>
            /// The time elapsed per iteration in
            /// milliseconds
            /// </summary>
            public double AverageInMilliseconds
            {
                get
                {
                    var avg = Elapsed / Iterations;
                    return avg / Stopwatch.Frequency * 1000;
                }
            }

            /// <summary>
            /// The total time elapsed in seconds
            /// </summary>
            public double TotalInSeconds
            {
                get
                {
                    return Elapsed / Stopwatch.Frequency;
                }
            }
        }

        /// <summary>
        /// Dictionary relating measurement names to measurements
        /// </summary>
        private static Dictionary<string, TimeData> _timings = new Dictionary<string, TimeData>();

        /// <summary>
        /// Dictionary to keep track of tick-counts at the start of a new measurement
        /// </summary>
        private static Dictionary<string, long> _startTicks = new Dictionary<string, long>();

        /// <summary>
        /// Starts a new named measurement
        /// </summary>
        /// <param name="name">The name of the measurement</param>
        [Conditional("DEBUG")]
        public static void StartNextMeasurement(string name)
        {
            _startTicks[name] = Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// End a previously started measurement and add
        /// the elapsed time
        /// </summary>
        /// <param name="name">The name of the measurement</param>
        [Conditional("DEBUG")]
        public static void EndNextMeasurement(string name)
        {
            var end = Stopwatch.GetTimestamp();
            if (_startTicks.ContainsKey(name))
            {
                if (_timings.ContainsKey(name))
                {
                    var t = _timings[name];
                    t.Iterations++;
                    t.Elapsed += end - _startTicks[name];
                    _timings[name] = t;
                }
                else
                {
                    var t = new TimeData();
                    t.Iterations = 1;
                    t.Elapsed = end - _startTicks[name];
                    _timings[name] = t;
                }
            }
            else
                Debug.WriteLine("Can't end measurement that hasn't been started. Ignored call.", name);
        }

        /// <summary>
        /// Writes the number elapsed milliseconds per iteration
        /// to the debug console
        /// </summary>
        /// <param name="name">The name of the measurement</param>
        [Conditional("DEBUG")]
        public static void ReportAverageMilliseconds(string name)
        {
            if (_timings.ContainsKey(name))
            {
                Debug.WriteLine("Each iteration of {0} took {1} ms.", name, _timings[name].AverageInMilliseconds);
            }
            else
                Debug.WriteLine("No measurement with this name exists.", name);
        }

        /// <summary>
        /// Writes the total number of elapsed seconds
        /// to the debug console
        /// </summary>
        /// <param name="name">The name of the measurement</param>
        [Conditional("DEBUG")]
        public static void ReportTotalSeconds(string name)
        {
            if (_timings.ContainsKey(name))
            {
                Debug.WriteLine("Total time taken by {0} was {1} seconds.", name, _timings[name].TotalInSeconds);
            }
            else
                Debug.WriteLine("No measurement with this name exists.", name);
        }

        /// <summary>
        /// Resets the named measurement
        /// </summary>
        /// <param name="name">The name of the measurement</param>
        [Conditional("DEBUG")]
        public static void Reset(string name)
        {
            if (_timings.ContainsKey(name))
            {
                _timings[name] = new TimeData();
            }
            else
                Debug.WriteLine("No measurement with this name exists.", name);
        }

    }
}