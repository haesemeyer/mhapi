using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MHApi.PIDController
{
    /// <summary>
    /// Implementation of a temperature probe
    /// repressentation with external update of
    /// the temperature
    /// </summary>
    public class DummyTempSensor : Sensor
    {
        public double Temperature
        {
            get
            {
                return Value;
            }
            set
            {
                Value = value;
            }
        }
    }
}
