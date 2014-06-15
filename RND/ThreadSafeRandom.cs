using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

namespace MHApi.RND
{
    //The following is from a stack-overflow discussion at:
    //http://stackoverflow.com/questions/273313/randomize-a-listt-in-c-sharp

    public static class ThreadSafeRandom
    {
        [ThreadStatic]
        private static Random Local;

        public static Random ThisThreadsRandom
        {
            get { return Local ?? (Local = new Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId))); }
        }
    }
}
