using System;
using ipp;
using MHApi.DrewsClasses;
using MHApi.Imaging;

namespace MHApi.Imaging
{
    public unsafe class DynamicBackgroundModel : BackgroundModel
    {
        Image32F temp1;

        /// <summary>
        /// The fraction of the current image to use for updating the background
        /// </summary>
        public float FractionUpdate { get; protected set; }

        public override void UpdateBackground(Image8 im)
        {
            if (im.Width != width || im.Height != height)
                throw new ArgumentException("The supplied image must have the same dimensions as the background");
            var fracOld = 1.0F-FractionUpdate;
            //var temp1 = new Image32F(im);          
            var size = new IppiSize(width, height);
            IppHelper.IppCheckCall(ip.ippiConvert_8u32f_C1R(im.Image,im.Stride,temp1.Image,temp1.Stride,size));
            IppHelper.IppCheckCall(ip.ippiMulC_32f_C1IR(FractionUpdate, temp1.Image, temp1.Stride, size));
            IppHelper.IppCheckCall(ip.ippiMulC_32f_C1IR(fracOld, background.Image, background.Stride, size));
            IppHelper.IppCheckCall(ip.ippiAdd_32f_C1IR(temp1.Image, temp1.Stride, background.Image, background.Stride, size));
        }

        /// <summary>
        /// Default constructor setting FractionCurrent to 0.1
        /// </summary>
        /// <param name="im">The initial background image</param>
        public DynamicBackgroundModel(Image8 im)
            : base(im)
        {
            FractionUpdate = 0.1F;
            temp1 = new Image32F(im);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="im">The initial background image</param>
        /// <param name="fUpdate">The fraction of the current image to use for updating the background</param>
        public DynamicBackgroundModel(Image8 im, float fUpdate) : base(im) {
            if (0 > fUpdate || 1 < fUpdate)
            {
                throw new ArgumentOutOfRangeException("fUpdate", "The update fraction has to be bigger than 0 and smaller than 1");
            }
            FractionUpdate = fUpdate;
            temp1 = new Image32F(im);
        }

        public override void Dispose()
        {
            if (temp1 != null)
            {
                temp1.Dispose();
                temp1 = null;
            }
            base.Dispose();
        }
    }
}
