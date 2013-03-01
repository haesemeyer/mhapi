using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

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
        public Bout? CompletedBout;

        /// <summary>
        /// The original coordinate passed
        /// to the analysis
        /// </summary>
        public IppiPoint OriginalCoordinate;

        /// <summary>
        /// The track-smoothened coordinate
        /// </summary>
        public IppiPoint_32f SmoothenedCoordinate;

        /// <summary>
        /// The current instant speed in 
        /// pixels/second
        /// </summary>
        public float InstantSpeed;

        public PointAnalysis(Bout bout, IppiPoint coordinate, IppiPoint_32f smoothenedCoordinate, float instantSpeed)
        {
            CompletedBout = bout;
            OriginalCoordinate = coordinate;
            SmoothenedCoordinate = smoothenedCoordinate;
            InstantSpeed = instantSpeed;
        }

        public PointAnalysis(IppiPoint coordinate, IppiPoint_32f smoothenedCoordinate, float instantSpeed)
        {
            CompletedBout = null;
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

        /// <summary>
        /// The filter taps
        /// </summary>
        float* _taps;

        /// <summary>
        /// Initializer for our delay lines
        /// (will be 0)
        /// </summary>
        float* _dlyLines;

        /// <summary>
        /// The number of taps in our filter
        /// </summary>
        int _tapLength;

        /// <summary>
        /// Pointer to the filter state structure for our
        /// FIR filter of the x-coordinates
        /// </summary>
        IppsFIRState_32f* _filterStateX;

        /// <summary>
        /// The buffer in memory for the actual
        /// x-coordinate filter state structure
        /// </summary>
        byte* _stateMemX;

        /// <summary>
        /// Pointer to the filter state structure
        /// for our FIR filter of the y-coordinates
        /// </summary>
        IppsFIRState_32f* _filterStateY;

        /// <summary>
        /// The buffer in memory for the actual
        /// y-coordinate filter state structure
        /// </summary>
        byte* _stateMemY;

        /// <summary>
        /// The index of the current
        /// coordinate
        /// </summary>
        int _index;

        /// <summary>
        /// The last smoothened
        /// x-position (for speed calculation)
        /// </summary>
        float _lastX;

        /// <summary>
        /// The last smoothened y-position
        /// (for speed calculation)
        /// </summary>
        float _lastY;

        /// <summary>
        /// The acquisition framerate
        /// to relate displacement to speed
        /// </summary>
        int _frameRate;

        /// <summary>
        /// The absolute instant speed
        /// threshold for bout detection
        /// </summary>
        float _speedThreshold;

        /// <summary>
        /// The minimum number of consecutive frames
        /// above speed threshold required for a bout
        /// </summary>
        int _minFramesPerBout;

        /// <summary>
        /// The maximum number of frames at peakspeed
        /// allowed for a bout
        /// </summary>
        int _maxFramesAtPeak;

        #endregion

        #region Constructor

        public RealtimeMovementAnalyzer(int frameRate, int smoothingWindowSize, float speedThreshold, int minFramesPerBout, int maxFramesAtPeak)
        {
            _frameRate = frameRate;
            _tapLength = smoothingWindowSize;
            _speedThreshold = speedThreshold;
            _minFramesPerBout = minFramesPerBout;
            _maxFramesAtPeak = maxFramesAtPeak;
            _index = 0;
            _lastX = 0;
            _lastY = 0;
            //allocate memory for taps and delay lines
            _taps = (float*)Marshal.AllocHGlobal(_tapLength * 4);
            _dlyLines = (float*)Marshal.AllocHGlobal(_tapLength * 4);
            //set filter taps - boxcar filter
            int i = 0;
            while (i < _tapLength)
                _taps[i++] = 1 / (float)_tapLength;
            //set delay lines to 0
            sp.ippsZero_32f(_dlyLines, _tapLength);
            //get memory requirements and allocate state structures
            int size = 0;
            sp.ippsFIRGetStateSize_32f(_tapLength, &size);
            _stateMemX = (byte*)Marshal.AllocHGlobal(size);
            _stateMemY = (byte*)Marshal.AllocHGlobal(size);
            //initialize state structures
            fixed (IppsFIRState_32f** ppState = &_filterStateX)
            {
                sp.ippsFIRInit_32f(ppState, _taps, _tapLength, _dlyLines, _stateMemX);
            }
            fixed (IppsFIRState_32f** ppState = &_filterStateY)
            {
                sp.ippsFIRInit_32f(ppState, _taps, _tapLength, _dlyLines, _stateMemY);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Resets the state of the analyzer.
        /// Delay lines get reinitialized to reset the filter
        /// and bout calling is stopped.
        /// </summary>
        public void Reset()
        {
            //reset our coordinate index and our last positions
            _index = 0;
            _lastX = 0;
            _lastY = 0;
            //re-blank the delay lines of the FIR filters
            sp.ippsFIRSetDlyLine_32f(_filterStateX, _dlyLines);
            sp.ippsFIRSetDlyLine_32f(_filterStateY, _dlyLines);
        }

        /// <summary>
        /// Processes the next point, filtering its position
        /// computing speeds and detecting bouts
        /// </summary>
        /// <param name="coordinate">The coordinate to analyze</param>
        /// <returns>The analysis of the given coordinate</returns>
        public PointAnalysis ProcessNextPoint(IppiPoint coordinate)
        {
            //set up locals for processing
            float x = coordinate.x;
            float y = coordinate.y;
            float xSmooth, ySmooth, speed;
            xSmooth = ySmooth = speed = 0;
            //filter x-coordinate
            sp.ippsFIR_32f(&x, &xSmooth, 1, _filterStateX);
            //filter y-coordinate
            sp.ippsFIR_32f(&y, &ySmooth, 1, _filterStateY);
            //compute instant speed
            speed = (float)Math.Sqrt((xSmooth - _lastX) * (xSmooth - _lastX) + (ySmooth - _lastY) * (ySmooth - _lastY));
            speed *= 240;
            //update index and last coordinates
            _index++;
            _lastX = xSmooth;
            _lastY = ySmooth;
            //return our analysis
            return new PointAnalysis(coordinate, new IppiPoint_32f(xSmooth, ySmooth), speed);
        }

        #endregion


        #region IDisposable Members

        public bool IsDisposed {get; private set;}

        public void Dispose() {
            if (IsDisposed)
                return;
            if (_taps != null)
            {
                Marshal.FreeHGlobal((IntPtr)_taps);
                _taps = null;
            }
            if (_dlyLines != null)
            {
                Marshal.FreeHGlobal((IntPtr)_dlyLines);
                _dlyLines = null;
            }
            if (_stateMemX != null)
            {
                Marshal.FreeHGlobal((IntPtr)_stateMemX);
                _stateMemX = null;
            }
            if (_stateMemY != null)
            {
                Marshal.FreeHGlobal((IntPtr)_stateMemY);
                _stateMemY = null;
            }
            IsDisposed = true;
        }

        ~RealtimeMovementAnalyzer() {
            Dispose();
        }
        #endregion
    }
}
