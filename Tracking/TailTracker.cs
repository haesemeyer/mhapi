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
        /// <summary>
        /// Represents the tracking result
        /// for one tail segment giving its angle and distance
        /// from the tail-start
        /// </summary>
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
            /// The cartesian tail coordinate
            /// identified for this tail segment
            /// </summary>
            public IppiPoint Coordinate;

            
            /// <summary>
            /// Constructs a new tailpoit
            /// </summary>
            /// <param name="angle">The angle of the point</param>
            /// <param name="distance">The distance from tailstart</param>
            /// <param name="coordinate">The cartesian coordinate of the tailpoint</param>
            public TailPoint(double angle, double distance, IppiPoint coordinate)
            {
                Angle = angle;
                Distance = distance;
                Coordinate = coordinate;
            }
        }

        /// <summary>
        /// Represents a point that should be testesd for the
        /// presence of the tail during tracking
        /// </summary>
        private struct TailTestCoordinate
        {
            /// <summary>
            /// The angle of this coordinate
            /// </summary>
            public double Angle;

            /// <summary>
            /// The radius of this coordinate
            /// </summary>
            public double Radius;

            /// <summary>
            /// The coordinate itself
            /// </summary>
            public IppiPoint Coordinate;

            public TailTestCoordinate(double angle, double radius, IppiPoint coordinate)
            {
                Angle = angle;
                Radius = radius;
                Coordinate = coordinate;
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
        /// Thread-safety of region updates
        /// </summary>
        object _regionLock = new object();

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

        /// <summary>
        /// This 2D array contains all the points to scan
        /// for each tail segment [NAngles,_nSegments]
        /// </summary>
        TailTestCoordinate[,] _pointsToScan;

        object _scanPointLock = new object();

        /// <summary>
        /// During tracking used to store all
        /// positive angles that contain foreground
        /// </summary>
        int* _angleStore;

        /// <summary>
        /// Whenever coordinates or parameters change
        /// we want to refresh the background
        /// </summary>
        bool _bgValid;

        /// <summary>
        /// The tail angle at which we start to scan (-90 degrees)
        /// </summary>
        const double ScanStartAngle = -1 * Math.PI / 2;

        /// <summary>
        /// We scan over a whole 180 degrees
        /// </summary>
        const double SweepAngle = Math.PI;

        /// <summary>
        /// The number of angle steps we scan over (901 means 0.2 degrees per step including the outer angles of -90 and +90)
        /// </summary>
        const int NAngles = 901;

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
            InitializeImageBuffers();//set up image buffers
            InitializeScanPoints();//create scan points appropriate for the tail parameters
            //Initialize our angle store for tracking (size never changes)
            _angleStore = (int*)System.Runtime.InteropServices.Marshal.AllocHGlobal(NAngles*4);
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
                //redefine scan points
                InitializeScanPoints();
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
                //redefine scan points
                InitializeScanPoints();
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
                //redefine scan points
                InitializeScanPoints();
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
        /// Based on the current tail-start and tail-end
        /// as well as the number of segments computes the
        /// scan points used during tracking
        /// </summary>
        protected virtual void InitializeScanPoints()
        {
            double angleStep = SweepAngle / NAngles;
            lock (_scanPointLock)
            {
                _pointsToScan = new TailTestCoordinate[NAngles, _nSegments];
                double segmentDistance = 0;
                //Compute distance between consecutive segments
                if (TailIsVertical())
                {
                    segmentDistance = Math.Abs(_tailEnd.y - _tailStart.y) / (double)_nSegments;
                }
                else
                    segmentDistance = Math.Abs(_tailEnd.x - _tailStart.x) / (double)_nSegments;
                //establish scan points:
                //loop over segments
                for (int i = 0; i < _nSegments; i++)
                {
                    double radius = (i + 1) * segmentDistance;//the radius of the current segment
                    int lastX = -1;//the last x-coordinate we added to the list
                    int lastY = -1;//the last y-coordinate we added to the list
                    int lastIndex = -1;//the last index were we added a point
                    //loop over all angles - THE FOLLOWING CODE CURRENTLY ASSUMES THE TAIL IS VERTICAL, FACING DOWN!!!
                    for (int j = 0; j < NAngles; j++)
                    {
                        double currAngle = ScanStartAngle + angleStep * j;
                        int currX = (int)Math.Floor(radius * Math.Sin(currAngle) + TailStart.x);
                        int currY = (int)Math.Floor(radius * Math.Cos(currAngle) + TailStart.y);
                        if (currX != lastX || currY != lastY)//new point on circle identified
                        {
                            lastIndex++;
                            _pointsToScan[lastIndex, i] = new TailTestCoordinate(currAngle, radius, new IppiPoint(currX, currY));
                            lastX = currX;
                            lastY = currY;
                        }
                    }
                }
            }//release lock
        }

        /// <summary>
        /// Uses tail start and end positions as well
        /// as the image size to define the track region sizes
        /// </summary>
        private void DefineTrackRegions()
        {
            lock (_regionLock)
            {
                int startx, starty, width, height;
                //determine if tail is horizontal or vertical
                if (TailIsVertical())
                {
                    starty = TailStart.y < TailEnd.y ? TailStart.y : TailEnd.y;
                    height = Math.Abs(TailEnd.y - TailStart.y);
                    startx = (int)((TailStart.x + TailEnd.x) / 2 - height);//ROI is centered around the tail
                    width = 2 * height;
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
                //compute and trim outer coordinates - morphological operations need to leave
                //border pixels and don't compute anything useful within the border for those
                //border pixels - therefore we leave a border twice the size of our structuring
                //element
                int outerx = startx - 2 * _strel.Anchor.x;
                int outery = starty - 2 * _strel.Anchor.y;
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
            _bgValid = false;
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
        /// Forces the tracker to update the background
        /// at the next frame
        /// </summary>
        public void ForceBGUpdate()
        {
            _bgValid = false;
        }

        /// <summary>
        /// Tracks the tailsegments on the supplied image and returns
        /// the angles and distances of each segment from the tailstart
        /// </summary>
        /// <param name="image">The image on which to id the tail</param>
        /// <returns>NSegments number of TailPoints</returns>
        public TailPoint[] TrackTail(Image8 image)
        {
            lock (_regionLock)
            {
                //CURRENTLY ONLY DOWNWARD FACING TAILS ARE PROPERLY SUPPORTED!!!
                //Generate background by closing operation - once a second or whenever our coordinates changed
                if (_frameNumber % _frameRate == 0 || !_bgValid)
                {
                    if (!_bgValid)
                    {
                        //if our regions changed, reblank our images!
                        ip.ippiSet_8u_C1R(0, _background.Image, _background.Stride, _background.Size);
                        ip.ippiSet_8u_C1R(0, _foreground.Image, _foreground.Stride, _foreground.Size);
                        ip.ippiSet_8u_C1R(0, _thresholded.Image, _thresholded.Stride, _thresholded.Size);
                    }
                    BWImageProcessor.Close(image, _background, _calc1, _strel, _trackRegionOuter);
                    _bgValid = true;
                }
                //Compute foreground
                IppHelper.IppCheckCall(cv.ippiAbsDiff_8u_C1R(_background[_trackRegionInner.TopLeft], _background.Stride, image[_trackRegionInner.TopLeft], image.Stride, _foreground[_trackRegionInner.TopLeft], _foreground.Stride, _trackRegionInner.Size));
                //Threshold
                BWImageProcessor.Im2Bw(_foreground, _thresholded, _trackRegionInner, _threshold);
                //Fill small holes
                BWImageProcessor.Close3x3(_thresholded, _thresholded, _calc1, _trackRegionInner);
            }
            //Tracking concept: Whenever new tail coordinates or segment numbers are defined
            //we compute for each segment in 0.2 degree steps all points on a -90 to +90 degree arc
            //and store the coordinates with the respective angles and radii
            //Depending on the actual distance we won't need all 900 points obviously so only
            //unique coordinates get stored with the rest of the array "empty" (array will be 900xnSections in size)
            //During tracking, for each segment we loop over the precomputed coordinates (until the first "empty" coordinate is reached)
            //and store all pixels where the foreground is white. Since the angles are inherently sorted (coming from a directional sweep)
            //we can then easily retrieve the median coordinate
            var retval = new TailPoint[_nSegments];
            lock (_scanPointLock)
            {
                //loop over tail segments
                for (int i = 0; i < _nSegments; i++)
                {
                    //loop over all scan-points, exiting loop if empty
                    //points are reached then find tail angle of this segment
                    int pointsFound = 0;//the number of non-zero pixels identified
                    for (int j = 0; j < NAngles; j++)
                    {
                        if (_pointsToScan[j, i].Radius == 0)//have gone past the end of valid points
                            break;
                        //If current point is outside of the image, ignore it
                        IppiPoint pt = _pointsToScan[j, i].Coordinate;
                        if (pt.x < 0 || pt.y < 0 || pt.x >= _imageSize.width || pt.y >= _imageSize.height)
                            continue;
                        //if the value at the current point >0 we mark that angle as valid
                        //by storing the position in our anglestore
                        if (*_thresholded[pt] > 0)
                        {
                            _angleStore[pointsFound] = j;
                            pointsFound++;
                        }
                    }
                    //if we have more than two points, compute average after
                    //removeing the minimum and maximum
                    double tailAngle;
                    IppiPoint coordinate;
                    if (pointsFound == 0)
                    {
                        tailAngle = double.NaN;
                        coordinate = new IppiPoint();
                    }
                    else if (pointsFound < 3)
                    {
                        tailAngle = _pointsToScan[_angleStore[0], i].Angle;
                        coordinate = _pointsToScan[_angleStore[0], i].Coordinate;
                    }
                    else
                    {
                        //we want the angle at the median position of valid points
                        //we don't compute intermediate positions however (we should be able to afford this since our angle step is so small), so in case
                        //of an even number of points, the top of the lower half currently
                        //wins - since array indexing is zero based, we have to subtract 1
                        //from the computed median position
                        var pos = _angleStore[(int)(Math.Ceiling(pointsFound / 2.0) - 1)];
                        tailAngle = _pointsToScan[pos, i].Angle;
                        coordinate = _pointsToScan[pos, i].Coordinate;
                    }
                    retval[i] = new TailPoint(tailAngle, _pointsToScan[0, i].Radius, coordinate);
                }
            }
            _frameNumber++;
            return retval;
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
            if (_angleStore != null)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal((IntPtr)_angleStore);
                _angleStore = null;
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
