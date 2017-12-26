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
        public Type TypeOfExpression { get; set; }

        public Table Table { get; set; }

        public string Alias { get; set; }

        public override Type Type => typeof(object);

        public override ExpressionType NodeType => ExpressionType.Extension;

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

        public QueryExpression Outer { get; protected set; }

        public QueryEntityModel QueryModel { get; set; }

        public IQueryProjectionMapper ExpressionMapper { get; }

        /// <summary>
        /// is part of a prediacte ,such as where (select top 1 1 id from [user])=1
        /// </summary>
        public bool IsPartOfPredicate { get; internal set; }

        public QueryExpression(Type typeOfEntity, IQueryProjectionMapper mapper, string alias = "")
        {
            Sets = new List<SetsExpression>();
            Joins = new List<JoinExpression>();
            Orders = new List<OrderExpression>();
            Groups = new List<GroupExpression>();
            Projections = new List<Expression>();
            TypeOfExpression = typeOfEntity;
            Table = typeOfEntity.ToTable();
            ExpressionMapper = mapper;
            Alias = alias;
        }

        public QueryExpression PushDownSubQuery(string alias)
        {
            var query = new QueryExpression(Type, ExpressionMapper, alias)
            {
                SubQuery = this,
                Table = null,
                DefaultIfEmpty = DefaultIfEmpty,
                QueryModel = QueryModel
            };

            DefaultIfEmpty = false;
            Outer = query;
            query.AddProjectionRange(query.GenQueryProjections());
            return query;
        }

        public void AddJoin(JoinExpression joinQuery)
        {
            joinQuery.Query.Outer = this;
            Joins.Add(joinQuery);
        }

        public void AddProjectionRange(IEnumerable<Expression> exps)
        {
            foreach (var item in exps)
            {
                AddProjection(item);
            }
        }

        public void AddProjection(Expression exp)
        {
            foreach (var item in Projections)
            {
                if (new ExpressionEqualityComparer().Equals(item, exp))
                {
                    return;
                }
            }

            Projections.Add(exp);
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
                Offset == null && (SubQuery?.IsEmptyQuery() ?? true) &&
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

        public IEnumerable<Expression> GenQueryProjections()
        {
            var cols = new List<Expression>();
            if (SubQuery == null)
            {
                var typeOfEntity = TypeDescriptorCacheFactory.Factory.Create(TypeOfExpression);

                foreach (var memberDescriptor in typeOfEntity.MemberDescriptors)
                {
                    cols.Add(new ColumnExpression(this, memberDescriptor.Member));
                }

                foreach (var item in Joins.Select(item => item.Query))
                {
                    cols.AddRange(item.GenQueryProjections());
                }

                return cols;
            }

            foreach (var item in SubQuery.Projections)
            {
                ColumnExpression col = null;
                if (item.Is<AliasExpression>())
                {
                    col = new ColumnExpression(this, new Column(item.As<AliasExpression>().Alias), item.Type);
                }

                else if (item.Is<ColumnExpression>())
                {
                    col = item.As<ColumnExpression>().Clone();
                    col.Query = this;
                }

                if (item.Is<AggregateExpression>())
                {
                    col = new ColumnExpression(this, new Column(item.As<AggregateExpression>().Alias), item.Type);
                }

                if (col == null)
                {
                    continue;
                }
                //拆分
                ExpressionMapper.Map(item, col);
                ExpressionMapper.Map(col, item);
                cols.Add(col);
            }

            return cols;
        }

        public QueryExpression Clone()
        {
            var query = new QueryExpression(TypeOfExpression, ExpressionMapper, Alias);
            query.Orders.AddRange(Orders);
            query.Groups.AddRange(Groups);
            query.Joins.AddRange(query.Joins);
            query.Limit = Limit;
            query.IsDistinct = IsDistinct;
            query.SubQuery = SubQuery?.Clone();
            query.Where = Where == null ? Where : new WhereExperssion(Where.Predicate);
            return query;
        }

        public bool ConstainsQuery(QueryExpression subQuery)
        {
            if (subQuery == this)
            {
                return true;
            }

            foreach (var item in Joins.Select(_ => _.Query))
            {
                if (item.ConstainsQuery(subQuery))
                {
                    return true;
                }
            }

            return SubQuery?.ConstainsQuery(subQuery) ?? false;
        }

        public void ChangeTypeOfExpression(Type typeOfExpression)
        {
            TypeOfExpression = typeOfExpression;
        }
    }
}