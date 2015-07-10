using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MHApi.RND
{
    /// <summary>
    /// Class to generate exponentially distributed
    /// random numbers by transform of uniform
    /// distribution
    /// </summary>
    public sealed class RandExp
    {
        /// <summary>
        /// Uniform random number generator used
        /// to generate the exponentially distributed numbers
        /// </summary>
        readonly Random _random;

        public RandExp(Random random = null)
        {
            _random = random ?? ThreadSafeRandom.ThisThreadsRandom;
        }

        /// <summary>
        /// Generates an exponentially distributed random number from
        /// an exponential distribution with the indicated scale beta
        /// mean = beta
        /// var = beta^2
        /// </summary>
        /// <param name="beta">The scale (=mean) of the distribution</param>
        /// <returns>Random number from the distribution</returns>
        public double NextExponential(double beta)
        {
            var u = _random.NextDouble();
            return -1 * beta * Math.Log(u);
        }
    }
}
