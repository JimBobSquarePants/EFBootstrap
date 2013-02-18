#region Licence
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CachedQueryResult.cs" company="James South">
//     Copyright (c) James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion

namespace EFBootstrap.Caching
{
    #region Using
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Web;
    using System.Web.Caching;
    using EFBootstrap.Extensions;
    #endregion

    /// <summary>
    /// Encapsulates methods which allow the caching and retrievel of linq queries.
    /// Loosly based on the work by Peter Montgomery
    /// http://petemontgomery.wordpress.com
    /// http://petemontgomery.wordpress.com/2008/08/07/caching-the-results-of-linq-queries/ 
    /// Licenced under GNU LGPL v3.
    /// http://www.gnu.org/licenses/lgpl.html 
    /// </summary>
    public static class CachedQueryResult
    {
        #region Methods
        #region Get
        /// <summary>
        /// Returns the result of the query; if possible from the cache, otherwise
        /// the query is materialized and the result cached before being returned.
        /// The cache entry has a one minute sliding expiration with normal priority.
        /// </summary>
        /// <param name="query">The IQueryable for which to return the query.</param>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <returns>The result of the query; if possible from the cache</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public static IEnumerable<T> FromCache<T>(this IQueryable<T> query, Expression<Func<T, bool>> expression) where T : class
        {
            return query.FromCache(expression, CacheItemPriority.Normal, TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Returns the result of the query; if possible from the cache, otherwise
        /// the query is materialized and the result cached before being returned.
        /// </summary>
        /// <param name="query">The IQueryable for which to return the query.</param>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <param name="priority">The relative cache priority of the object.</param>
        /// <param name="slidingExpiration">The timespan indicating the duration of the sliding expiration</param>
        /// <returns>The result of the query; if possible from the cache</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public static IEnumerable<T> FromCache<T>(this IQueryable<T> query, Expression<Func<T, bool>> expression, CacheItemPriority priority, TimeSpan slidingExpiration) where T : class
        {
            // Pull the correct key to cache the item with.
            string key = expression == null ? KeyFromExpression.Prefix + typeof(T).FullName.ToMD5Fingerprint() : expression.GetCacheKey();

            // Try to get the query result from the cache
            List<T> result = HttpRuntime.Cache.Get(key) as List<T>
                             ?? ToCache(query, expression, key, priority, slidingExpiration).ToList();

            return result;
        }
        #endregion

        #region Set
        /// <summary>
        /// Adds the result of the query to the cache.
        /// The query is materialized before being returned.
        /// The cache entry has a one minute sliding expiration with normal priority.
        /// </summary>
        /// <param name="query">The IQueryable for which to return the query.</param>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <param name="key">The key by which to store the IQueriable result.</param>
        /// <returns>The result of the query; if possible from the cache</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public static IEnumerable<T> ToCache<T>(this IQueryable<T> query, Expression<Func<T, bool>> expression, string key) where T : class
        {
            return query.ToCache(expression, key, CacheItemPriority.Normal, TimeSpan.FromMinutes(1));
        }

        /// <summary>
        /// Adds the result of the query to the cache.
        /// The query is materialized before being returned.
        /// </summary>
        /// <param name="query">The IQueryable for which to return the query.</param>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <param name="key">The key by which to store the IQueriable result.</param>
        /// <param name="priority">The relative cache priority of the object.</param>
        /// <param name="slidingExpiration">The timespan indicating the duration of the sliding expiration</param>
        /// <returns>The result of the query; if possible from the cache</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public static IEnumerable<T> ToCache<T>(this IQueryable<T> query, Expression<Func<T, bool>> expression, string key, CacheItemPriority priority, TimeSpan slidingExpiration) where T : class
        {
            List<T> result = query.ToList();

            HttpRuntime.Cache.Insert(
                key,
                result,
                null, // no cache dependency
                Cache.NoAbsoluteExpiration,
                slidingExpiration,
                priority,
                null); // no removal notification

            return result;
        }
        #endregion

        /// <summary>
        /// Clears all cached queries from the runtime cache.
        /// </summary>
        public static void ClearCachedQueries()
        {
            // You can't remove items from a collection whilst you are iterating over it so you need to 
            // create a list to store the items to remove.
            List<string> itemsToRemove = new List<string>();
            Cache cache = HttpRuntime.Cache;
            string prefix = KeyFromExpression.Prefix;

            IDictionaryEnumerator enumerator = cache.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (enumerator.Key.ToString().ToUpper().StartsWith(prefix))
                {
                    itemsToRemove.Add(enumerator.Key.ToString());
                }
            }

            foreach (string itemToRemove in itemsToRemove)
            {
                cache.Remove(itemToRemove);
            }
        }
        #endregion
    }
}