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
using Zarf.Mapping.Bindings;
using Zarf.Query.ExpressionVisitors;
using Zarf.Query.ExpressionTranslators;

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
        protected IEntityProjectionMappingProvider MappingProvider { get; }

        protected ISqlTextBuilder SqlBuilder { get; }

        protected Delegate ObjectActivator { get; set; }

        protected IQueryContext Context { get; set; }

        public LinqExpressionInvoker()
        {
            Context = QueryContextFacotry.Factory.CreateContext();
            MappingProvider = Context.ProjectionMappingProvider;
            SqlBuilder = new SqlServerTextBuilder();
        }

        public QType Invoke<QType>(Expression linqExpression)
        {
            var context = QueryContextFacotry.Factory.CreateContext();
            if (linqExpression.NodeType != ExpressionType.Extension)
            {
                linqExpression = new SqlTranslatingExpressionVisitor(context, NodeTypeTranslatorProvider.Default).Visit(linqExpression);
            }

            if (ObjectActivator == null)
            {
                var binder = new DefaultEntityBinder(context);
                var body = binder.Bind(new BindingContext(linqExpression.As<QueryExpression>().Result?.EntityNewExpression ?? linqExpression));
                ObjectActivator = Expression.Lambda(body, DefaultEntityBinder.DataReader).Compile();
            }

            var sqlText = SqlBuilder.Build(linqExpression);
            if (sqlText.IsNullOrEmpty())
            {
                throw new NullReferenceException(nameof(sqlText));
            }

            var dbDataReader = ExecuteSql(sqlText);
            var queryType = typeof(QType);

            if (dbDataReader.Read())
            {
                return (QType)ObjectActivator.DynamicInvoke(dbDataReader);
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
