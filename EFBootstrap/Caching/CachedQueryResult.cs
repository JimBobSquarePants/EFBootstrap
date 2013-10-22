// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CachedQueryResult.cs" company="James South">
//   Copyright (c) James South
//   Licensed under GNU LGPL v3.
// </copyright>
// <summary>
//   Encapsulates methods which allow the caching and retrieval of linq queries.
//   Based on the work by Peter Montgomery
//   <see cref="http://petemontgomery.wordpress.com" />
//   <see cref="http://petemontgomery.wordpress.com/2008/08/07/caching-the-results-of-linq-queries/ " />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EFBootstrap.Caching
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.Caching;
    using EFBootstrap.Extensions;
    #endregion

    /// <summary>
    /// Encapsulates methods which allow the caching and retrieval of linq queries.
    /// Based on the work by Peter Montgomery
    /// <see cref="http://petemontgomery.wordpress.com"/> 
    /// <see cref="http://petemontgomery.wordpress.com/2008/08/07/caching-the-results-of-linq-queries/ "/> 
    /// </summary>
    public static class CachedQueryResult
    {
        /// <summary>
        /// The fallback cache length.
        /// </summary>
        private const int CacheLength = 10;

        #region Methods
        #region Get
        /// <summary>
        /// Returns the result of the query; if possible from the cache, otherwise
        /// the query is materialized and the result cached before being returned.
        /// The cache entry has a ten minute sliding expiration with normal priority.
        /// </summary>
        /// <param name="query">The <see cref="T:System.Linq.IQueryable"/> for which to return the query.</param>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <param name="includeCollection">
        /// An optional parameter array of strongly typed lambda expressions containing details of which related entities to
        /// eagerly load.
        /// </param>
        /// <returns>The result of the query; if possible from the cache</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public static IEnumerable<T> FromCache<T>(
            this IQueryable<T> query,
            Expression<Func<T, bool>> expression,
            params Expression<Func<T, object>>[] includeCollection) where T : class
        {
            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(CacheLength) };
            return query.FromCache(expression, cacheItemPolicy, includeCollection);
        }

        /// <summary>
        /// Returns the result of the query; if possible from the cache, otherwise
        /// the query is materialized and the result cached before being returned.
        /// </summary>
        /// <param name="query">The <see cref="T:System.Linq.IQueryable"/> for which to return the query.</param>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <param name="cacheItemPolicy">
        /// Represents a set of eviction and expiration details for a specific cache entry.
        /// </param>
        /// <param name="includeCollection">
        /// An optional parameter array of strongly typed lambda expressions containing details of which related entities to
        /// eagerly load.
        /// </param>
        /// <returns>The result of the query; if possible from the cache</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public static IEnumerable<T> FromCache<T>(
            this IQueryable<T> query,
            Expression<Func<T, bool>> expression,
            CacheItemPolicy cacheItemPolicy,
            params Expression<Func<T, object>>[] includeCollection) where T : class
        {
            // Pull the correct key to cache the item with.
            string key = expression == null
                ? KeyFromExpression.Prefix + typeof(T).FullName.ToMD5Fingerprint()
                : KeyFromExpression.Prefix + expression.GetCacheKey();

            // Get the includes and add them to the cache key.
            if (includeCollection.Any())
            {
                key = includeCollection.Aggregate(key, (current, include) => current + include.GetCacheKey());
            }

            // Try to get the query result from the cache
            List<T> result = CacheManager.GetItem(key) as List<T>
                             ?? ToCache(query, expression, key, cacheItemPolicy).ToList();

            return result;
        }
        #endregion

        #region Set
        /// <summary>
        /// Adds the result of the query to the cache.
        /// The query is materialized before being returned.
        /// The cache entry has a one minute sliding expiration with normal priority.
        /// </summary>
        /// <param name="query">The <see cref="T:System.Linq.IQueryable"/> for which to return the query.</param>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <param name="key">The key by which to store the <see cref="T:System.Linq.IQueryable"/> result.</param>
        /// <returns>The result of the query; if possible from the cache</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public static IEnumerable<T> ToCache<T>(this IQueryable<T> query, Expression<Func<T, bool>> expression, string key) where T : class
        {
            CacheItemPolicy cacheItemPolicy = new CacheItemPolicy { SlidingExpiration = TimeSpan.FromMinutes(CacheLength) };
            return query.ToCache(expression, key, cacheItemPolicy);
        }

        /// <summary>
        /// Adds the result of the query to the cache.
        /// The query is materialized before being returned.
        /// </summary>
        /// <param name="query">
        /// The <see cref="T:System.Linq.IQueryable"/> for which to return the query.
        /// </param>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <param name="key">
        /// The key by which to store the <see cref="T:System.Linq.IQueryable"/> result.
        /// </param>
        /// <param name="cacheItemPolicy">
        /// Represents a set of eviction and expiration details for a specific cache entry.
        /// </param>
        /// <returns>
        /// The result of the query; if possible from the cache
        /// </returns>
        /// <typeparam name="T">
        /// The type of entity for which to provide the method.
        /// </typeparam>
        public static IEnumerable<T> ToCache<T>(this IQueryable<T> query, Expression<Func<T, bool>> expression, string key, CacheItemPolicy cacheItemPolicy) where T : class
        {
            List<T> result = query.ToList();

            CacheManager.AddItem(key, result, cacheItemPolicy);

            return result;
        }
        #endregion

        /// <summary>
        /// Clears all cached queries from the cache.
        /// </summary>
        public static void ClearCachedQueries()
        {
            CacheManager.Clear();
        }
        #endregion
    }
}