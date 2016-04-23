namespace EFBootstrap.Tests
{
    using System.Data.Entity;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;

    public class FakeDbContextAdapter : DbContextAdapter
    {
        /// <summary>
        /// Sets the current database context to the given entity type.
        /// </summary>
        /// <typeparam name="T">The type of entity to set the context to.</typeparam>
        /// <returns>
        /// The <see cref="IDbSet{T}"/>.
        /// </returns>
        public override IDbSet<T> SetContext<T>()
        {
            return this.GetDbSet<T>();
        }

        /// <summary>
        /// Saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        public override int SaveChanges()
        {
            return default(int);
        }

        /// <summary>
        /// Asynchronously saves all changes made in this context to the underlying database.
        /// </summary>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public override Task<int> SaveChangesAsync()
        {
            return new Task<int>(() => default(int));
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
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return new Task<int>(() => default(int));
        }

        /// <summary>
        /// Gets the correct <see cref="IDbSet{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of entity to return the correct collection for.</typeparam>
        /// <returns>
        /// The <see cref="IDbSet{T}"/>.
        /// </returns>
        private IDbSet<T> GetDbSet<T>() where T : class
        {
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;

            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (PropertyInfo propertyInfo in this.GetType().GetProperties(bindingFlags))
            {
                if (propertyInfo.PropertyType == typeof(IDbSet<T>))
                {
                    return (IDbSet<T>)propertyInfo.GetValue(this, null);
                }
            }

            return null;
        }
    }
}
