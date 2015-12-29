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
        Image8 imageRaw, imageScaled;

        WriteableBitmap imageSource;
        public WriteableBitmap ImageSource {
            get { return imageSource; }
            private set { imageSource = value; RaisePropertyChanged("ImageSource"); }
        }

        double cMax;
        public double CMax {
            get { return cMax; }
            set { cMax = value; UpdateImageScaled(1000); RaisePropertyChanged("CMax"); }
        }

        public int Width {
            get { return imageRaw.Width; }
        }

        public int Height {
            get { return imageRaw.Height; }
        }

        public EZImageSource() {
            imageRaw = new Image8(100, 100);
            imageScaled = new Image8(imageRaw.Width, imageRaw.Height);
            DispatcherHelper.UIDispatcher.Invoke(new Action(() => {
                ImageSource = new WriteableBitmap(imageRaw.Width, imageRaw.Height, 96, 96, PixelFormats.Gray8, null);
            }));
            CMax = 255;
        }

        /// <summary>
        /// Writes an image on the user-interface thread BLOCKING until the UI thread completed the work!
        /// </summary>
        /// <param name="image">The image to write</param>
        /// <param name="cancel">Thread signal to cancel the write</param>
        public void Write(Image8 image, AutoResetEvent cancel) {
            if (imageRaw.Width != image.Width || imageRaw.Height != image.Height) {
                if (imageRaw != null) imageRaw.Dispose();
                imageRaw = new Image8(image.Width, image.Height);
                if (imageScaled != null) imageScaled.Dispose();
                imageScaled = new Image8(image.Width, image.Height);
                var done = new AutoResetEvent(false);
                DispatcherHelper.CheckBeginInvokeOnUI(() => {
                    ImageSource = new WriteableBitmap(imageRaw.Width, imageRaw.Height, 96, 96, PixelFormats.Gray8, null);
                    done.Set();
                });
                if (WaitHandle.WaitAny(new[] { cancel, done }) == 0)
                    throw new OperationCanceledException();
            }
            ip.ippiCopy_8u_C1R(image.Image, image.Stride, imageRaw.Image, imageRaw.Stride, image.Size);
            UpdateImageScaled(cancel);
        }

        void UpdateImageScaled(AutoResetEvent cancel) {
            ip.ippiCopy_8u_C1R(imageRaw.Image, imageRaw.Stride, imageScaled.Image, imageScaled.Stride, imageRaw.Size);
            ip.ippiMulC_8u_C1IRSfs((byte)(255 / CMax), imageScaled.Image, imageScaled.Stride, imageScaled.Size, 0);
            var done = new AutoResetEvent(false);
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                ImageSource.WritePixels(new Int32Rect(0, 0, imageRaw.Width, imageRaw.Height), (IntPtr)imageScaled.Image, imageScaled.Stride * imageScaled.Height, imageScaled.Stride);
                done.Set();
            });
            if (WaitHandle.WaitAny(new[] { cancel, done }) == 0)
                throw new OperationCanceledException();
        }

        void UpdateImageScaled(int timeout) {
            ip.ippiCopy_8u_C1R(imageRaw.Image, imageRaw.Stride, imageScaled.Image, imageScaled.Stride, imageRaw.Size);
            ip.ippiMulC_8u_C1IRSfs((byte)(255 / CMax), imageScaled.Image, imageScaled.Stride, imageScaled.Size, 0);
            var done = new AutoResetEvent(false);
            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                ImageSource.WritePixels(new Int32Rect(0, 0, imageRaw.Width, imageRaw.Height), (IntPtr)imageScaled.Image, imageScaled.Stride * imageScaled.Height, imageScaled.Stride);
                done.Set();
            });
            if (!done.WaitOne(timeout))
                throw new OperationCanceledException();
        }

    }
}
