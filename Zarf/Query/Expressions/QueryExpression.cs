using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Mapping;
using Zarf.Query.Internals;

namespace Zarf.Query.Expressions
{
    public class QueryExpression : Expression
    {
        protected Type TypeOfExpression { get; set; }

        public Table Table { get; set; }

        public string Alias { get; }

        public override Type Type => TypeOfExpression;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public List<ColumnDescriptor> Columns { get; }

        public List<Expression> Projections { get; }

        public List<JoinExpression> Joins { get; }

        public List<SetsExpression> Sets { get; }

        public List<OrderExpression> Orders { get; }

        public List<GroupExpression> Groups { get; }

        public WhereExperssion Where { get; set; }

        public bool IsDistinct { get; set; }

        public int Limit { get; set; }

        public SkipExpression Offset { get; set; }

        public bool DefaultIfEmpty { get; set; }

        public QueryExpression SubQuery { get; protected set; }

        public QueryExpression Container { get; protected set; }

        public EntityResult Result { get; set; }

        public IQueryColumnCaching ColumnCaching { get; }

        protected HashSet<string> ColumnAliases { get; }

        /// <summary>
        /// is part of a prediacte ,such as where (select top 1 1 id from [user])=1
        /// </summary>
        public bool IsPartOfPredicate { get; internal set; }

        public QueryExpression(Type typeOfEntity, IQueryColumnCaching columnCaching, string alias = "")
        {
            Sets = new List<SetsExpression>();
            Joins = new List<JoinExpression>();
            Orders = new List<OrderExpression>();
            Groups = new List<GroupExpression>();
            Columns = new List<ColumnDescriptor>();
            Projections = new List<Expression>();
            ColumnAliases = new HashSet<string>();
            TypeOfExpression = typeOfEntity;
            Table = typeOfEntity.ToTable();
            ColumnCaching = columnCaching;
            Alias = alias;
        }

        public QueryExpression PushDownSubQuery(string alias, Func<QueryExpression, QueryExpression> subQueryHandle = null)
        {
            var query = new QueryExpression(Type, ColumnCaching, alias)
            {
                SubQuery = this,
                Table = null,
                DefaultIfEmpty = DefaultIfEmpty,
            };

            DefaultIfEmpty = false;
            Container = query;
            query.Result = query.SubQuery.Result;
            return subQueryHandle != null ? subQueryHandle(query) : query;
        }

        public void AddJoin(JoinExpression joinQuery)
        {
            joinQuery.Query.Container = this;
            Joins.Add(joinQuery);
        }

        public void AddColumns(IEnumerable<ColumnDescriptor> columns)
        {
            foreach (var item in columns)
            {
                var col = item.Expression.As<ColumnExpression>()?.Clone();
                if (col == null)
                {
                    continue;
                }

                while (ColumnAliases.Contains(col.Alias))
                {
                    col.Alias = col.Alias + "_1";
                }

                item.Expression = col;
                ColumnAliases.Add(col.Alias);
                ColumnCaching.AddColumn(col);
            }

            Columns.AddRange(columns);
        }

        public void AddProjection(Expression exp)
        {
            Projections.Add(exp);
        }

        public void RedirectColumnQuery(ColumnExpression col)
        {
            if (col.Query == this)
            {
                return;
            }

            if (col.Query == SubQuery)
            {
                return;
            }

            foreach (var item in Joins)
            {
                if (col.Query == item.Query)
                {
                    return;
                }
            }

            if (col.Query.Container == null)
            {
                return;
            }

            col.Query = col.Query.Container;
            RedirectColumnQuery(col);
        }

        public void CombineCondtion(Expression predicate)
        {
            if (predicate == null)
            {
                return;
            }

            if (Where == null)
            {
                Where = new WhereExperssion(predicate);
                return;
            }

            Where.Combine(predicate);
        }

        /// <summary>
        /// 是否一个空查询
        /// 引用一个Table
        /// </summary>
        /// <returns></returns>
        public bool IsEmptyQuery()
        {
            var isEmpty = !IsDistinct && Where == null &&
                Offset == null && SubQuery == null &&
                Orders.Count == 0 && Groups.Count == 0 &&
                Sets.Count == 0 && Limit == 0;

            if (!isEmpty)
            {
                foreach (var item in Joins)
                {
                    if (!item.Query.IsEmptyQuery())
                    {
                        return false;
                    }
                }
            }

            return isEmpty;
        }

        public IEnumerable<Expression> GenerateTableColumns()
        {
            if (Columns.Count != 0)
            {
                return Columns.Select(item => item.Expression);
            }

            var typeOfEntity = TypeDescriptorCacheFactory.Factory.Create(Type);
            var cols = new List<Expression>();

            foreach (var memberDescriptor in typeOfEntity.MemberDescriptors)
            {
                var col = new ColumnExpression(this, memberDescriptor.Member, memberDescriptor.Name);
                cols.Add(col);
            }

            foreach (var item in Joins.Select(item => item.Query))
            {
                cols.AddRange(item.Columns.Select(a => a.Expression));
            }

            return cols;
        }

        public void ChangeTypeOfExpression(Type typeOfExpression)
        {
            TypeOfExpression = typeOfExpression;
        }
    }
}