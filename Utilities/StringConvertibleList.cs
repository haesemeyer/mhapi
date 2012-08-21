using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace MHApi.Utilities
{
    /// <summary>
    /// List type with a ToString overrride that concatenates all items intermitting a newline
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class StringConvertibleList<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Gives a string representation of all items, each on a separate line
        /// </summary>
        /// <returns>A string representation of all items</returns>
        public override string ToString()
        {
            if (this.Count == 0)
                return "";
            else
            {
                //Lets assume that on average each item is represented by 40 characters
                //The stringbuilder gives us better performance than a regular string
                //since the string type is immutable
                StringBuilder retval = new StringBuilder(Count * 40);
                foreach (T item in this)
                {
                    retval.AppendLine(item.ToString());
                }
                return retval.ToString();
            }
        }

    }
}
