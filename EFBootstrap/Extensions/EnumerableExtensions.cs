﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EnumerableExtensions.cs" company="James South">
//   Copyright (c) James South
//   Licensed under GNU LGPL v3.
// </copyright>
// <summary>
//   Encapsulates a series of time saving extension methods to <see cref="T:System.Collections.Generic.IEnumerable`1" />.
//   Based on the work by Peter Montgomery
//   <see cref="http://petemontgomery.wordpress.com" />
//   <see cref="http://petemontgomery.wordpress.com/2008/08/07/caching-the-results-of-linq-queries/ " />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EFBootstrap.Extensions
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Text;
    #endregion

    /// <summary>
    /// Encapsulates a series of time saving extension methods to <see cref="T:System.Collections.Generic.IEnumerable`1"/>.
    /// Based on the work by Peter Montgomery
    /// <see cref="http://petemontgomery.wordpress.com"/> 
    /// <see cref="http://petemontgomery.wordpress.com/2008/08/07/caching-the-results-of-linq-queries/ "/>   
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Creates a doubly linked list from the <see cref="T:System.Collections.Generic.IEnumerable`1"/>.
        /// </summary>
        /// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1"/> to produce the doubly linked list from.</param>
        /// <returns>A doubly linked list from the <see cref="T:System.Collections.Generic.IEnumerable`1"/>.</returns>
        /// <typeparam name="T">The type of object that is enumerated.</typeparam>
        public static LinkedList<T> ToLinkedList<T>(this IEnumerable<T> source)
        {
            return new LinkedList<T>(source);
        }

        /// <summary>
        /// Returns a concatenated string separated by the given separator from the
        /// given IEnumerable.
        /// </summary>
        /// <param name="source">The <see cref="T:System.Collections.Generic.IEnumerable`1"/> to parse.</param>
        /// <param name="selector">The function expression to add to the String.</param>
        /// <param name="separator">The separator that defines separate function expressions.</param>
        /// <returns>A a concatenated string separated by the given separator from the given <see cref="T:System.Collections.Generic.IEnumerable`1"/>.</returns>
        /// <typeparam name="T">The type of object that is enumerated.</typeparam>
        public static string ToConcatenatedString<T>(this IEnumerable<T> source, Func<T, string> selector, string separator)
        {
            StringBuilder stringBuilder = new StringBuilder();
            bool needSeparator = false;

            foreach (T item in source)
            {
                if (needSeparator)
                {
                    stringBuilder.Append(separator);
                }

                stringBuilder.Append(selector(item));
                needSeparator = true;
            }

            return stringBuilder.ToString();
        }
    }
}
