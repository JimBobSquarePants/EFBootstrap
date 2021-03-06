﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReadOnlySession.cs" company="James South">
//   Copyright (c) James South
//   Licensed under GNU LGPL v3.
// </copyright>
// <summary>
//   Encapsulates methods for persisting objects to and from data storage
//   using Entity Framework Code First.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace EFBootstrap.Sessions
{
    #region Using
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Runtime.Caching;
    using EFBootstrap.Caching;
    using EFBootstrap.Interfaces;
    #endregion

    /// <summary>
    /// Encapsulates methods for persisting objects to and from data storage
    /// using Entity Framework Code First. 
    /// </summary>
    public class ReadOnlySession : IReadOnlySession
    {
        #region Fields
        /// <summary>
        /// The <see cref="T:System.Data.Entity.DbContext"/> 
        /// for querying and working with entity data as objects.
        /// </summary>
        private readonly DbContext context;

        /// <summary>
        /// A value indicating whether this instance of the given entity has been disposed.
        /// </summary>
        /// <value><see langword="true"/> if this instance has been disposed; otherwise, <see langword="false"/>.</value>
        /// <remarks>
        /// If the entity is disposed, it must not be disposed a second
        /// time. The isDisposed field is set the first time the entity
        /// is disposed. If the isDisposed field is true, then the Dispose()
        /// method will not dispose again. This help not to prolong the entity's
        /// life in the Garbage Collector.
        /// </remarks>
        private bool isDisposed;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:EFBootstrap.Sessions.ReadOnlySession"/> class. 
        /// </summary>
        /// <param name="context">
        /// The <see cref="T:System.Data.Entity.DbContext"/> 
        /// for querying and working with entity data as objects.
        /// </param>
        public ReadOnlySession(DbContext context)
        {
            this.context = context;
        }
        #endregion

        #region Destructors
        /// <summary>
        /// Finalizes an instance of the <see cref="EFBootstrap.Sessions.ReadOnlySession"/> class. 
        /// </summary>
        /// <remarks>
        /// Use C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method 
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in types derived from this class.
        /// </remarks>
        ~ReadOnlySession()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            this.Dispose(false);
        }
        #endregion

        #region Methods
        #region Public
        #region ISession Members
        #region Retrieval
        /// <summary>
        /// Retrieves the first instance of the specified type that matches the given query, if possible from the cache.
        /// <para>
        /// Objects are maintained in a <see cref="EntityState">Detached</see> state and 
        /// are not tracked in the <see cref="ObjectStateManager"/>.
        /// </para>
        /// <remarks>
        /// <para>
        /// It's important to note that since the method internally calls ToList(), any expression functions will 
        /// utilize LinqToObjects and the normal rules of C# comparison will apply.
        /// As a result a query this like this 
        /// <para>
        /// <example>
        /// .Single&lt;Product&gt;(x => x.Color.Equals("Black", StringComparison.InvariantCultureIgnoreCase)
        /// </example>
        /// </para>
        /// will call a NullExceptionError if the property "Color" is null.
        /// </para>
        /// The best way to avoid this exception is to use <example>==</example> or switch the sides of the argument
        /// <para>
        /// <example>
        /// .Single&lt;Product&gt;(x => "Black".Equals(x.Color, StringComparison.InvariantCultureIgnoreCase)
        /// </example>
        /// </para>
        /// </remarks>
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
        public T First<T>(Expression<Func<T, bool>> expression, params Expression<Func<T, object>>[] includeCollection)
            where T : class, new()
        {
            return this.Any(expression, includeCollection).FirstOrDefault();
        }

        /// <summary>
        /// A list of all instances of the specified type that match the expression, if possible from the cache.
        /// <para>
        /// Objects are maintained in a <see cref="EntityState">Detached</see> state and 
        /// are not tracked in the <see cref="ObjectStateManager">ObjectStateManager</see>.
        /// </para>
        /// <remarks>
        /// <para>
        /// It's important to note that since the method internally calls ToList(), any expression functions will 
        /// utilize LinqToObjects and the normal rules of C# comparison will apply.
        /// As a result a query this like this 
        /// <para>
        /// <example>
        /// .All&lt;Product&gt;().Where(x => x.Color.Equals("Black", StringComparison.InvariantCultureIgnoreCase)
        /// </example>
        /// </para>
        /// will call a NullExceptionError if the property "Color" is null.
        /// </para>
        /// The best way to avoid this exception is to use <example>==</example> or switch the sides of the argument
        /// <para>
        /// <example>
        /// .All&lt;Product&gt;().Where(x => "Black".Equals(x.Color, StringComparison.InvariantCultureIgnoreCase)
        /// </example>
        /// </para>
        /// </remarks>
        /// </summary>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <param name="includeCollection">
        /// An optional parameter array of strongly typed lambda expressions containing details of which related entities to
        /// eagerly load.
        /// </param>
        /// <returns>A list of all instances of the specified type that match the expression.</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public IQueryable<T> Any<T>(Expression<Func<T, bool>> expression = null, params Expression<Func<T, object>>[] includeCollection)
            where T : class, new()
        {
            IQueryable<T> query = this.context.Set<T>().AsNoTracking().AsQueryable();

            if (includeCollection.Any())
            {
                query = includeCollection.Aggregate(query, (current, include) => current.Include(include));
            }

            // Check for a filtering expression and pull all if not.
            if (expression != null)
            {
                query = query.Where(expression);
            }
            
            if (Configuration.CacheLength > 0)
            {
                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy
                                                      {
                                                          SlidingExpiration = TimeSpan.FromMinutes(Configuration.CacheLength)
                                                      };

                return query.FromCache(expression, cacheItemPolicy, includeCollection).AsQueryable();
            }

            return query.FromCache(expression, includeCollection).AsQueryable();
        }
        #endregion
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Disposes the object and frees resources for the Garbage Collector.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);

            // This object will be cleaned up by the Dispose method.
            // Therefore, you should call GC.SupressFinalize to
            // take this object off the finalization queue 
            // and prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes the object and frees resources for the Garbage Collector.
        /// </summary>
        /// <param name="disposing">If true, the object gets disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (this.isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // Dispose of any managed resources here.
                this.context.Dispose();
            }

            // Call the appropriate methods to clean up
            // unmanaged resources here.
            // Note disposing is done.
            this.isDisposed = true;
        }
        #endregion
        #endregion
        #endregion
    }
}