////////
//Written by Drew Robson (with modifications)
///////


using System;
using System.Runtime.InteropServices;
using ipp;

namespace MHApi.DrewsClasses {

    /// <summary>
    /// Class to encapsulate an 8bit image in raw memory
    /// </summary>
    public unsafe class Image8 : IDisposable {
        
        /// <summary>
        /// Pointer to the image in memory
        /// </summary>
        public byte* Image;
        /// <summary>
        /// Image Width
        /// </summary>
        public int Width
        {
            get
            {
                return Size.width;
            }
        }

        /// <summary>
        /// Image height
        /// </summary>
        public int Height
        {
            get
            {
                return Size.height;
            }
        }

        /// <summary>
        /// Image stride (memory layout)
        /// </summary>
        public int Stride;

        /// <summary>
        /// Pointer to a given pixel in the image buffer
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel</param>
        /// <param name="y">The y-coordinate of the pixel</param>
        /// <returns>Pointer to the pixel in the buffer</returns>
        public byte* this[int x, int y]
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Image8");
                if (x >= Width || y >= Height)
                {
                    throw new IndexOutOfRangeException("The indexed point lies outside of the image");
                }
                return Image+x + y * Stride;
            }
        }

        /// <summary>
        /// Pointer to a given pixel in the image buffer
        /// </summary>
        /// <param name="point">The point referencing the pixel</param>
        /// <returns>Pointer to the pixel in the buffer</returns>
        public byte* this[IppiPoint point]
        {
            get
            {
                if (IsDisposed)
                    throw new ObjectDisposedException("Image8");
                if (point.x >= Width || point.y >= Height)
                {
                    throw new IndexOutOfRangeException("The indexed point lies outside of the image");
                }
                return Image + point.x + point.y * Stride;
            }
        }

        /// <summary>
        /// Imagesize
        /// </summary>
        public IppiSize Size;

        public Image8(int width, int height) {
            Stride = (int)(4 * Math.Ceiling(width / 4.0));
            Size = new IppiSize(width, height);
            Image = (byte*)Marshal.AllocHGlobal(Stride * height).ToPointer();
        }

        public Image8(IppiSize imageSize) : this(imageSize.width, imageSize.height) { }

        #region IDisposable Members

        public bool IsDisposed { get; private set; }


        public void Dispose() {
            if (IsDisposed)
                return;
            Dispose(true);
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            Marshal.FreeHGlobal((IntPtr)Image);
        }

        ~Image8() {
            Dispose(false);
        }

        #endregion
    }
}
