// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IReadOnlySession.cs" company="James South">
//   Copyright (c) James South
//   Licensed under GNU LGPL v3.
// </copyright>
// <summary>
//   Encapsulates methods for persisting objects from data storage.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EFBootstrap.Interfaces
{
    #region Using
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    #endregion

    /// <summary>
    /// Encapsulates methods for persisting objects from data storage.
    /// </summary>
    public interface IReadOnlySession : IDisposable
    {
        #region Methods
        #region Retrieval
        /// <summary>
        /// Retrieves the first instance of the specified type that matches the given query.
        /// </summary>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <param name="includeCollection">
        /// An optional parameter array of strongly typed lambda expressions containing details of which related entities to
        /// eagerly load.
        /// </param>
        /// <returns>A single instance of the given type</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        T First<T>(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeCollection) where T : class, new();

        /// <summary>
        /// Retrieves all instances of the specified type that manage the given query.
        /// </summary>
        /// <returns>A list of all instances of the specified type.</returns>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <param name="includeCollection">
        /// An optional parameter array of strongly typed lambda expressions containing details of which related entities to
        /// eagerly load.
        /// </param>
        /// <returns>A single instance of the given type</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        IQueryable<T> Any<T>(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeCollection) where T : class, new();
        #endregion
        #endregion
    }
}
