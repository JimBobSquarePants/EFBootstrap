namespace EFBootstrap
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Encapsulates methods for retrieving objects from and persisting to data storage.
    /// </summary>
    /// <typeparam name="T">The type of entity for which to provide the methods.</typeparam>
    public interface IReadWriteRepository<T> : IReadRepository<T> where T : class
    {
        /// <summary>
        /// Adds a single instance of the specified type.
        /// </summary>
        /// <param name="item">The instance of the given type to add.</param>
        void Add(T item);

        /// <summary>
        /// Adds a collection of instances of the specified type.
        /// </summary>
        /// <param name="items">A collection of items of the given type to add.</param>
        void AddRange(IEnumerable<T> items);

        /// <summary>
        /// Updates an instance of the specified type.
        /// </summary>
        /// <param name="item">The instance of the given type to add.</param>
        void Update(T item);

        /// <summary>
        /// Deletes a single instance of the specified type.
        /// </summary>
        /// <param name="item">The instance of the type to delete.</param>
        void Delete(T item);

        /// <summary>
        /// Deletes all instances of the specified type that match the query.
        /// </summary>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        void Delete(Expression<Func<T, bool>> expression);

        /// <summary>
        /// Deletes all instances of the specified type that match the query asynchronously.
        /// </summary>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        Task DeleteAsync(Expression<Func<T, bool>> expression);
    }
}
