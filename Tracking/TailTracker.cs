﻿using System;
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

        public struct TailPoint
        {
            /// <summary>
            /// The angle of the tailpoint
            /// </summary>
            public double Angle;

            /// <summary>
            /// The distance of the point
            /// from tailstart in pixels
            /// </summary>
            public double Distance;

            /// <summary>
            /// Constructs a new tailpoit
            /// </summary>
            /// <param name="angle">The angle of the point</param>
            /// <param name="distance">The distance from tailstart</param>
            public TailPoint(double angle, double distance)
            {
                Angle = angle;
                Distance = distance;
            }
        }

        #region Members

        /// <summary>
        /// The image size this tracker
        /// will process
        /// </summary>
        IppiSize _imageSize;

        /// <summary>
        /// The region in which we expect to find the
        /// tail excluding morphology border pixels
        /// </summary>
        IppiROI _trackRegionInner;

        /// <summary>
        /// The region in which we expect to find the
        /// tail including morphology border pixels
        /// </summary>
        IppiROI _trackRegionOuter;

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
        int _morphSize;

        /// <summary>
        /// The number of tail-segments to track
        /// </summary>
        int _nSegments;

        /// <summary>
        /// The start of the tail
        /// </summary>
        IppiPoint _tailStart;

        /// <summary>
        /// The end of the tail
        /// </summary>
        IppiPoint _tailEnd;

        /// <summary>
        /// Framerate of acquisition - used to
        /// perform background calculation once a second
        /// </summary>
        int _frameRate;

        /// <summary>
        /// The current framenumber
        /// </summary>
        uint _frameNumber;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a new tail tracker
        /// </summary>
        /// <param name="regionToTrack">The ROI in which we should track the tail</param>
        /// <param name="tailStart">The designated starting point of the tail</param>
        /// <param name="tailEnd">The designated end point of the tail</param>
        public TailTracker(IppiSize imageSize, IppiPoint tailStart, IppiPoint tailEnd) : this(imageSize,tailStart,tailEnd,5){ }

        /// <summary>
        /// Creates a new tail tracker
        /// </summary>
        /// <param name="regionToTrack">The ROI in which we should track the tail</param>
        /// <param name="tailStart">The designated starting point of the tail</param>
        /// <param name="tailEnd">The designated end point of the tail</param>
        /// <param name="nsegments">The number of tail segments to track btw. start and end</param>
        public TailTracker(IppiSize imageSize, IppiPoint tailStart, IppiPoint tailEnd, int nsegments)
        {
            _threshold = 20;
            _morphSize = 8;
            _frameRate = 200;
            _strel = BWImageProcessor.GenerateDiskMask(_morphSize);
            _nSegments = 5;
            _imageSize = imageSize;
            _tailStart = tailStart;
            _tailEnd = tailEnd;
            //set up our track regions based on the tail positions
            DefineTrackRegions();
            NSegments = nsegments;
            InitializeImageBuffers();
        }

        #endregion

        #region Properties

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
        public int MorphSize
        {
            get
            {
                return _morphSize;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("MorphSize");
                //If the size changed, we need to recreate our
                //structuring element and redefine track regions
                if (value != _morphSize)
                {
                    if (_strel != null)
                        _strel.Dispose();
                    _strel = BWImageProcessor.GenerateDiskMask(_morphSize);
                    DefineTrackRegions();
                }
                _morphSize = value;
            }
        }

        /// <summary>
        /// The number of tail-segments to track
        /// </summary>
        public int NSegments
        {
            get
            {
                return _nSegments;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("NSegments");
                _nSegments = value;
            }
        }

        /// <summary>
        /// The start of the tail in absolute coordinates
        /// NOT in track region coordinates!
        /// </summary>
        public IppiPoint TailStart
        {
            get
            {
                return _tailStart;
            }
            set
            {
                if (value.x < 0|| value.x >= _imageSize.width || value.y < 0 || value.y >= _imageSize.height)
                    throw new ArgumentOutOfRangeException("TailStart");
                _tailStart = value;
                //redefine track region
                DefineTrackRegions();
            }
        }

        /// <summary>
        /// The end of the tail in absolute coordinates
        /// NOT in track region coordinates
        /// </summary>
        public IppiPoint TailEnd
        {
            get
            {
                return _tailEnd;
            }
            set
            {
                if (value.x < 0 || value.x >= _imageSize.width || value.y < 0 || value.y >= _imageSize.height)
                    throw new ArgumentOutOfRangeException("TailEnd");
                _tailEnd = value;
                //redefine track regions
                DefineTrackRegions();
            }
        }

        /// <summary>
        /// The current background image
        /// </summary>
        public Image8 Background
        {
            get
            {
                return _background;
            }
        }

        /// <summary>
        /// The current foreground image
        /// </summary>
        public Image8 Foreground
        {
            get
            {
                return _foreground;
            }
        }

        /// <summary>
        /// The current thresholded foreground
        /// </summary>
        public Image8 Thresholded
        {
            get
            {
                return _thresholded;
            }
        }

        /// <summary>
        /// The acquisition framerate.
        /// Used to clock background updates
        /// </summary>
        public int FrameRate
        {
            get
            {
                return _frameRate;
            }
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException("FrameRate");
                _frameRate = value;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Initializes all internal buffers based on the
        /// size of the current tracking region
        /// </summary>
        protected virtual void InitializeImageBuffers()
        {
            //create images
            _background = new Image8(_imageSize);
            _foreground = new Image8(_imageSize);
            _thresholded = new Image8(_imageSize);
            _calc1 = new Image8(_imageSize);
            //blank foreground - to ensure that the border is always black!
            ip.ippiSet_8u_C1R(0, _foreground.Image, _foreground.Stride, _imageSize);
        }

        /// <summary>
        /// Uses tail start and end positions as well
        /// as the image size to define the track region sizes
        /// </summary>
        private void DefineTrackRegions()
        {
            int startx, starty, width, height;
            //determine if tail is horizontal or vertical
            if (TailIsVertical())
            {
                starty = TailStart.y < TailEnd.y ? TailStart.y : TailEnd.y;
                height = Math.Abs(TailEnd.y - TailStart.y);
                startx = (int)((TailStart.x + TailEnd.x) / 2 - height);//ROI is centered around the tail
                width = 2* height;
            }
            else
            {
                startx = TailStart.x < TailEnd.x ? TailStart.x : TailEnd.x;
                width = Math.Abs(TailEnd.x - TailStart.x);
                starty = (int)((TailStart.y + TailEnd.y) / 2 - width);
                height = 2 * width;
            }
            //trim to fit into image dimensions
            if (startx < 0)
                startx = 0;
            if (starty < 0)
                starty = 0;
            if (startx + width > _imageSize.width)
                width = _imageSize.width - startx + 1;
            if (starty + height > _imageSize.height)
                height = _imageSize.height - starty + 1;
            //compute and trim outer coordinates
            int outerx = startx - 2 * _strel.Anchor.x;
            int outery = starty - 2* _strel.Anchor.y;
            int outerw = width + 2 * _strel.Mask.Width;
            int outerh = height + 2 * _strel.Mask.Height;
            if (outerx < 0)
                outerx = 0;
            if (outery < 0)
                outery = 0;
            if (outerx + outerw > _imageSize.width)
                outerw = _imageSize.width - outerx + 1;
            if (outery + outerh > _imageSize.height)
                outerh = _imageSize.height - outery + 1;
            //create regions
            _trackRegionInner = new IppiROI(startx, starty, width, height);
            _trackRegionOuter = new IppiROI(outerx, outery, outerw, outerh);
        }

        /// <summary>
        /// Uses tailstart and end to determine
        /// tail orientation
        /// </summary>
        /// <returns>True if the tail runs in vertical orientation</returns>
        private bool TailIsVertical()
        {
            var distX = Math.Abs(TailEnd.x - TailStart.x);
            var distY = Math.Abs(TailEnd.y - TailStart.y);
            return (distY > distX);
        }

        /// <summary>
        /// Tracks the tailsegments on the supplied image and returns
        /// the angles and distances of each segment from the tailstart
        /// </summary>
        /// <param name="image">The image on which to id the tail</param>
        /// <returns>NSegments number of TailPoints</returns>
        public TailPoint[] TrackTail(Image8 image)
        {
            //Generate background by closing operation - once a second
            if(_frameNumber % _frameRate == 0)
                BWImageProcessor.Close(image, _background, _calc1, _strel, _trackRegionOuter);
            //Compute foreground
            IppHelper.IppCheckCall(cv.ippiAbsDiff_8u_C1R(_background[_trackRegionInner.TopLeft], _background.Stride, image[_trackRegionInner.TopLeft], image.Stride, _foreground[_trackRegionInner.TopLeft], _foreground.Stride, _trackRegionInner.Size));
            //Threshold
            BWImageProcessor.Im2Bw(_foreground, _thresholded, _trackRegionInner, _threshold);

            _frameNumber++;
            return null;
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
