using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ipp;

using MHApi.Imaging;

namespace MHApi.Analysis
{
    /// <summary>
    /// Structure to describe a bout
    /// </summary>
    public struct Bout
    {
        /// <summary>
        /// The first frame of the bout
        /// </summary>
        public int BoutStart { get; set; }

        /// <summary>
        /// The frame with the peak speed of the bout
        /// </summary>
        public int BoutPeak { get; set; }

        /// <summary>
        /// The last frame of the bout
        /// </summary>
        public int BoutEnd { get; set; }

        /// <summary>
        /// The total displacement in pixels during the bout
        /// </summary>
        public float TotalDisplacement { get; set; }

        /// <summary>
        /// The peak speed in pixels/second reached during the bout
        /// </summary>
        public float PeakSpeed { get; set; }

        /// <summary>
        /// Converts the bout structure to a string representation
        /// </summary>
        /// <returns>A tab-delimited string of the bout characteristics</returns>
        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}\t{3}\t{4}", BoutStart, BoutPeak, BoutEnd, TotalDisplacement, PeakSpeed);
        }
    }

    /// <summary>
    /// This class provided functionality to calculate instant
    /// speeds and detect bouts
    /// </summary>
    public unsafe class MovementAnalyzer : IDisposable
    {
        /// <summary>
        /// Internal buffer for data processing
        /// </summary>
        CentroidBuffer _calc1;

        /// <summary>
        /// Internal buffer for data processing
        /// </summary>
        CentroidBuffer _calc2;

        public MovementAnalyzer() { }

        public MovementAnalyzer(int nFrames)
        {
            //Pre-allocate and blank buffers
            _calc1 = new CentroidBuffer(nFrames);
            _calc2 = new CentroidBuffer(nFrames);
            ip.ippiSet_32f_C1R(0, _calc1.Buffer, _calc1.Stride, _calc1.Size);
            ip.ippiSet_32f_C1R(0, _calc2.Buffer, _calc2.Stride, _calc2.Size);
        }

        /// <summary>
        /// Computes instants speeds from a coordinate trace and stores the result
        /// in the provided buffer
        /// </summary>
        /// <param name="path">The movement path</param>
        /// <param name="frameRate">The framerate at acquisition</param>
        /// <param name="isb">The buffer to store the speed in</param>
        public void ComputeInstantSpeeds(CentroidBuffer path, int frameRate, InstantSpeedBuffer isb)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(this.ToString());
            if (isb.Size.width != path.Size.width)
                throw new ArgumentException("Path length and size of speed buffer must be the same!");
            //Check that our buffers are present and match, otherwise fix
            if (_calc1 == null)
            {
                _calc1 = new CentroidBuffer(path.Size.width);
                _calc2 = new CentroidBuffer(path.Size.width);
            }
            else if (_calc1.Size.width != path.Size.width)
            {
                _calc1.Dispose();
                _calc2.Dispose();
                _calc1 = new CentroidBuffer(path.Size.width);
                _calc2 = new CentroidBuffer(path.Size.width);
            }
            //compute difference trace subtracting the position at t from the position at t+1 by subtracting the unshifted input buffer from its shifted version
            //the result gets stored in calc1 with the first value being 0
            ip.ippiSet_32f_C1R(0, _calc1.Buffer, _calc1.Stride, _calc1.Size);
            IppHelper.IppCheckCall(ip.ippiSub_32f_C1R(path.Buffer, path.Stride, (float*)((byte*)path.Buffer + 4), path.Stride, (float*)((byte*)_calc1.Buffer + 4), _calc1.Stride, new IppiSize(path.Size.width - 1, 2)));
            //square the difference trace, storing the result in _calc2
            IppHelper.IppCheckCall(ip.ippiSqr_32f_C1R(_calc1.Buffer, _calc1.Stride, _calc2.Buffer, _calc2.Stride, _calc1.Size));
            //Add the values of the x and y coordinates, storing the sum of squares in the first row of _calc1
            IppHelper.IppCheckCall(ip.ippiAdd_32f_C1R(_calc2.Buffer, _calc2.Stride, (float*)((byte*)_calc2.Buffer + _calc2.Stride), _calc2.Stride, _calc1.Buffer, _calc1.Stride, new IppiSize(_calc1.Size.width, 1)));
            //Compute the square root of the sum-of-squares and copy the result to the first row of _calc2
            IppHelper.IppCheckCall(ip.ippiSqrt_32f_C1R(_calc1.Buffer, _calc1.Stride, _calc2.Buffer, _calc2.Stride, new IppiSize(_calc1.Size.width, 1)));
            //Multiply the distances by the framerate and store the result in our instant speed buffer
            IppHelper.IppCheckCall(ip.ippiMulC_32f_C1R(_calc2.Buffer, _calc2.Stride, frameRate, isb.Buffer, isb.Stride, isb.Size));
        }

        /// <summary>
        /// Computes instant speeds from a coordinate trace and returns a
        /// new instant speed buffer with the results
        /// </summary>
        /// <param name="path">The movement path</param>
        /// <param name="frameRate">The acquisition framerate</param>
        /// <returns>A new instant speed buffer with the speed results</returns>
        public InstantSpeedBuffer ComputeInstantSpeeds(CentroidBuffer path, int frameRate)
        {
            InstantSpeedBuffer retval = new InstantSpeedBuffer(path.Size.width);
            ComputeInstantSpeeds(path, frameRate, retval);
            return retval;
        }

        /// <summary>
        /// Extracts the bouts from an instant speed trace
        /// </summary>
        /// <param name="isb">The instant speed trace</param>
        /// <param name="speedThreshold">Speeds below this threshold will be set to 0</param>
        /// <param name="minFramesPerBout">For a movement to be called a bout we require at least this number of frames</param>
        /// <param name="maxFramesAtPeak">Bouts should have clearly defined peaks. Bouts with a peak longer than this number get rejected</param>
        /// <param name="frameRate">The framerate used for acquisition</param>
        /// <returns>A queue of bout structures describing every bout</returns>
        public Queue<Bout> DetectBouts(InstantSpeedBuffer isb, float speedThreshold, int minFramesPerBout, int maxFramesAtPeak, int frameRate)
        {
            Queue<Bout> retval = new Queue<Bout>();

            int i = 0;

            //threshold the speed trace
            ip.ippiThreshold_LTVal_32f_C1IR(isb.Buffer, isb.Stride, isb.Size, speedThreshold, 0);

            while (i < isb.Size.width)
            {
                if (isb[i] == 0)
                {
                    i++;
                    continue;
                }
                else//found potential bout start since speed above threshold
                {
                    int peaklength = 0;
                    Bout b = new Bout();
                    b.BoutStart = i;
                    b.PeakSpeed = isb[i];
                    b.BoutPeak = i;
                    b.TotalDisplacement = isb[i] / frameRate;
                    //loop over following frames collecting all frames that belong to the bout
                    while (i < isb.Size.width && isb[i] > 0)
                    {
                        if (isb[i] > b.PeakSpeed)
                        {
                            b.PeakSpeed = isb[i];
                            b.BoutPeak = i;
                            peaklength = 0;//new peakspeed found, reset peaklength
                        }
                        else if (isb[i] == b.PeakSpeed)//another frame with the same speed as our peak
                            peaklength++;
                        b.TotalDisplacement += isb[i] / frameRate;
                        i++;
                    }
                    b.BoutEnd = i - 1;
                    //check our bout criteria
                    if ((b.BoutEnd - b.BoutStart + 1) >= minFramesPerBout && peaklength <= maxFramesAtPeak)
                    {
                        //we have a valid bout
                        if (b.BoutStart == b.BoutPeak)
                            b.BoutStart--;
                        retval.Enqueue(b);
                    }
                }//found potential bout
            }
            return retval;
        }


        #region IDisposable Members

        public bool IsDisposed {get; private set;}

        public void Dispose() {
            if (IsDisposed)
                return;
            if (_calc1 != null)
            {
                _calc1.Dispose();
                _calc1 = null;
            }
            if (_calc2 != null)
            {
                _calc2.Dispose();
                _calc2 = null;
            }
            IsDisposed = true;
        }

        ~MovementAnalyzer() {
            Dispose();
        }
        #endregion

    }
}
