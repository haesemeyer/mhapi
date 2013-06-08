using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

using BitMiracle.LibTiff.Classic;

using MHApi.DrewsClasses;

namespace MHApi.Imaging
{
    public unsafe class LibTiffWriter : IDisposable
    {

       

        #region Fields

        /// <summary>
        /// The libtiff encapsulation of a tiff file
        /// </summary>
        Tiff _tiffFile;

        /// <summary>
        /// The base-name of the file (without numbering and extension)
        /// </summary>
        string _baseName;

        /// <summary>
        /// Indicates whether this is the first frame
        /// we are writing or not
        /// </summary>
        bool _firstFrame;

        /// <summary>
        /// The current file index
        /// </summary>
        int _fileIndex;

        /// <summary>
        /// The index of the current page we write to the tiff file
        /// </summary>
        int _pageIndex;

        #endregion

        #region Constructor

        public LibTiffWriter(string fileName)
        {
            _baseName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
            if (File.Exists(CurrentFileNameWithExtension))
                throw new Exception("TiffWriter error: File " + CurrentFileNameWithExtension + " already exists");
            if (!Directory.Exists(Path.GetDirectoryName(CurrentFileNameWithExtension)))
                Directory.CreateDirectory(Path.GetDirectoryName(CurrentFileNameWithExtension));
            _firstFrame = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The full name of the current file taking the file-index
        /// into account
        /// </summary>
        public string CurrentFileNameWithExtension { 
            get { 
                return _baseName + "_" + _fileIndex.ToString("D4") + ".tif";
            } 
        }

        #endregion

        #region Methods

        public void WriteFrame(Image16 frame)
        {
            //if this is the first frame of the current file
            //we have to create the file
            if (_firstFrame)
            {
                System.Diagnostics.Debug.Assert(_tiffFile == null, "Tried first write but tiffile not null");
                _pageIndex = 0;
                _tiffFile = Tiff.Open(CurrentFileNameWithExtension, "w");
                //write first page
                WritePage(frame);
                //increment page index
                _pageIndex++;
                _firstFrame = false;
            }
            else
            {
                //TiffStream stream = _tiffFile.GetStream();
                if (new FileInfo(CurrentFileNameWithExtension).Length > 1900000000)//limit file size for MATLABs sake
                {
                    _fileIndex++;
                    _firstFrame = true;
                    _tiffFile.Dispose();
                    _tiffFile = null;
                    WriteFrame(frame);
                    return;
                }//if file size maxed out
                //write next page
                WritePage(frame);
                //increment page index
                _pageIndex++;
            }
        }

        public void WriteFrame(Image8 frame)
        {
            //if this is the first frame of the current file
            //we have to create the file
            if (_firstFrame)
            {
                System.Diagnostics.Debug.Assert(_tiffFile == null,"Tried first write but tiffile not null");
                _pageIndex = 0;
                _tiffFile = Tiff.Open(CurrentFileNameWithExtension, "w");
                //write first page
                WritePage(frame);
                //increment page index
                _pageIndex++;
                _firstFrame = false;
            }
            else
            {
                //TiffStream stream = _tiffFile.GetStream();
                if (new FileInfo(CurrentFileNameWithExtension).Length > 1900000000)//limit file size for MATLABs sake
                {
                    _fileIndex++;
                    _firstFrame = true;
                    _tiffFile.Dispose();
                    _tiffFile = null;
                    WriteFrame(frame);
                    return;
                }//if file size maxed out
                //write next page
                WritePage(frame);
                //increment page index
                _pageIndex++;
            }
        }

        /// <summary>
        /// Writes a 16-bit greyscale image to a multipage tiff file.
        /// </summary>
        /// <param name="frame">The 16-bit greyscale image to write</param>
        void WritePage(Image16 frame)
        {
            _tiffFile.SetField(TiffTag.IMAGEWIDTH, frame.Width);
            _tiffFile.SetField(TiffTag.IMAGELENGTH, frame.Height);
            _tiffFile.SetField(TiffTag.BITSPERSAMPLE, 16);
            _tiffFile.SetField(TiffTag.SAMPLESPERPIXEL, 1);
            _tiffFile.SetField(TiffTag.ROWSPERSTRIP, frame.Height);
            _tiffFile.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
            byte[] rowBuffer = new byte[frame.Width * 2];
            for (int row = 0; row < frame.Height; row++)
            {
                Marshal.Copy((IntPtr)frame[0, row], rowBuffer, 0, frame.Width * 2);
                _tiffFile.WriteScanline(rowBuffer, row);
            }
            _tiffFile.WriteDirectory();
        }

        /// <summary>
        /// Writes an 8-bit greyscale image to a multipage tiff file.
        /// </summary>
        /// <param name="frame">The 8-bit greyscale image to write</param>
        void WritePage(Image8 frame)
        {
            _tiffFile.SetField(TiffTag.IMAGEWIDTH, frame.Width);
            _tiffFile.SetField(TiffTag.IMAGELENGTH, frame.Height);
            _tiffFile.SetField(TiffTag.BITSPERSAMPLE, 8);
            _tiffFile.SetField(TiffTag.SAMPLESPERPIXEL, 1);
            _tiffFile.SetField(TiffTag.ROWSPERSTRIP, frame.Height);
            _tiffFile.SetField(TiffTag.SUBFILETYPE, FileType.PAGE);
            byte[] rowBuffer = new byte[frame.Width];
            for (int row = 0; row < frame.Height; row++)
            {
                Marshal.Copy((IntPtr)frame[0,row], rowBuffer, 0, frame.Width);
                _tiffFile.WriteScanline(rowBuffer, row);
            }
            _tiffFile.WriteDirectory();
        }

        #endregion

        #region IDisposable

        public bool IsDisposed;

        public void Dispose()
        {
            if (_tiffFile != null)
            {
                _tiffFile.Close();
                _tiffFile.Dispose();
                _tiffFile = null;
            }
            IsDisposed = true;
        }

        ~LibTiffWriter()
        {
            if (!IsDisposed)
            {
                Dispose();
            }
        }

        #endregion
    }
}
