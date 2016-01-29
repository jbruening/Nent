using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nent.Extensions
{
    static class CollectionExtensions
    {
        public static T[] RemoveAll<T, C>(this T[] source, Func<T, C> match, C matcher)
             where C : class 
        {
            var dest = new List<T>(source.Length);
            foreach (var t in source)
            {
                if (match(t) != matcher)
                    dest.Add(t);
            }
            return dest.ToArray();
        }

        public static T[] RemoveAll<T, C>(this T[] source, Func<T, C> match, C matcher, List<T> removed)
            where C : class 
        {
            var dest = new List<T>(source.Length);
            foreach (var t in source)
            {
                if (match(t) != matcher)
                    dest.Add(t);
                else
                    removed.Add(t);
            }
            return dest.ToArray();
        }

        public static T[] RemoveAt<T>(this T[] source, int index)
        {
            var dest = new T[source.Length - 1];
            if (index > 0)
                Array.Copy(source, 0, dest, 0, index);

            if (index < source.Length - 1)
                Array.Copy(source, index + 1, dest, index, source.Length - index - 1);

            return dest;
        }

        public static T[] Add<T>(this T[] source, T value)
        {
            var dest = new T[source.Length + 1];
            Array.Copy(source, dest, source.Length);
            dest[dest.Length - 1] = value;
            return dest;
        }
    }
}
