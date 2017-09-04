using System;
using System.Linq.Expressions;
using Zarf.Mapping;
using Zarf.Query;
using Zarf.Builders;
using Zarf.Extensions;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Zarf.Query.Expressions;

/// <summary>
/// 临时文件 执行
/// </summary>
namespace Zarf
{
    public interface ILinqExpressionInvoker
    {
        T Invoke<T>(Expression linqExpression);
    }

    public class LinqExpressionInvoker : ILinqExpressionInvoker
    {
        protected EntityProjectionMappingProvider MappingProvider { get; }

        protected IQueryExpressionBuilder QueryExpressionBuilder { get; }

        protected ISqlTextBuilder SqlBuilder { get; }

        protected ObjectActivateDelegateFactory DelegateFactory { get; }

        protected Delegate ObjectActivate { get; set; }

        public LinqExpressionInvoker()
        {
            MappingProvider = new EntityProjectionMappingProvider();
            QueryExpressionBuilder = new QueryExpressionBuilder(MappingProvider);
            DelegateFactory = new ObjectActivateDelegateFactory(MappingProvider);
            SqlBuilder = new SqlServerTextBuilder();
        }

        public LinqExpressionInvoker(EntityProjectionMappingProvider map)
        {
            MappingProvider = map;
            QueryExpressionBuilder = new QueryExpressionBuilder(MappingProvider);
            DelegateFactory = new ObjectActivateDelegateFactory(MappingProvider);
            SqlBuilder = new SqlServerTextBuilder();
        }

        public QType Invoke<QType>(Expression linqExpression)
        {
            var queryContext = QueryContextFacotry.Factory.CreateContext() as QueryContext;
            var rootQuery = QueryExpressionBuilder.Build(linqExpression, queryContext);
            if (rootQuery == null)
            {
                throw new NullReferenceException(nameof(rootQuery));
            }

            if (rootQuery.Is<QueryExpression>())
            {
                ObjectActivate = DelegateFactory.CreateQueryModelActivateDelegate(rootQuery.As<QueryExpression>().Result.EntityNewExpression, rootQuery, queryContext);
            }
            else
            {
                ObjectActivate = DelegateFactory.CreateQueryModelActivateDelegate(rootQuery, rootQuery);
            }

            var sqlText = SqlBuilder.Build(rootQuery);
            if (sqlText.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(sqlText));
            }

            var dbDataReader = ExecuteSql(sqlText);
            var queryType = typeof(QType);

            if (dbDataReader.Read())
            {
                return (QType)ObjectActivate.DynamicInvoke(dbDataReader);
            }

            return default(QType);
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

        public void Add(string sql, Dictionary<string, object> paramemtersValue)
        {
            var dbConnection = new SqlConnection(@"Data Source=localhost\SQLEXPRESS;Initial Catalog=ORM;Integrated Security=True");
            var dbCommand = new SqlCommand(sql, dbConnection);

            dbConnection.Open();

            foreach (var item in paramemtersValue)
            {
                dbCommand.Parameters.AddWithValue(item.Key, item.Value);
            }

            dbCommand.ExecuteNonQuery();
        }
    }
}
