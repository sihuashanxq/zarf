using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

namespace Zarf.Query
{
    public class DbMemoryEnumerator<TEntity> : IEnumerator<TEntity>
    {
        private TEntity _current;

        public TEntity Current => _current;

        object IEnumerator.Current => _current;

        private int _index = 0;

        protected List<TEntity> ElementItems { get; }

        protected Delegate ElementCreator { get; }

        public DbMemoryEnumerator(Delegate elementCreator, IDataReader dbDataReader)
        {
            ElementCreator = elementCreator;
            ElementItems = new List<TEntity>();
            CopyDataToMemory(dbDataReader);
        }

        protected virtual void CopyDataToMemory(IDataReader dbDataReader)
        {
            while (dbDataReader.Read())
            {
                ElementItems.Add((TEntity)ElementCreator.DynamicInvoke(dbDataReader));
            }
        }

        public bool MoveNext()
        {
            if (_index >= ElementItems.Count)
            {
                return false;
            }

            _current = ElementItems[_index];
            _index++;

            return true;
        }

        public void Reset()
        {
            _index = 0;
        }

        public void Dispose()
        {
            //重置,否则子查询过滤无数据
            Reset();
        }
    }
}
