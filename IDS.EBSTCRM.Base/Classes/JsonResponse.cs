using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IDS.EBSTCRM.Base.Classes {
    /// <summary>
    /// This class return the json response
    /// </summary>
    public class JsonResponse : IDisposable {
        /// <summary>
        /// Return the status for current request. i.e. >>  1 = Success || 0 = Error
        /// </summary>
        public Response Status { get; set; }

        /// <summary>
        /// Return the response data for current request.
        /// </summary>
        public String ResponseData { get; set; }

        /// <summary>
        /// Return list of controls who cause error or not having valid inputs. i.e.  >> ctrl1, ctrl2, ctrl3...
        /// </summary>
        public String Controls { get; set; }

        #region "Dispose"
        //private bool disposed = false;
        /// <summary>
        /// garbrage collector
        /// </summary>
        protected virtual void Dispose(bool disposing) { }

        /// <summary>
        /// garbrage collector
        /// </summary>
        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }

    /// <summary>
    /// Json response status
    /// </summary>
    public enum Response {

        /// <summary>
        /// session expired while making response
        /// </summary>
        SessionExpired = 2,

        /// <summary>
        /// success
        /// </summary>
        Success = 1,

        /// <summary>
        /// error
        /// </summary>
        Error = 0
    }
}
