#region Licence
// -----------------------------------------------------------------------
// <copyright file="ReadWriteSession.cs" company="James South">
//     Copyright (c) James South.
//     Dual licensed under the MIT or GPL Version 2 licenses.
// </copyright>
// -----------------------------------------------------------------------
#endregion

namespace EFBootstrap.Sessions
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Entity;
    using System.Data.Entity.Validation;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using EFBootstrap.Caching;
    using EFBootstrap.Interfaces;
    #endregion

    /// <summary>
    /// Encapsulates methods for persisting objects to and from data storage
    /// using Entity Framework Code First. 
    /// </summary>
    public class ReadWriteSession : ISession
    {
        #region Fields
        /// <summary>
        /// The <see cref="T:System.Data.Entity.DbContext">DbContext</see> 
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

        /// <summary>
        /// A value indicating whether this instance has been changed.
        /// </summary>
        private bool isDirty;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="T:EFBootstrap.Sessions.ReadWriteSession"/> class. 
        /// </summary>
        /// <param name="context">The <see cref="T:System.Data.Entity.DbContext">DbContext</see> 
        /// for querying and working with entity data as objects.</param>
        public ReadWriteSession(DbContext context)
        {
            this.context = context;
        }
        #endregion

        #region Destructors
        /// <summary>
        /// Finalizes an instance of the <see cref="T:EFBootstrap.Sessions.ReadWriteSession"/> class.
        /// </summary>
        /// <remarks>
        /// Use C# destructor syntax for finalization code.
        /// This destructor will run only if the Dispose method 
        /// does not get called.
        /// It gives your base class the opportunity to finalize.
        /// Do not provide destructors in types derived from this class.
        /// </remarks>
        ~ReadWriteSession()
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
            return this.Any(expression).FirstOrDefault();
        }

        /// <summary>
        /// Retrieves all instances of the specified type.
        /// </summary>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <param name="includeCollection">
        /// An optional parameter array of strongly typed lambda expressions containing details of which related entities to
        /// eagerly load.
        /// </param>
        /// <returns>A list of all instances of the specified type.</returns>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public IQueryable<T> Any<T>(Expression<Func<T, bool>> expression = null, params Expression<Func<T, object>>[] includeCollection)
            where T : class, new()
        {
            IQueryable<T> query = this.context.Set<T>().AsQueryable<T>();

            if (includeCollection.Any())
            {
                query = includeCollection.Aggregate(query, (current, include) => current.Include(include));
            }

            // Check for a filtering expression and pull all if not.
            if (expression != null)
            {
                query = query.Where<T>(expression);
            }

            return query;
        }
        #endregion

        #region Modification
        /// <summary>
        /// Adds a single instance of the specified type.
        /// </summary>
        /// <param name="item">The instance of the given type to add.</param>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public void Add<T>(T item) where T : class, new()
        {
            this.context.Set<T>().Add(item);

            // Mark as dirty.
            this.isDirty = true;
        }

        /// <summary>
        /// Adds a collection of instances of the specified type.
        /// </summary>
        /// <param name="items">A collection of items of the given type to add.</param>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public void Add<T>(IEnumerable<T> items) where T : class, new()
        {
            Parallel.ForEach(items, this.Add);
        }

        /// <summary>
        /// Updates an instance of the specified type.
        /// </summary>
        /// <param name="item">The instance of the given type to add.</param>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public void Update<T>(T item) where T : class, new()
        {
            this.context.Entry<T>(item).State = EntityState.Modified;

            // Mark as dirty.
            this.isDirty = true;
        }

        /// <summary>
        /// Deletes a single instance of the specified type.
        /// </summary>
        /// <param name="item">The instance of the type to delete.</param>c
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public void Delete<T>(T item) where T : class, new()
        {
            this.context.Set<T>().Remove(item);

            // Mark as dirty.
            this.isDirty = true;
        }

        /// <summary>
        /// Deletes all instances of the specified type that match the query.
        /// </summary>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public void Delete<T>(Expression<Func<T, bool>> expression) where T : class, new()
        {
            IQueryable<T> query = this.Any(expression);
            Parallel.ForEach(query, this.Delete);
        }

        /// <summary>
        /// Deletes all instances of the given type.
        /// </summary> 
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        public void DeleteAll<T>() where T : class, new()
        {
            IQueryable<T> query = this.Any<T>();
            Parallel.ForEach(query, this.Delete);
        }

        /// <summary>
        /// Commits the changes made to the repository.
        /// </summary>
        public void CommitChanges()
        {
            try
            {
                // Clear the cache and save.
                this.context.SaveChanges();
            }
            catch (DbEntityValidationException e)
            {
                foreach (var validationErrors in e.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
            }

            // Clean the cache if dirty.
            if (this.isDirty)
            {
                CachedQueryResult.ClearCachedQueries();
            }
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