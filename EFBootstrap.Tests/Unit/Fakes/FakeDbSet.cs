namespace EFBootstrap.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity;
    using System.Linq;

    public class FakeDbSet<T> : IDbSet<T>
        where T : class
    {
        ObservableCollection<T> data;
        IQueryable query;

        public FakeDbSet()
        {
            this.data = new ObservableCollection<T>();
            this.query = this.data.AsQueryable();
        }

        public virtual T Find(params object[] keyValues)
        {
            throw new NotImplementedException("Derive from FakeDbSet<T> and override Find");
        }

        /// <summary>
        /// Adds the given entity to the context underlying the set in the Added state such 
        /// that it will be inserted into the database when SaveChanges is called.
        /// </summary>
        /// <param name="entity">The entity to add. </param>
        /// <returns>The entity.</returns>
        /// <remarks>
        /// Note that entities that are already in the context in some other state will have their 
        /// state set to Added.  Add is a no-op if the entity is already in the context in the 
        /// Added state.
        /// </remarks>
        public T Add(T entity)
        {
            this.data.Add(entity);
            return entity;
        }

        /// <summary>
        /// Marks the given entity as Deleted such that it will be deleted from the database when 
        /// SaveChanges is called.  Note that the entity must exist in the context in some 
        /// other state before this method is called.
        /// </summary>
        /// <param name="entity">The entity to remove. </param>
        /// <returns>
        /// The entity. 
        /// </returns>
        /// <remarks>
        /// Note that if the entity exists in the context in the Added state, then this method
        /// will cause it to be detached from the context.  This is because an Added entity is 
        /// assumed not to exist in the database such that trying to delete it does not make sense.
        /// </remarks>
        public T Remove(T entity)
        {
            this.data.Remove(entity);
            return entity;
        }

        public T Attach(T item)
        {
            this.data.Add(item);
            return item;
        }

        public T Detach(T item)
        {
            this.data.Remove(item);
            return item;
        }

        public T Create()
        {
            return Activator.CreateInstance<T>();
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
        {
            return Activator.CreateInstance<TDerivedEntity>();
        }

        public ObservableCollection<T> Local
        {
            get { return this.data; }
        }

        Type IQueryable.ElementType
        {
            get { return this.query.ElementType; }
        }

        System.Linq.Expressions.Expression IQueryable.Expression
        {
            get { return this.query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return this.query.Provider; }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to 
        /// iterate through the collection.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.data.GetEnumerator();
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to 
        /// iterate through the collection.
        /// </returns>
        /// <filterpriority>1</filterpriority>
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.data.GetEnumerator();
        }
    }
}
