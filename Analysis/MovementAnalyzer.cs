using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ipp;

using MHApi.Imaging;

namespace MHApi.Analysis
{

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

        public InstantSpeedBuffer ComputeInstantSpeeds(CentroidBuffer path, int frameRate)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(this.ToString());
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
            //Multiply the distances by the framerate and store the result in a new instant speed buffer
            InstantSpeedBuffer retval = new InstantSpeedBuffer(path.Size.width);
            IppHelper.IppCheckCall(ip.ippiMulC_32f_C1R(_calc2.Buffer, _calc2.Stride, frameRate, retval.Buffer, retval.Stride, retval.Size));
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
