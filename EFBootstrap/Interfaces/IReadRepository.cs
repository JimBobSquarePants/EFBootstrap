namespace EFBootstrap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Encapsulates methods for retrieving objects from data storage.
    /// </summary>
    /// <typeparam name="T">The type of entity for which to provide the methods.</typeparam>
    public interface IReadRepository<T> where T : class
    {
        /// <summary>
        /// Retrieves all instances of the specified type.
        /// </summary>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <param name="includeCollection">
        /// An optional parameter array of strongly typed lambda expressions containing details of 
        /// which related entities to eagerly load.
        /// </param>
        /// <returns>The <see cref="IEnumerable{T}"/>.</returns>
        IEnumerable<T> Select(
            Expression<Func<T, bool>> expression = null,
            params Expression<Func<T, object>>[] includeCollection);

        /// <summary>
        /// Asynchronously retrieves all instances of the specified type.
        /// </summary>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <param name="includeCollection">
        /// An optional parameter array of strongly typed lambda expressions containing details of 
        /// which related entities to eagerly load.
        /// </param>
        /// <returns>The <see cref="IEnumerable{T}"/>.</returns>
        Task<IEnumerable<T>> SelectAsync(
            Expression<Func<T, bool>> expression = null,
            params Expression<Func<T, object>>[] includeCollection);

        /// <summary>
        /// Creates a raw SQL query that will return entities.
        /// </summary>
        /// <param name="query">The SQL query string.</param>
        /// <param name="parameters">
        /// The parameters to apply to the SQL query string. If output parameters are used, their values 
        /// will not be available until the results have been read completely. 
        /// This is due to the underlying behavior of <see cref="System.Data.Common.DbDataReader"/>, 
        /// see <see href="http://go.microsoft.com/fwlink/?LinkID=398589"/> for more details.
        /// </param>
        /// <returns>
        /// The <see cref="IQueryable"/>.
        /// </returns>
        IQueryable<T> SelectQuery(string query, params object[] parameters);
    }
}
