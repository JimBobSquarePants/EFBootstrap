namespace EFBootstrap
{
    using System.Data.Common;
    using System.Data.Entity;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Encapsulates properties and methods representing a combination 
    /// of Unit of Work and repository patterns. 
    /// </summary>
    public class DbContextAdapter : DbContext, IDbContextAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextAdapter"/> class.
        /// </summary>
        /// <param name="nameOrConnectionString">
        /// Either the database name or a connection string.
        /// </param>
        public DbContextAdapter(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextAdapter"/> class.
        /// </summary>
        /// <param name="existingConnection">
        /// An existing connection to use for the new context.
        /// </param>
        /// <param name="contextOwnsConnection">
        /// If set to true the connection is disposed when the context is disposed, otherwise
        /// the caller must dispose the connection.
        /// </param>
        public DbContextAdapter(DbConnection existingConnection, bool contextOwnsConnection)
            : base(existingConnection, contextOwnsConnection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextAdapter"/> class.
        /// </summary>
        /// <param name="objectContext">
        /// An existing <see cref="ObjectContext"/> to wrap with the new context.
        /// </param>
        /// <param name="dbContextOwnsObjectContext">
        /// If set to true the <see cref="ObjectContext"/> is disposed when the <see cref="DbContext"/> 
        /// is disposed, otherwise the caller must dispose the connection.
        /// </param>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1305:FieldNamesMustNotUseHungarianNotation", Justification = "Reviewed. Suppression is OK here.")]
        public DbContextAdapter(ObjectContext objectContext, bool dbContextOwnsObjectContext)
            : base(objectContext, dbContextOwnsObjectContext)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextAdapter"/> class.
        /// </summary>
        /// <param name="nameOrConnectionString">
        /// Either the database name or a connection string.
        /// </param>
        /// <param name="model">
        /// The model that will back this context.
        /// </param>
        public DbContextAdapter(string nameOrConnectionString, DbCompiledModel model)
            : base(nameOrConnectionString, model)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextAdapter"/> class.
        /// </summary>
        /// <param name="existingConnection">
        /// An existing connection to use for the new context.
        /// </param>
        /// <param name="model">
        /// The model that will back this context.
        /// </param>
        /// <param name="contextOwnsConnection">
        /// If set to true the connection is disposed when the context is disposed, otherwise
        /// the caller must dispose the connection.
        /// </param>
        public DbContextAdapter(DbConnection existingConnection, DbCompiledModel model, bool contextOwnsConnection)
            : base(existingConnection, model, contextOwnsConnection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextAdapter"/> class.
        /// </summary>
        protected DbContextAdapter()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbContextAdapter"/> class.
        /// </summary>
        /// <param name="model">
        /// The model that will back this context.
        /// </param>
        protected DbContextAdapter(DbCompiledModel model)
            : base(model)
        {
        }

        /// <summary>
        /// Returns a <see cref="IDbSet{T}"/> instance for access to entities of 
        /// the given type in the context and the underlying store.
        /// </summary>
        /// <typeparam name="T">The type of entity to set the context to.</typeparam>
        /// <returns>
        /// The <see cref="IDbSet{T}"/>.
        /// </returns>
        public virtual IDbSet<T> SetContext<T>() where T : class
        {
            return this.Set<T>();
        }

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
        public virtual DbSqlQuery<T> SqlQuery<T>(string query, params object[] parameters) where T : class
        {
            return this.Set<T>().SqlQuery(query, parameters);
        }

        /// <summary>
        /// Gets the state of the object in the data context.
        /// </summary>
        /// <param name="entity">The entity whose state to get.</param>
        /// <returns>
        /// The <see cref="EntityState"/>.
        /// </returns>
        public virtual EntityState GetState(object entity)
        {
            return this.Entry(entity).State;
        }

        /// <summary>
        /// Sets the state of the object in the data context.
        /// </summary>
        /// <param name="entity">The entity whose state to set.</param>
        /// <param name="state">The state of the entity.</param>
        public virtual void SetState(object entity, EntityState state)
        {
            this.Entry(entity).State = state;
        }
    }
}
