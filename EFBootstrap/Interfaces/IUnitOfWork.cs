namespace EFBootstrap
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The IUnitOfWork interface encapsulates methods that allow maintaining a list of objects 
    /// affected by a business transaction and coordinates the writing out of changes and 
    /// the resolution of concurrency problems.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        ///  Gets the read only repository.
        /// </summary>
        /// <typeparam name="T">The type of entity for which to return the repository.</typeparam>
        /// <returns>
        /// The <see cref="IReadRepository{T}"/>.
        /// </returns>
        IReadRepository<T> GetReadRepository<T>() where T : class;

        /// <summary>
        ///  Gets the read/write repository.
        /// </summary>
        /// <typeparam name="T">The type of entity for which to return the repository.</typeparam>
        /// <returns>
        /// The <see cref="IReadWriteRepository{T}"/>.
        /// </returns>
        IReadWriteRepository<T> GetReadWriteRepository<T>() where T : class;

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
