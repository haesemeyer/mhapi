using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using ipp;

namespace MHApi.Analysis
{
    /// <summary>
    /// Offers a function to smooth coordinate traces
    /// and contains all necessary internal buffers
    /// </summary>
    public unsafe class CoordinateSmoother : IDisposable
    {
        /// <summary>
        /// Internal buffer for data processing
        /// including border necessary for filtering
        /// </summary>
        CentroidBuffer _calc1;

        /// <summary>
        /// Internal buffer for data processing
        /// including border necessary for filtering
        /// </summary>
        CentroidBuffer _calc2;

        /// <summary>
        /// The filter kernel
        /// </summary>
        float* _kernel;

        /// <summary>
        /// The size of the allocated kernel
        /// </summary>
        int _kernelSize;

        public CoordinateSmoother(){}

        /// <summary>
        /// Pre-allocates the internal buffer to the requested size
        /// </summary>
        /// <param name="nCoordinates">The number of coordinates for which space should be pre-allocated in the internal buffers</param>
        public CoordinateSmoother(int nCoordinates, int windowSize)
        {
            _calc1 = new CentroidBuffer(nCoordinates + (int)Math.Ceiling(windowSize / 2.0) * 2);
            _calc2 = new CentroidBuffer(_calc1.Size.width);
            _kernelSize = windowSize;
            //preinit kernel
            _kernel = (float*)Marshal.AllocHGlobal(_kernelSize * 4);
            int i = 0;
            while (i < _kernelSize)
                _kernel[i++] = 1 / (float)_kernelSize;
        }

        public void SmoothenTrack(CentroidBuffer srcDest, int windowSize)
        {
            if (IsDisposed)
                throw new ObjectDisposedException(this.ToString());
            //For the internal buffers we require a size that fits both the coordinate buffer we
            //intend to filter as well as the border pixels required for filtering
            int borderSize = (int)Math.Ceiling(windowSize / 2.0);
            int reqSize = srcDest.Size.width + borderSize * 2;
            //Adjust internal buffers if necessary
            if (_calc1 == null)
            {
                _calc1 = new CentroidBuffer(reqSize);
                _calc2 = new CentroidBuffer(reqSize);
            }
            else if (_calc1.Size.width != reqSize)
            {
                _calc1.Dispose();
                _calc2.Dispose();
                _calc1 = new CentroidBuffer(reqSize);
                _calc2 = new CentroidBuffer(reqSize);
            }
            if (_kernel == null)
            {
                _kernelSize = windowSize;
                _kernel = (float*)Marshal.AllocHGlobal(_kernelSize * 4);
                int i = 0;
                while (i < _kernelSize)
                    _kernel[i++] = 1 / (float)_kernelSize;
            }
            else if (_kernelSize != windowSize)
            {
                Marshal.FreeHGlobal((IntPtr)_kernel);
                _kernelSize = windowSize;
                _kernel = (float*)Marshal.AllocHGlobal(_kernelSize * 4);
                int i = 0;
                while (i < _kernelSize)
                    _kernel[i++] = 1 / (float)_kernelSize;
            }
            //filter parameters
            IppiSize regionSize = new IppiSize(srcDest.Size.width,1);
            IppiPoint anchor = new IppiPoint(borderSize,0);
            IppiSize kernelSize = new IppiSize(_kernelSize, 1);
            //Copy src buffer adding borders
            ip.ippiCopyConstBorder_32f_C1R(srcDest.Buffer, srcDest.Stride, srcDest.Size, _calc1.Buffer, _calc1.Stride, _calc1.Size, 0, borderSize, 0);
            //Fill calc2 to have borders ready after filtering
            ip.ippiSet_32f_C1R(0, _calc2.Buffer, _calc2.Stride, _calc2.Size);
            //filter x-coordinates with our kernel
            ip.ippiFilter_32f_C1R(_calc1.Buffer+borderSize*4,_calc1.Stride,_calc2.Buffer+4*borderSize,_calc2.Stride,regionSize,_kernel,kernelSize,anchor);
            //filter y-coordinates with our kernel
            ip.ippiFilter_32f_C1R(_calc1.Buffer + borderSize * 4 + _calc1.Stride, _calc1.Stride, _calc2.Buffer + 4 * borderSize + _calc2.Stride, _calc2.Stride, regionSize, _kernel, kernelSize, anchor);
            //invert image - mirror on vertical axis
            ip.ippiMirror_32f_C1R(_calc2.Buffer, _calc2.Stride, _calc1.Buffer, _calc1.Stride, _calc2.Size, IppiAxis.ippAxsVertical);
            //filter x-coordinates with our kernel - now on inverted image
            ip.ippiFilter_32f_C1R(_calc1.Buffer + borderSize * 4, _calc1.Stride, _calc2.Buffer + 4 * borderSize, _calc2.Stride, regionSize, _kernel, kernelSize, anchor);
            //filter y-coordinates with our kernel - now on inverted image
            ip.ippiFilter_32f_C1R(_calc1.Buffer + borderSize * 4 + _calc1.Stride, _calc1.Stride, _calc2.Buffer + 4 * borderSize + _calc2.Stride, _calc2.Stride, regionSize, _kernel, kernelSize, anchor);
            //flip image back
            ip.ippiMirror_32f_C1R(_calc2.Buffer, _calc2.Stride, _calc1.Buffer, _calc1.Stride, _calc2.Size, IppiAxis.ippAxsVertical);
            //copy to src-dest
            ip.ippiCopy_32f_C1R(_calc1.Buffer + borderSize * 4, _calc1.Stride, srcDest.Buffer, srcDest.Stride, srcDest.Size);
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
            if (_kernel != null)
            {
                Marshal.FreeHGlobal((IntPtr)_kernel);
                _kernel = null;
            }
            IsDisposed = true;
        }

        ~CoordinateSmoother() {
            Dispose();
        }
        #endregion
    }
}
