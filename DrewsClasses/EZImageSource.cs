using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ipp;
using MHApi.Utilities;

namespace MHApi.DrewsClasses {
    public unsafe class EZImageSource : PropertyChangeNotification {

        /// <summary>
        /// Raw version of the image
        /// </summary>
        Image8 imageRaw;

        /// <summary>
        /// Image scaled to 0...cMax
        /// </summary>
        Image8 imageScaled;

        /// <summary>
        /// The windows image source
        /// </summary>
        WriteableBitmap imageSource;

        /// <summary>
        /// The windows image source
        /// </summary>
        public WriteableBitmap ImageSource {
            get { return imageSource; }
            private set { imageSource = value; RaisePropertyChanged("ImageSource"); }
        }

        /// <summary>
        /// Brightness scale - pixel values
        /// >=cMax will be set to 255
        /// </summary>
        double cMax;

        /// <summary>
        /// Brightness scale - pixel values
        /// >=cMax will be set to 255
        /// </summary>
        public double CMax {
            get { return cMax; }
            set
            {
                if (cMax < 0 || cMax > 255)
                    throw new ArgumentOutOfRangeException("CMax", "CMax has to be >=0 and <=255");
                cMax = value;
                //redraw newly scaled image
                UpdateImageScaled(1000);
                RaisePropertyChanged("CMax");
            }
        }

        /// <summary>
        /// The width of the image
        /// </summary>
        public int Width {
            get { return imageRaw.Width; }
        }

        /// <summary>
        /// The height of the image
        /// </summary>
        public int Height {
            get { return imageRaw.Height; }
        }

        /// <summary>
        /// Create new image source
        /// </summary>
        public EZImageSource() {
            imageRaw = new Image8(100, 100);
            imageScaled = new Image8(imageRaw.Width, imageRaw.Height);
            ///create the actual windows image source on the UI thread
            DispatcherHelper.UIDispatcher.Invoke(new Action(() => {
                ImageSource = new WriteableBitmap(imageRaw.Width, imageRaw.Height, 96, 96, PixelFormats.Gray8, null);
            }));
            //initialize CMax
            cMax = 255;
        }

        /// <summary>
        /// Writes an image on the user-interface thread BLOCKING until the UI thread completed the work!
        /// </summary>
        /// <param name="image">The image to write</param>
        /// <param name="cancel">Thread signal to cancel the write</param>
        public void Write(Image8 image, AutoResetEvent cancel) {
            //if image sizes don't match re-initialize raw image, scaled image and UI image
            if (imageRaw.Width != image.Width || imageRaw.Height != image.Height)
            {
                if (imageRaw != null)
                    imageRaw.Dispose();
                imageRaw = new Image8(image.Width, image.Height);
                if (imageScaled != null)
                    imageScaled.Dispose();
                imageScaled = new Image8(image.Width, image.Height);
                var done = new AutoResetEvent(false);
                DispatcherHelper.CheckBeginInvokeOnUI(() =>
                {
                    ImageSource = new WriteableBitmap(imageRaw.Width, imageRaw.Height, 96, 96, PixelFormats.Gray8, null);
                    done.Set();
                });
                //wait for our done event, indicating that the bitmap has been created (index 1) or
                //alternatively for cancel which tells us to finish
                if (WaitHandle.WaitAny(new[] { cancel, done }) == 0)
                    throw new OperationCanceledException();
            }
            //copy the new image into image-raw
            ip.ippiCopy_8u_C1R(image.Image, image.Stride, imageRaw.Image, imageRaw.Stride, image.Size);
            //scale image if necessary and write to screen
            UpdateImageScaled(cancel);
        }

        /// <summary>
        /// Update scaled version of the image if necessary
        /// and write to screen
        /// </summary>
        /// <param name="cancel">Signals us to stop</param>
        void UpdateImageScaled(AutoResetEvent cancel) {
            //copy raw image to scaled version
            ip.ippiCopy_8u_C1R(imageRaw.Image, imageRaw.Stride, imageScaled.Image, imageScaled.Stride, imageRaw.Size);
            //scale brightness
            ip.ippiMulC_8u_C1IRSfs((byte)(255 / CMax), imageScaled.Image, imageScaled.Stride, imageScaled.Size, 0);
            var done = new AutoResetEvent(false);
            //write scaled image to screen
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                ImageSource.WritePixels(new Int32Rect(0, 0, imageRaw.Width, imageRaw.Height), (IntPtr)imageScaled.Image, imageScaled.Stride * imageScaled.Height, imageScaled.Stride);
                done.Set();
            });
            //Block on UI thread until either we are asked to stop or write operation is finished
            if (WaitHandle.WaitAny(new[] { cancel, done }) == 0)
                throw new OperationCanceledException();
        }

        /// <summary>
        /// Update scaled version of the image if necessary
        /// and write to screen
        /// </summary>
        /// <param name="timeout">Timeout of write operation</param>
        void UpdateImageScaled(int timeout) {
            //copy raw image to scaled version
            ip.ippiCopy_8u_C1R(imageRaw.Image, imageRaw.Stride, imageScaled.Image, imageScaled.Stride, imageRaw.Size);
            //scale brightness
            ip.ippiMulC_8u_C1IRSfs((byte)(255 / CMax), imageScaled.Image, imageScaled.Stride, imageScaled.Size, 0);
            var done = new AutoResetEvent(false);
            //write scaled image to screen
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                ImageSource.WritePixels(new Int32Rect(0, 0, imageRaw.Width, imageRaw.Height), (IntPtr)imageScaled.Image, imageScaled.Stride * imageScaled.Height, imageScaled.Stride);
                done.Set();
            });
            //Block on UI thread until we either time out or write operation is finished
            if (!done.WaitOne(timeout))
                throw new OperationCanceledException();
        }

    }
}
