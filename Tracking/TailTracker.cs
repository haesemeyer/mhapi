using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MHApi.Imaging;
using MHApi.DrewsClasses;

using ipp;

namespace MHApi.Tracking
{
    /// <summary>
    /// Fish-tail tracker using in-frame
    /// background differencing
    /// </summary>
    public unsafe class TailTracker : IDisposable
    {
        #region Members

        /// <summary>
        /// The image region which we track
        /// </summary>
        IppiROI _trackRegion;

        /// <summary>
        /// The computed background
        /// </summary>
        Image8 _background;

        /// <summary>
        /// Our foreground
        /// </summary>
        Image8 _foreground;

        /// <summary>
        /// The thresholded foreground
        /// </summary>
        Image8 _thresholded;

        /// <summary>
        /// Processing intermediate buffer
        /// </summary>
        Image8 _calc1;

        /// <summary>
        /// The threshold used on the foreground
        /// to isolate the tail
        /// </summary>
        byte _threshold;

        /// <summary>
        /// The morphology mask for background creation
        /// </summary>
        BWImageProcessor.MorphologyMask _strel;

        /// <summary>
        /// The size of the closing mask
        /// </summary>
        byte _morphSize;

        #endregion

        #region Constructor
        #endregion

        #region Properties


        /// <summary>
        /// The tracked image region
        /// </summary>
        public IppiROI TrackRegion
        {
            get
            {
                //We don't want anyone to change parts of our ROI so return copy!!
                return new IppiROI(_trackRegion.TopLeft, _trackRegion.Size);
            }
            set
            {
                //If the ROI changes, we need to recreate our images
                _trackRegion = value;
                InitializeResources();
            }
        }

        /// <summary>
        /// The threshold used on the foreground
        /// to isolate the tail
        /// </summary>
        public byte Threshold
        {
            get
            {
                return _threshold;
            }
            set
            {
                _threshold = value;
            }
        }

        /// <summary>
        /// The size of the morphology mask used
        /// for the background generating closing operation
        /// </summary>
        public byte MorphSize
        {
            get
            {
                return _morphSize;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentException();
                //If the size changed, we need to recreate our
                //structuring element
                if (value != _morphSize)
                {
                    if (_strel != null)
                        _strel.Dispose();
                    _strel = BWImageProcessor.GenerateDiskMask(_morphSize);
                }
                _morphSize = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes all internal buffers based on the
        /// size of the current tracking region
        /// </summary>
        protected virtual void InitializeResources()
        {
            //Free old resources
            if (_background != null)
            {
                _background.Dispose();
                _background = null;
            }
            if (_foreground != null)
            {
                _foreground.Dispose();
                _foreground = null;
            }
            if (_thresholded != null)
            {
                _thresholded.Dispose();
                _thresholded = null;
            }
            if (_calc1 != null)
            {
                _calc1.Dispose();
                _calc1 = null;
            }
            //(re)create images
            _background = new Image8(_trackRegion.Width, _trackRegion.Height);
            _foreground = new Image8(_trackRegion.Width, _trackRegion.Height);
            _thresholded = new Image8(_trackRegion.Width, _trackRegion.Height);
            _calc1 = new Image8(_trackRegion.Width, _trackRegion.Height);
        }

        #endregion

        #region Cleanup

        public bool IsDisposed { get; private set; }

        protected void Dispose(bool IsDisposing)
        {
            if (_background != null)
            {
                _background.Dispose();
                _background = null;
            }
            if (_foreground != null)
            {
                _foreground.Dispose();
                _foreground = null;
            }
            if (_thresholded != null)
            {
                _thresholded.Dispose();
                _thresholded = null;
            }
            if (_calc1 != null)
            {
                _calc1.Dispose();
                _calc1 = null;
            }
            if (_strel != null)
            {
                _strel.Dispose();
                _strel = null;
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            else
            {
                Dispose(true);
                IsDisposed = true;
            }
        }

        ~TailTracker()
        {
            if (!IsDisposed)
                System.Diagnostics.Debug.WriteLine("Forgot to dispose tail tracker!");
            Dispose(false);
        }

        #endregion

       
    }
}
