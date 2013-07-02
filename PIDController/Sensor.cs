using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MHApi.PIDController
{
    /// <summary>
    /// Implements a generic sensor for a
    /// PID controller
    /// </summary>
    public abstract class Sensor
    {
        /// <summary>
        /// The value sensed by the sensor
        /// </summary>
        double _value;

        /// <summary>
        /// The value currently sensed
        /// by the sensor
        /// </summary>
        protected virtual double Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                if (ValueChanged != null)
                    ValueChanged(_value);
            }
        }

        /// <summary>
        /// This event gets raised whenever the sensor
        /// value changes
        /// </summary>
        public Action<double> ValueChanged = delegate { };
    }
}
