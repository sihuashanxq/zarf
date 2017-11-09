using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using Zarf.Core;
using Zarf.Query;

namespace Zarf
{
    public class EntityPropertyEnumerable<TEntity> : EntityEnumerable<TEntity>
    {
        private IMemberValueCache _memValueCache;

        private IDbContextParts _dbContextParts;

        public EntityPropertyEnumerable(Expression query, IMemberValueCache memValueCache, IDbContextParts dbContextParts)
            : base(query, dbContextParts)
        {
            _memValueCache = memValueCache;
            _dbContextParts = dbContextParts;
        }

        public override IEnumerator<TEntity> GetEnumerator()
        {
            return Enumerator ?? (Enumerator = Interpreter.Execute<TEntity>(Expression, QueryContextFacotry.Factory.CreateContext(memValue: _memValueCache, dbContextParts: _dbContextParts)));
        }
    }

    public class EntityEnumerable<TEntity> : IEnumerable<TEntity>
    {
        protected IEnumerator<TEntity> Enumerator { get; set; }

        protected Expression Expression { get; }

        protected IQueryInterpreter Interpreter { get; }

        public EntityEnumerable(Expression query, IDbContextParts dbContextParts)
        {
            Expression = query;
            Interpreter = new QueryInterpreter(dbContextParts);
        }

        public virtual IEnumerator<TEntity> GetEnumerator()
        {
            return Enumerator ?? (Enumerator = Interpreter.Execute<TEntity>(Expression));
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<TEntity>)this).GetEnumerator();
        }
    }

    public class EntityEnumerator<TEntity> : IEnumerator<TEntity>
    {
        private List<TEntity> _cacheItems = new List<TEntity>();

        private IDataReader _dbDataReader;

        private TEntity _current;

        public TEntity Current => _current;

        object IEnumerator.Current => _current;

        private int _currentIndex = 0;

        protected Func<IDataReader, TEntity> ObjectActivator { get; }

        protected string CommandText { get; }

        public EntityEnumerator(Func<IDataReader, TEntity> activator, IDataReader dataReader)
        {
            ObjectActivator = activator;
            _dbDataReader = dataReader;
        }

        public bool MoveNext()
        {
            if (_currentIndex > _cacheItems.Count)
            {
                return false;
            }

            if (_currentIndex < _cacheItems.Count)
            {
                _current = _cacheItems[_currentIndex];
                _currentIndex++;
                return true;
            }

            if (_dbDataReader == null || _dbDataReader.IsClosed)
            {
                return false;
            }

            if (_dbDataReader.Read())
            {
                _current = ObjectActivator(_dbDataReader);
                _currentIndex++;
                _cacheItems.Add(_current);
                return true;
            }

            _dbDataReader.Close();
            return false;
        }

        public void Reset()
        {
            _currentIndex = 0;
        }

        public void Dispose()
        {
            if (_dbDataReader != null && !_dbDataReader.IsClosed)
            {
                _dbDataReader.Close();
            }

            _currentIndex = 0;
        }
    }
}
