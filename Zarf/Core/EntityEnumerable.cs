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
using Zarf.Query.ExpressionVisitors;
using Zarf.Query.ExpressionTranslators;

namespace Zarf
{
    public class EntityPropertyEnumerable<TEntity> : EntityEnumerable<TEntity>
    {
        private IMemberValueCache _memValueCache;

        public EntityPropertyEnumerable(Expression linq, IMemberValueCache memValueCache)
            : base(linq)
        {
            _memValueCache = memValueCache;
        }

        protected override IQueryContext CreateQueryContext()
        {
            return QueryContextFacotry.Factory.CreateContext(memValue: _memValueCache);
        }
    }

    public class EntityEnumerable<TEntity> : IEnumerable<TEntity>
    {
        protected ISqlTextBuilder SqlBuilder { get; }

        protected Expression Expression { get; set; }

        protected IEnumerator<TEntity> Enumerator { get; set; }

        protected Delegate ObjectActivator { get; set; }

        protected IBinder Binder { get; set; }

        protected string CommandText { get; set; }

        protected IQueryContext Context { get; set; }

        public EntityEnumerable(Expression linq)
        {
            Expression = linq;
            SqlBuilder = new SqlServerTextBuilder();
            Context = CreateQueryContext();
            Binder = new DefaultEntityBinder(Context);
        }

        protected virtual IQueryContext CreateQueryContext()
        {
            return QueryContextFacotry.Factory.CreateContext();
        }

        public virtual IEnumerator<TEntity> GetEnumerator()
        {
            if (Enumerator == null)
            {
                if (Expression.NodeType != ExpressionType.Extension)
                {
                    Expression = new SqlTranslatingExpressionVisitor(Context, NodeTypeTranslatorProvider.Default).Visit(Expression);
                }

                if (ObjectActivator == null)
                {
                    var body = Binder.Bind(new BindingContext(Expression.As<QueryExpression>().Result?.EntityNewExpression ?? Expression));
                    ObjectActivator = Expression.Lambda(body, DefaultEntityBinder.DataReader).Compile();
                }

                CommandText = SqlBuilder.Build(Expression);
                Enumerator = new EntityEnumerator<TEntity>(ObjectActivator, CommandText);
            }

            return Enumerator;
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

        protected Delegate ObjectActivator { get; }

        protected string CommandText { get; }

        public EntityEnumerator(Delegate activator, string commdText)
        {
            ObjectActivator = activator;
            CommandText = commdText;
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
                _dbDataReader = ExecuteSql(CommandText);
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
