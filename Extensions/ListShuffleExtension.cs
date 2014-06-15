using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MHApi.RND;

namespace MHApi.Extensions
{
    public static class ListShuffleExtension
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}
