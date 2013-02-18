#region Licence
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyFromExpression.cs" company="James South">
//     Copyright (c) James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
#endregion

namespace EFBootstrap.Caching
{
    #region Using
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using EFBootstrap.Extensions;
    #endregion

    /// <summary>
    /// Encapsulates methods that allow the generation of a unique string representing a function expression.
    /// Adapted from work By Joakim aka Nertip http://pastebin.com/DhHi0Cs2
    /// and Peter Montogomery http://petemontgomery.wordpress.com/2008/08/07/caching-the-results-of-linq-queries/ 
    /// </summary>
    public static class KeyFromExpression
    {
        #region Fields
        /// <summary>
        /// A prefix for adding to the cache key to allow easy removal of cached linq queries.
        /// </summary>
        private const string CachedQueryPrefix = "CACHED_QUERY";
        #endregion

        #region Properties
        /// <summary>
        /// Gets the prefix used for cached queries.
        /// </summary>
        public static string Prefix
        {
            get { return CachedQueryPrefix; }
        }

        /// <summary>
        /// Gets a value indicating whether the expression can be evaluated locally.
        /// </summary>
        /// <value><see langword="true"/> if the expression can be evaluated locally; otherwise, <see langword="false"/>.</value>
        private static Func<Expression, bool> CanBeEvaluatedLocally
        {
            get
            {
                return expression =>
                {
                    // Don't evaluate parameters
                    if (expression.NodeType == ExpressionType.Parameter)
                    {
                        return false;
                    }

                    // Can't evaluate queries
                    if (typeof(IQueryable).IsAssignableFrom(expression.Type))
                    {
                        return false;
                    }

                    return true;
                };
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Returns a unique cache key for the given expression.
        /// </summary>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <returns>A single instance of the given type</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public static string GetCacheKey<T>(this Expression<Func<T, bool>> expression) where T : class
        {
            // Locally evaluate as much of the query as possible
            Expression predicate = Evaluator.PartialEval(expression, CanBeEvaluatedLocally);

            // Handles local expressions.
            predicate = LocalCollectionExpander.Rewrite(predicate);

            // Use the string representation of the expression for the cache key
            string key = predicate.ToString();
            string typeName = typeof(T).Name;

            // Loop through and replace any parent parameters.
            key = expression.Parameters.Select(param => param.Name)
                                       .Aggregate(key, (current, name) => current.Replace(name + ".", typeName + "."));

            // Convert the key to an MD5 representation of the string.
            // MD5 fingerprints are not guaranteed to be be unique, 
            // but it should be computationally infeasible to find two identical fingerprints.
            // Please bear in mind that although there is not yet a known method to attack MD5, there are theoretical 
            // collisions that can be exploited against it so if you are concerned about sensitive data use a higher encryption method
            // This will affect performance though. http://msdn.microsoft.com/en-us/library/ms978415.aspx
            key = CachedQueryPrefix + key.ToMD5Fingerprint();

            return key;
        }
        #endregion
    }
}