namespace EFBootstrap
{
    using System;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Encapsulates properties and methods representing a combination 
    /// of Unit of Work and repository patterns.
    /// </summary>
    public interface IDbContextAdapter : IDisposable
    {
        /// <summary>
        /// Gets the Configuration method of <see cref="DbContext"/> to provide access to 
        /// configuration options for the context.
        /// </summary>
        DbContextConfiguration Configuration { get; }

        /// <summary>
        /// Returns a <see cref="IDbSet{T}"/> instance for access to entities of 
        /// the given type in the context and the underlying store.
        /// </summary>
        /// <typeparam name="T">The type of entity to set the context to.</typeparam>
        /// <returns>
        /// The <see cref="IDbSet{T}"/>.
        /// </returns>
        IDbSet<T> SetContext<T>() where T : class;

        /// <summary>
        /// Creates a raw SQL query that will return entities.
        /// </summary>
        /// <typeparam name="T">The type of entity for which to provide the method.</typeparam>
        /// <param name="query">The SQL query string.</param>
        /// <param name="parameters">
        /// The parameters to apply to the SQL query string. If output parameters are used, their 
        /// values will not be available until the results have been read completely. 
        /// This is due to the underlying behavior of <see cref="DbDataReader"/>, 
        /// see <see href="http://go.microsoft.com/fwlink/?LinkID=398589"/> for more details.
        /// </param>
        /// <returns>
        /// The <see cref="DbSqlQuery"/>.
        /// </returns>
        DbSqlQuery<T> SqlQuery<T>(string query, params object[] parameters) where T : class;

        /// <summary>
        /// Gets the state of the object in the data context.
        /// </summary>
        /// <param name="entity">The entity whose state to get.</param>
        /// <returns>
        /// The <see cref="EntityState"/>.
        /// </returns>
        EntityState GetState(object entity);

        /// <summary>
        /// Sets the state of the object in the data context.
        /// </summary>
        /// <param name="entity">The entity whose state to set.</param>
        /// <param name="state">The state of the entity.</param>
        void SetState(object entity, EntityState state);

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        int SaveChanges();

        /// <summary>
        /// Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<int> SaveChangesAsync();

        /// <summary>
        /// Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);
    }
}
