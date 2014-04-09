using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MHApi.CameraLink
{
    public unsafe class PGGazelleCamera : CameraLinkCamera
    {
        #region Fields
        #endregion

        #region Properties
        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new point grey gazelle camera link object
        /// </summary>
        /// <param name="interfaceId">The interface name. Optionally followed by ::# identifying the port - 0 based</param>
        PGGazelleCamera(string interfaceId) : base(interfaceId)
        {

        }

        #endregion

        #region Methods
        #endregion

        #region Cleanup

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }

        #endregion
    }
}
