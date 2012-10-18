using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;


using ipp;

using MHApi.DrewsClasses;

namespace MHApi.Imaging
{
    /// <summary>
    /// Represents a 16 bit image
    /// </summary>
    public unsafe class Image16 : IDisposable
    {
        public ushort* Image;

        public readonly int Width, Height, Stride;
        public readonly IppiSize Size;

        //Construct image with 1 channel and 16bit for each pixel from scratch
        public Image16(int width, int height) {
            Width = width;
            Height = height;
            Size = new IppiSize(width,height);
            Stride = (int)(4 * Math.Ceiling(width * 2 / 4.0));
            Image = (ushort*)Marshal.AllocHGlobal(Stride * height).ToPointer();
        }

        //Construct 16-bit image using an 8-bit unsigned image as input
        public Image16(Image8 im) {
            Width = im.Width;
            Height = im.Height;
            Size = new IppiSize(Width,Height);
            Stride = (int)(4 * Math.Ceiling(Width * 2 / 4.0));
            Image = (ushort*)Marshal.AllocHGlobal(Stride * Height).ToPointer();
            //convert and copy image
            IppHelper.IppCheckCall(ip.ippiConvert_8u16u_C1R(im.Image, im.Stride, Image, Stride, Size));
        }

        /// <summary>
        /// Reduce image to 8bit representation
        /// </summary>
        /// <param name="im"></param>
        public void ReduceTo8U(Image8 im) {
            System.Diagnostics.Debug.Assert(im.Width==Width);
            System.Diagnostics.Debug.Assert(im.Height == Height);
            IppHelper.IppCheckCall(ip.ippiConvert_16u8u_C1R(Image, Stride, im.Image, im.Stride, Size));
        }

        /// <summary>
        /// Pointer to a given pixel in the image buffer
        /// </summary>
        /// <param name="x">The x-coordinate of the pixel</param>
        /// <param name="y">The y-coordinate of the pixel</param>
        /// <returns>Pointer to the pixel in the buffer</returns>
        public ushort* this[int x, int y]
        {
            get
            {
                if (x >= Width || y >= Height)
                {
                    throw new IndexOutOfRangeException("The indexed point lies outside of the image");
                }
                return (ushort*)((byte*)Image + x*2 + y * Stride);
            }
        }

        /// <summary>
        /// Pointer to a given pixel in the image buffer
        /// </summary>
        /// <param name="point">The point referencing the pixel</param>
        /// <returns>Pointer to the pixel in the buffer</returns>
        public ushort* this[IppiPoint point]
        {
            get
            {
                if (point.x >= Width || point.y >= Height)
                {
                    throw new IndexOutOfRangeException("The indexed point lies outside of the image");
                }
                return (ushort*)((byte*)Image + point.x*2 + point.y * Stride);
            }
        }
        
        #region IDisposable Members
        bool isDisposed;
        public void Dispose() {
            if (isDisposed) return;
            Marshal.FreeHGlobal((IntPtr)Image);
            isDisposed = true;
        }

        ~Image16() {
            Dispose();
        }
        #endregion
    }
}
