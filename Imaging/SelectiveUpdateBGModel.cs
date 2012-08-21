using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using ipp;

using MHApi.DrewsClasses;

namespace MHApi.Imaging
{
    public unsafe class SelectiveUpdateBGModel : DynamicBackgroundModel
    {

        public SelectiveUpdateBGModel(Image8 im) : base(im, 0.1F) {
            _cache = new Image8(im.Width, im.Height);
        }

        public SelectiveUpdateBGModel(Image8 im, float fUpdate) : base(im, fUpdate) {
            _cache = new Image8(im.Width, im.Height);
        }

        /// <summary>
        /// Since our selective update modifies the foreground
        /// we do this on the cached version rather than the
        /// Image handed to us
        /// </summary>
        protected Image8 _cache;

        /// <summary>
        /// Update background excluding detected regions from the update
        /// </summary>
        /// <param name="im">The new image to add to the background</param>
        /// <param name="regionsToExclude">The blobs that should be excluded from the update</param>
        public virtual void UpdateBackground(Image8 im, Blob[] regionsToExclude)
        {
            if (regionsToExclude == null)
                UpdateBackground(im);
            else
            {
                Image8 currentBG = Background;//cache it so the 8-bit representation does not get created multiple times
                ip.ippiCopy_8u_C1R(im.Image, im.Stride, _cache.Image, _cache.Stride, im.Size);
                foreach (Blob b in regionsToExclude)
                {
                    if (b != null)
                    {
                        //for each blob we don't want to update the background. This is the same
                        //as updating it after copying that part of the current background image
                        //into the forground
                        IppiROI roi = new IppiROI(b.BoundingBox);
                        ip.ippiCopy_8u_C1R(currentBG[roi.TopLeft], currentBG.Stride, _cache[roi.TopLeft], _cache.Stride, roi.Size);
                    }
                }
                UpdateBackground(_cache);
            }
        }

        /// <summary>
        /// Update background excluding detected regions from the update
        /// </summary>
        /// <param name="im">The new image to add to the background</param>
        /// <param name="regionsToExclude">The blobs that should be excluded from the update</param>
        public virtual void UpdateBackground(Image8 im, BlobWithMoments[] regionsToExclude)
        {
            if (regionsToExclude == null)
                UpdateBackground(im);
            else
            {
                Image8 currentBG = Background;//cache it so the 8-bit representation does not get created multiple times
                ip.ippiCopy_8u_C1R(im.Image, im.Stride, _cache.Image, _cache.Stride, im.Size);
                foreach (BlobWithMoments b in regionsToExclude)
                {
                    if (b != null)
                    {
                        //for each blob we don't want to update the background. This is the same
                        //as updating it after copying that part of the current background image
                        //into the forground
                        IppiROI roi = new IppiROI(b.BoundingBox.x,b.BoundingBox.y,b.BoundingBox.width,b.BoundingBox.height);
                        ip.ippiCopy_8u_C1R(currentBG[roi.TopLeft], currentBG.Stride, _cache[roi.TopLeft], _cache.Stride, roi.Size);
                    }
                }
                UpdateBackground(_cache);
            }
        }

        public virtual void UpdateBackground(Image8 im, BlobWithMoments regionToExclude)
        {
            if (regionToExclude == null)
                UpdateBackground(im);
            else
            {
                Image8 currentBG = Background;//cache it so the 8-bit representation does not get created multiple times
                ip.ippiCopy_8u_C1R(im.Image, im.Stride, _cache.Image, _cache.Stride, im.Size);             
                    if (regionToExclude != null)
                    {
                        //for each blob we don't want to update the background. This is the same
                        //as updating it after copying that part of the current background image
                        //into the forground
                        IppiROI roi = new IppiROI(regionToExclude.BoundingBox.x,regionToExclude.BoundingBox.y,regionToExclude.BoundingBox.width,regionToExclude.BoundingBox.height);
                        ip.ippiCopy_8u_C1R(currentBG[roi.TopLeft], currentBG.Stride, _cache[roi.TopLeft], _cache.Stride, roi.Size);
                    }
                UpdateBackground(_cache);
            }
        }

        public override void Dispose()
        {
            _cache.Dispose();
            base.Dispose();
        }
    }
}
