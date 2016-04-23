namespace EFBootstrap
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Maintains a list of objects affected by a business transaction and coordinates the writing 
    /// out of changes and the resolution of concurrency problems.
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        /// <summary>
        /// The context representing a combination of Unit of Work and repository patterns.
        /// </summary>
        private readonly IDbContextAdapter context;

        /// <summary>
        /// The repositories for querying data.
        /// </summary>
        private readonly List<object> repositories = new List<object>();

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
        /// Initializes a new instance of the <see cref="UnitOfWork"/> class.
        /// </summary>
        /// <param name="context">
        /// The context that can be used to query and store objects to/from the database.
        /// </param>
        public UnitOfWork(IDbContextAdapter context)
        {
            this.context = context;
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="UnitOfWork"/> class. 
        /// </summary>
        ~UnitOfWork()
        {
            // Do not re-create Dispose clean-up code here.
            // Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            this.Dispose(false);
        }

        /// <summary>
        ///  Gets the read only repository.
        /// </summary>
        /// <typeparam name="T">The type of entity for which to return the repository.</typeparam>
        /// <returns>
        /// The <see cref="IReadRepository{T}"/>.
        /// </returns>
        public IReadRepository<T> GetReadRepository<T>() where T : class
        {
            foreach (object repository in this.repositories)
            {
                ReadRepository<T> readRepository = repository as ReadRepository<T>;
                if (readRepository != null)
                {
                    return readRepository;
                }
            }

            IReadRepository<T> repo = new ReadRepository<T>(this.context);
            this.repositories.Add(repo);
            return repo;
        }

        /// <summary>
        ///  Gets the read/write repository.
        /// </summary>
        /// <typeparam name="T">The type of entity for which to return the repository.</typeparam>
        /// <returns>
        /// The <see cref="IReadWriteRepository{T}"/>.
        /// </returns>
        public IReadWriteRepository<T> GetReadWriteRepository<T>() where T : class
        {
            foreach (object repository in this.repositories)
            {
                ReadWriteRepository<T> readRepository = repository as ReadWriteRepository<T>;
                if (readRepository != null)
                {
                    return readRepository;
                }
            }

            IReadWriteRepository<T> repo = new ReadWriteRepository<T>(this.context);
            this.repositories.Add(repo);
            return repo;
        }

        /// <summary>
        /// Saves all changes made in this unit of work to the underlying database.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public int SaveChanges()
        {
            return this.context.SaveChanges();
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<int> SaveChangesAsync()
        {
            return await this.context.SaveChangesAsync();
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return await this.context.SaveChangesAsync(cancellationToken);
        }

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
    }
}
