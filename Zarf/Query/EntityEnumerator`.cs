using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Zarf.Queries
{
    public class EntityEnumerator<TEntity> : IEnumerator<TEntity>
    {
        private List<TEntity> _cacheItems = new List<TEntity>();

        private IDataReader _dbDataReader;

        private TEntity _current;

        public TEntity Current => _current;

        object IEnumerator.Current => _current;

        private int _currentIndex = 0;

        protected Delegate ObjectActivator { get; }

        protected string CommandText { get; }

        public EntityEnumerator(Delegate activator, IDataReader dataReader)
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
                _current = (TEntity)ObjectActivator.DynamicInvoke(_dbDataReader);
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
