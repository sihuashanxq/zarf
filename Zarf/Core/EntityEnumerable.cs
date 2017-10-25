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
using Zarf.Mapping.Bindings;

namespace Zarf
{
    public class SubEntityEnumerable<T> : EntityEnumerable<T>
    {
        private IQueryContext _context;

        public SubEntityEnumerable(Expression linq, QueryContext context)
            : base(linq)
        {
            _context = context;
        }

        protected override IQueryContext CreateQueryContext()
        {
            return _context;
        }
    }

    public class EntityEnumerable<T> : IEnumerable<T>
    {
        protected ISqlTextBuilder SqlBuilder { get; }

        protected Expression Expression { get; set; }

        protected IEnumerator<T> Enumerator { get; set; }

        public EntityEnumerable(Expression linq)
        {
            Expression = linq;
            SqlBuilder = new SqlServerTextBuilder();
        }

        protected virtual IQueryContext CreateQueryContext()
        {
            return QueryContextFacotry.Factory.CreateContext();
        }

        public virtual IEnumerator<T> GetEnumerator()
        {
            if (Enumerator == null)
            {
                var context = CreateQueryContext();

                if (Expression.NodeType != ExpressionType.Extension)
                {
                    Expression = new LinqExpressionTanslator(context).Build(Expression, context);
                }

                var sqlCommandText = SqlBuilder.Build(Expression);
                Enumerator = new EntityEnumerator<T>(Expression, sqlCommandText, context);
            }

            return Enumerator;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<T>)this).GetEnumerator();
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

        protected IBinder Binder { get; }

        private string _sqlCommandText;

        public EntityEnumerator(
            Expression rootQuery,
            string sqlCommdText,
            IQueryContext context)
        {
            DelegateFactory = new ObjectActivateDelegateFactory();
            Binder = new DefaultEntityBinder(context.ProjectionMappingProvider, context.PropertyNavigationContext, context, rootQuery);

            var body = Binder.Bind(
                new BindingContext(
                    rootQuery.Type.GetCollectionElementType(),
                    rootQuery.As<QueryExpression>().Result?.EntityNewExpression ?? rootQuery)
                    );

            ObjectActivate = Expression.Lambda(body, DefaultEntityBinder.DataReader)
                .Compile();

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
