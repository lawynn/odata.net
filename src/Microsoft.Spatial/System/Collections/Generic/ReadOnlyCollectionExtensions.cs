﻿namespace System.Collections.Generic
{
    /// <summary>
    /// Extensions methods <see cref="IReadOnlyList{T}"/>
    /// </summary>
    public static class ReadOnlyCollectionExtensions
    {
        /// <summary>
        /// Searches for an element that matches the conditions defined by the specified predicate, and returns the zero-based index of the last occurrence within the
        /// entire <see cref="IReadOnlyList{T}"/>
        /// </summary>
        /// <typeparam name="T">The type of the elements in <paramref name="list"/></typeparam>
        /// <param name="list">The <see cref="IReadOnlyList{T}"/> to find the index of the last element of</param>
        /// <param name="predicate">The <see cref="Predicate{T}"/> delegate that defines the conditions of the element to search for.</param>
        /// <returns>
        /// The zero-based index of the last occurrence of an element that matches the conditions defined by <paramref name="predicate"/>, if found; otherwise, -1
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="list"/> or <paramref name="predicate"/> is <see langword="null"/></exception>
        public static int FindLastIndex<T>(this IReadOnlyList<T> list, Predicate<T> predicate)
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list));
            }

            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            for (int i = list.Count - 1; i > -1; --i)
            {
                if (predicate(list[i]))
                {
                    return i;
                }
            }

            return -1;
        }
    }
}