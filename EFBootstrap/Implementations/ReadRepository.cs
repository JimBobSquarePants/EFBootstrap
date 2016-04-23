namespace EFBootstrap
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    /// <summary>
    /// Encapsulates methods for retrieving objects from data storage.
    /// </summary>
    /// <typeparam name="T">The type of entity for which to provide the methods.</typeparam>
    public class ReadRepository<T> : IReadRepository<T> where T : class
    {
        /// <summary>
        /// The context representing a combination of Unit of Work and repository patterns.
        /// </summary>
        private readonly IDbContextAdapter context;

        /// <summary>
        /// The database set representing the collection of all entities within a context.
        /// </summary>
        private readonly IDbSet<T> dataSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadRepository{T}"/> class.
        /// </summary>
        /// <param name="context">
        /// The context that can be used to query and store objects to/from the database.
        /// </param>
        public ReadRepository(IDbContextAdapter context)
        {
            this.context = context;
            this.dataSet = context.SetContext<T>();
        }

        /// <summary>
        /// Retrieves all instances of the specified type.
        /// </summary>
        /// <param name="expression">
        /// A strongly typed lambda expression as a date structure
        /// in the form of an expression tree.
        /// </param>
        /// <param name="includeCollection">
        /// An optional parameter array of strongly typed lambda expressions containing details 
        /// of which related entities to eagerly load.
        /// </param>
        /// <returns>The <see cref="IEnumerable{T}"/>.</returns>
        public virtual IEnumerable<T> Select(
            Expression<Func<T, bool>> expression = null,
            params Expression<Func<T, object>>[] includeCollection)
        {
            return this.SelectQueryable(expression, includeCollection).ToList();
        }

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
        public virtual async Task<IEnumerable<T>> SelectAsync(
            Expression<Func<T, bool>> expression = null,
            params Expression<Func<T, object>>[] includeCollection)
        {
            return await this.SelectQueryable(expression, includeCollection).ToListAsync();
        }

        /// <summary>
        /// Creates a raw SQL query that will return entities.
        /// </summary>
        /// <param name="query">The SQL query string.</param>
        /// <param name="parameters">
        /// The parameters to apply to the SQL query string. If output parameters are used, their 
        /// values will not be available until the results have been read completely. 
        /// This is due to the underlying behavior of <see cref="System.Data.Common.DbDataReader"/>, 
        /// see <see href="http://go.microsoft.com/fwlink/?LinkID=398589"/> for more details.
        /// </param>
        /// <returns>
        /// The <see cref="IQueryable"/>.
        /// </returns>
        public virtual IQueryable<T> SelectQuery(string query, params object[] parameters)
        {
            return this.context.SqlQuery<T>(query, parameters).AsNoTracking().AsQueryable();
        }

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
        private IQueryable<T> SelectQueryable(
            Expression<Func<T, bool>> expression = null,
            params Expression<Func<T, object>>[] includeCollection)
        {
            IQueryable<T> query = this.dataSet.AsNoTracking().AsQueryable();

            if (includeCollection.Any())
            {
                query = includeCollection.Aggregate(query, (current, include) => current.Include(include));
            }

            // Check for a filtering expression and pull all if not.
            if (expression != null)
            {
                query = query.Where(expression);
            }

            return query;
        }
    }
}
