using System;
using System.Collections.Generic;

namespace Utility.EnumEx
{
    public static class EnumEx
    {
        public static IEnumerable<(T item, int index)> Indexed<T>(this IEnumerable<T> self)
        {
            if (self == null) throw new ArgumentNullException(nameof(self));

            IEnumerable<(T item, int index)> impl()
            {
                var i = 0;
                foreach (var item in self)
                {
                    yield return (item, i);
                    ++i;
                }
            }

            return impl();
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="tflags"></param>
        /// <param name="uflags"></param>
        /// <returns></returns>
        public static bool HasFlag<T, U>(T tflags, U uflags) where T : Enum where U : Enum
        {
            if ((Convert.ToInt32(tflags) & Convert.ToInt32(uflags)) != 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static bool MultipleFlagExists<T, U>(U flags) where T : Enum where U : Enum
        {
            foreach (T value in Enum.GetValues(typeof(T)))
            {
                if (Convert.ToInt32(flags) == Convert.ToInt32(value))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
