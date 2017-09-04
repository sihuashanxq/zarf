using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Data;
using Zarf.Mapping;
using Zarf.Query;
using Zarf.Builders;
using Zarf.Query.Expressions;
using Zarf.Extensions;
using System.Data.SqlClient;

namespace Zarf
{
    public class EntityEnumerable<T> : IEnumerable<T>
    {
        protected EntityProjectionMappingProvider MappingProvider { get; }

        protected ISqlTextBuilder SqlBuilder { get; }

        private Expression _linq;

        private IEnumerator<T> _enumerator;

        private QueryContext _context;

        public EntityEnumerable(Expression linq, EntityProjectionMappingProvider mappingProvider, QueryContext context)
        {
            _linq = linq;
            _context = context;
            MappingProvider = mappingProvider;
            SqlBuilder = new SqlServerTextBuilder();
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (_enumerator == null)
            {
                var sqlCommandText = SqlBuilder.Build(_linq);
                _enumerator = new EntityEnumerator<T>(_linq, sqlCommandText, MappingProvider, _context);
            }

            return _enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (_enumerator == null)
            {
                var sqlCommandText = SqlBuilder.Build(_linq);
                _enumerator = new EntityEnumerator<T>(_linq, sqlCommandText, MappingProvider, _context);
            }

            return _enumerator;
        }
    }

    public class EntityEnumerator<T> : IEnumerator<T>
    {
        private List<T> _cacheItems;

        private IDataReader _dbDataReader;

        private T _current;

        public T Current => _current;

        object IEnumerator.Current => _current;

        private int _currentIndex = 0;

        protected ObjectActivateDelegateFactory DelegateFactory { get; }

        protected Delegate ObjectActivate { get; set; }

        private string _sqlCommandText;

        public EntityEnumerator(
            Expression rootQuery,
            string sqlCommdText,
            EntityProjectionMappingProvider mappingProvider,
            QueryContext context)
        {

            DelegateFactory = new ObjectActivateDelegateFactory(mappingProvider);
            if (rootQuery.Is<QueryExpression>())
            {
                ObjectActivate = DelegateFactory.CreateQueryModelActivateDelegate(rootQuery.Cast<QueryExpression>().Result.EntityNewExpression, rootQuery, context);
            }
            else
            {
                ObjectActivate = DelegateFactory.CreateQueryModelActivateDelegate(rootQuery, rootQuery);
            }

            _sqlCommandText = sqlCommdText;
            _cacheItems = new List<T>();
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
                _dbDataReader = ExecuteSql(_sqlCommandText);
            }

            if (_dbDataReader.Read())
            {
                _current = (T)ObjectActivate.DynamicInvoke(_dbDataReader);
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


        /// <summary>
        /// 暂时 写死 SqlServer  测试用
        /// </summary>
        /// <param name="sqlText"></param>
        /// <returns></returns>
        private IDataReader ExecuteSql(string sqlText)
        {
            var dbConnection = new SqlConnection(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=ORM;Integrated Security=True");
            var dbCommand = new SqlCommand(sqlText, dbConnection);

            dbConnection.Open();
            return dbCommand.ExecuteReader();
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
