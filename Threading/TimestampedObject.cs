using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MHApi.Threading
{
    public class TimestampedObject<K>
    {
        /// <summary>
        /// The timestapm associated with the contained object
        /// </summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>
        /// The contained object
        /// </summary>
        public K Object { get; private set; }

        /// <summary>
        /// Constructor setting the timestamp to NOW
        /// </summary>
        /// <param name="obj">The contained object</param>
        public TimestampedObject(K obj)
        {
            Timestamp = DateTime.Now;
            Object = obj;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="obj">The contained object</param>
        /// <param name="timestamp">The timestamp to associate with the object</param>
        public TimestampedObject(K obj, DateTime timestamp)
        {
            Timestamp = timestamp;
            Object = obj;
        }
    }
}
