using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MHApi.DrewsClasses;
using ipp;

namespace MHApi.Imaging
{
    /// <summary>
    /// Provides High-level access to image thresholding
    /// and morphology operations
    /// </summary>
    public unsafe static class BWImageProcessor
    {
        /// <summary>
        /// Mask used for morphology operations defining
        /// both the neighborhood and its anchor for the operation
        /// </summary>
        public class MorphologyMask :IDisposable
        {
            /// <summary>
            /// The mask defining the neighborhood of the operations
            /// </summary>
            private Image8 _mask;

            /// <summary>
            /// The anchor used on the mask for the morphology operation
            /// </summary>
            private IppiPoint _anchor;

            /// <summary>
            /// The mask defining the neighborhood of the operations
            /// </summary>
            public Image8 Mask
            {
                get
                {
                    return _mask;
                }
            }

            /// <summary>
            /// The anchor used on the mask for the morphology operation
            /// </summary>
            public IppiPoint Anchor
            {
                get
                {
                    return _anchor;
                }
            }

            /// <summary>
            /// Creates a new morhpology mask
            /// </summary>
            /// <param name="mask">The mask image</param>
            /// <param name="anchor">The anchor</param>
            public MorphologyMask(Image8 mask, IppiPoint anchor)
            {
                if (mask == null)
                    throw new ArgumentNullException("mask");
                if (anchor.x >= mask.Width || anchor.y >= mask.Height || anchor.x < 0 || anchor.y < 0)
                {
                    throw new ArgumentOutOfRangeException("anchor", "Anchor point has to lie within the image");
                }
                _mask = mask;
                _anchor = anchor;
            }

            /// <summary>
            /// Disposes the mask image
            /// </summary>
            public void Dispose()
            {
                if (_mask != null)
                {
                    _mask.Dispose();
                    _mask = null;
                }
            }
        }

        /// <summary>
        /// Implements a "greater than" threshold like MATLABS
        /// im2bw function
        /// </summary>
        /// <param name="imIn">The image to threshold</param>
        /// <param name="imThresh">The image after thresholding</param>
        /// <param name="region">The ROI in which to perform the operation</param>
        /// <param name="threshold">The threshold to apply</param>
        public static void Im2Bw(Image8 imIn, Image8 imThresh, IppiROI region, byte threshold)
        {
            IppHelper.IppCheckCall(ip.ippiThreshold_LTVal_8u_C1R(imIn[region.TopLeft], imIn.Stride, imThresh[region.TopLeft], imThresh.Stride, region.Size, (byte)(threshold + 1), 0));
            IppHelper.IppCheckCall(ip.ippiThreshold_GTVal_8u_C1IR(imThresh[region.TopLeft], imThresh.Stride, region.Size, threshold, 255));
        }

        /// <summary>
        /// Performs a 3x3 closing operation on an image - fills holes
        /// </summary>
        /// <param name="imIn">The image to perform a closing operation on</param>
        /// <param name="imClosed">The image after the closing operation</param>
        /// <param name="imCalc">Intermediate image for semi-processed version</param>
        /// <param name="region">The ROI in which to perform the operation</param>
        public static void Close3x3(Image8 imIn, Image8 imClosed , Image8 imCalc, IppiROI roi)
        {
            //Modify region we operate on to allow mask overhang
            var inner = new IppiROI(roi.X + 1, roi.Y + 1, roi.Width - 3, roi.Height - 3);
            IppHelper.IppCheckCall(ip.ippiDilate3x3_8u_C1R(imIn[inner.TopLeft], imIn.Stride, imCalc[inner.TopLeft], imCalc.Stride, inner.Size));
            IppHelper.IppCheckCall(ip.ippiErode3x3_8u_C1R(imCalc[inner.TopLeft], imCalc.Stride, imClosed[inner.TopLeft], imClosed.Stride, inner.Size));
        }

        /// <summary>
        /// Performs a closing operation in the specified ROI using the specified
        /// neighborhood mask
        /// </summary>
        /// <param name="imIn">The image to perform a closing operation on</param>
        /// <param name="imClosed">The image after the closing operation</param>
        /// <param name="imCalc">Intermediate buffer for processing</param>
        /// <param name="neighborhood">The mask for the closing operation</param>
        /// <param name="region">The image region in which to perform the operation</param>
        public static void Close(Image8 imIn, Image8 imClosed, Image8 imCalc, MorphologyMask neighborhood, IppiROI roi)
        {
            //Modify region we operate on to allow mask overhang
            var inner = new IppiROI(roi.X + neighborhood.Anchor.x, roi.Y + neighborhood.Anchor.y, roi.Width - neighborhood.Mask.Width, roi.Height - neighborhood.Mask.Height);
            IppHelper.IppCheckCall(ip.ippiDilate_8u_C1R(imIn[inner.TopLeft], imIn.Stride, imCalc[inner.TopLeft], imCalc.Stride, inner.Size, neighborhood.Mask.Image, neighborhood.Mask.Size, neighborhood.Anchor));
            IppHelper.IppCheckCall(ip.ippiErode_8u_C1R(imCalc[inner.TopLeft], imCalc.Stride, imClosed[inner.TopLeft], imClosed.Stride, inner.Size, neighborhood.Mask.Image, neighborhood.Mask.Size, neighborhood.Anchor));
        }

        /// <summary>
        /// Performs a 3x3 opening operation on an image - removes speckles
        /// </summary>
        /// <param name="imIn">The image to perform an opening operation on</param>
        /// <param name="imOpened">The image after the opening operation</param>
        /// <param name="imCalc">Intermediate image for semi-processed version</param>
        /// <param name="region">The ROI in which to perform the operation</param>
        public static void Open3x3(Image8 imIn, Image8 imOpened, Image8 imCalc, IppiROI roi)
        {
            //Modify region we operate on to allow mask overhang
            var inner = new IppiROI(roi.X + 1, roi.Y + 1, roi.Width - 3, roi.Height - 3);      
            IppHelper.IppCheckCall(ip.ippiErode3x3_8u_C1R(imIn[inner.TopLeft], imIn.Stride, imCalc[inner.TopLeft], imCalc.Stride, inner.Size));
            IppHelper.IppCheckCall(ip.ippiDilate3x3_8u_C1R(imCalc[inner.TopLeft], imCalc.Stride, imOpened[inner.TopLeft], imOpened.Stride, inner.Size));
        }

        /// <summary>
        /// Performs an opening operation in the specified ROI using the specified
        /// neighborhood mask
        /// </summary>
        /// <param name="imIn">The image to perform the opening operation on</param>
        /// <param name="imOpened">The image after the opening operation</param>
        /// <param name="imCalc">Intermediate buffer for processing</param>
        /// <param name="neighborhood">The mask for the opening operation</param>
        /// <param name="region">The image region in which to perform the operation</param>
        public static void Open(Image8 imIn, Image8 imOpened, Image8 imCalc, MorphologyMask neighborhood, IppiROI roi)
        {
            //Modify region we operate on to allow mask overhang
            var inner = new IppiROI(roi.X + neighborhood.Anchor.x, roi.Y + neighborhood.Anchor.y, roi.Width - neighborhood.Mask.Width, roi.Height - neighborhood.Mask.Height);         
            IppHelper.IppCheckCall(ip.ippiErode_8u_C1R(imIn[inner.TopLeft], imIn.Stride, imCalc[inner.TopLeft], imCalc.Stride, inner.Size, neighborhood.Mask.Image, neighborhood.Mask.Size, neighborhood.Anchor));
            IppHelper.IppCheckCall(ip.ippiDilate_8u_C1R(imCalc[inner.TopLeft], imCalc.Stride, imOpened[inner.TopLeft], imOpened.Stride, inner.Size, neighborhood.Mask.Image, neighborhood.Mask.Size, neighborhood.Anchor));
        }

        /// <summary>
        /// Generates a circular mask to use in morphological operations
        /// with the anchor in the center
        /// </summary>
        /// <param name="radius">The radius of the mask as maximum distance from center pixel (i.e. radius=0 would be 1-pixel mask)</param>
        /// <returns>The mask</returns>
        public static MorphologyMask GenerateDiskMask(int radius)
        {
            if (radius < 1)
                throw new ArgumentOutOfRangeException("radius");
            //Masks in ipps morphological functions don't really allow for stride
            //unless it is guaranteed, that the pixels within the stride are of value0
            //therefore we generate the mask with the minimum possible width and height
            //that is dividable by 4
            int diameter = 1 + 2 * radius;//center pixel is extra
            int imageWidth = (int)(Math.Ceiling(diameter / 4.0) * 4);
            IppiPoint center = new IppiPoint(radius,radius);
            Image8 mask = new Image8(imageWidth, diameter);
            //Set all pixels to 0
            IppHelper.IppCheckCall(ip.ippiSet_8u_C1R(0, mask.Image, mask.Stride, mask.Size));
            //Loop over pixels, check distance and set to 1 if within circle
            for(int x=0;x<mask.Width;x++)
                for (int y = 0; y < mask.Width; y++)
                {
                    if (Distance.Euclidian(new IppiPoint(x, y), center) <= radius)
                        *mask[x, y] = 1;
                }
            //default anchor is in the circle's center
            return new MorphologyMask(mask, center);
        }

        /// <summary>
        /// Generates a rectangular mask to use in morphological operations
        /// with the anchor in the center
        /// </summary>
        /// <param name="width">The width of the rectangle</param>
        /// <param name="height">The height of the rectangle</param>
        /// <returns>The morphology mask</returns>
        public static MorphologyMask GenerateRectMask(int width, int height)
        {
            if (width < 1)
                throw new ArgumentOutOfRangeException("width");
            if (height < 1)
                throw new ArgumentOutOfRangeException("height");
            int imageWidth = (int)(Math.Ceiling(width / 4.0) * 4);
            IppiPoint center = new IppiPoint((int)Math.Floor(width / 4.0), (int)Math.Floor(height / 4.0));
            Image8 mask = new Image8(imageWidth, height);
            //Set all pixels to zero
            IppHelper.IppCheckCall(ip.ippiSet_8u_C1R(0, mask.Image, mask.Stride, mask.Size));
            //Set rectangle to 1
            IppHelper.IppCheckCall(ip.ippiSet_8u_C1R(1, mask.Image, mask.Stride, new IppiSize(width, height)));
            return new MorphologyMask(mask, center);
        }
    }
}
