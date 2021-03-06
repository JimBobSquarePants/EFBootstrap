﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="KeyFromExpression.cs" company="James South">
//   Copyright (c) James South
//   Licensed under GNU LGPL v3.
// </copyright>
// <summary>
//   Encapsulates methods that allow the generation of a unique string representing a function expression.
//   Adapted from work By Joakim aka Nertip <see cref="http://pastebin.com/DhHi0Cs2" />
//   and Peter Montgomery <see cref="http://petemontgomery.wordpress.com/2008/08/07/caching-the-results-of-linq-queries/ " />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EFBootstrap.Caching
{
    #region Using
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using EFBootstrap.Extensions;
    #endregion

    /// <summary>
    /// Encapsulates methods that allow the generation of a unique string representing a function expression.
    /// Adapted from work By Joakim aka Nertip <see cref="http://pastebin.com/DhHi0Cs2"/> 
    /// and Peter Montgomery <see cref="http://petemontgomery.wordpress.com/2008/08/07/caching-the-results-of-linq-queries/ "/> 
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Reviewed. Suppression is OK here.")]
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
                    // Don't evaluate new instances.
                    if (expression.NodeType == ExpressionType.New)
                    {
                        return false;
                    }

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
            // Convert the expression type to an object.
            Expression<Func<T, object>> converted = AddBox<T, bool, object>(expression);

            return EvaluateExpression(converted);
        }

        /// <summary>
        /// Returns a unique cache key for the given expression.
        /// </summary>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <returns>A single instance of the given type</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public static string GetCacheKey<T>(this Expression<Func<T, object>> expression) where T : class
        {
            return EvaluateExpression(expression);
        }

        /// <summary>
        /// Converts a Linq Expression from one type to another.
        /// <see cref="http://stackoverflow.com/questions/729295/how-to-cast-expressionfunct-datetime-to-expressionfunct-object"/> 
        /// </summary>
        /// <typeparam name="TModel">The type of entity for which to provide the method.</typeparam>
        /// <typeparam name="TFromProperty">The type to convert from.</typeparam>
        /// <typeparam name="TToProperty">The type to convert to.</typeparam>
        /// <param name="expression">The expression to convert.</param>
        /// <returns>The strongly typed lambda expression</returns>
        private static Expression<Func<TModel, TToProperty>> AddBox<TModel, TFromProperty, TToProperty>(Expression<Func<TModel, TFromProperty>> expression)
        {
            Expression converted = Expression.Convert(expression.Body, typeof(TToProperty));

            return Expression.Lambda<Func<TModel, TToProperty>>(converted, expression.Parameters);
        }

        /// <summary>
        /// Returns a unique cache key for the given expression.
        /// </summary>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <returns>A single instance of the given type</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        private static string EvaluateExpression<T>(Expression<Func<T, object>> expression)
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
            return key.ToMD5Fingerprint();
        }
        #endregion
    }
}