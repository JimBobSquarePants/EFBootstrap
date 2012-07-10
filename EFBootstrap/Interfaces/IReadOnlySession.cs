#region Licence
// -----------------------------------------------------------------------
// <copyright file="IReadOnlySession.cs" company="James South">
//     Copyright (c) 2012,  James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace EFBootstrap.Interfaces
{
    #region Using
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    #endregion

    /// <summary>
    /// Encapsulates methods for persisting objects to and from data storage.
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
        /// <returns>A single instance of the given type</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        T First<T>(Expression<Func<T, bool>> expression) where T : class, new();

        /// <summary>
        /// Retrieves all instances of the specified type that manage the given query.
        /// </summary>
        /// <returns>A list of all instances of the specified type.</returns>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <returns>A single instance of the given type</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        IQueryable<T> Any<T>(Expression<Func<T, bool>> expression) where T : class, new();
        #endregion
        #endregion
    }
}
