using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NationalInstruments.DAQmx;

namespace MHApi.Laser
{

    /// <summary>
    /// Class to control a Shanghai Dream Laser
    /// SDLPS500 driver via a national instruments board 
    /// </summary>
    public class SDLPS500Controller : IDisposable
    {
        
        /// <summary>
        /// The control output
        /// to send to the laser
        /// </summary>
        double _controlOutput;

        /// <summary>
        /// The task linked to the analog out channel
        /// that controls the laser output
        /// </summary>
        Task _aoTask;

        /// <summary>
        /// The single channel writer for changing
        /// the laser output
        /// </summary>
        AnalogSingleChannelWriter _aoWriter;


        /// <summary>
        /// The control output to send to the laser.
        /// NOTE: This actually only properly works if the front dial is set to 10 (max)!
        /// </summary>
        public double ControlOutput
        {
            get
            {
                return _controlOutput;
            }
            set
            {
                if (IsDisposed)
                    throw new ObjectDisposedException(this.ToString());
                if (value < 0 || value > 5)
                    throw new ArgumentOutOfRangeException("The control output has to be between 0 and 5V");
                if (value != _controlOutput)
                {
                    _controlOutput = value;
                    _aoWriter.WriteSingleSample(true, _controlOutput);
                }
            }
        }


        public SDLPS500Controller(string device, string aoName)
        {
            _controlOutput = 0;
            //Set up task and channel writer
            _aoTask = new Task("LaserOutput");
            _aoTask.AOChannels.CreateVoltageChannel(device + "/" + aoName, "LasOut", 0, 5, AOVoltageUnits.Volts);
            _aoWriter = new AnalogSingleChannelWriter(_aoTask.Stream);
            //Set output to 0
            _aoWriter.WriteSingleSample(true, 0);
        }

        #region IDisposable

        public bool IsDisposed { get; private set; }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            if (_aoTask != null)
            {
                _aoWriter.WriteSingleSample(true, 0);
                _aoTask.Dispose();
                _aoTask = null;
            }
            IsDisposed = true;
        }

        #endregion
    }
}
