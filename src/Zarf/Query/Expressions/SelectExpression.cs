using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Zarf.Extensions;
using Zarf.Infrastructure;
using Zarf.Metadata.Descriptors;
using Zarf.Metadata.Entities;
using Zarf.Query.Mappers;

namespace Zarf.Query.Expressions
{
    public class SelectExpression : Expression
    {
        public string Alias { get; set; }

        public Table Table { get; set; }

        public Type EntityModelType { get; set; }

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

        public SelectExpression SubSelect { get; set; }

        public SelectExpression SourceSelect { get; set; }

        public SelectExpression OuterSelect { get; set; }

        public QueryEntityModel QueryModel { get; set; }

        public IMapper<Expression, Expression> Mapper { get; }

        /// <summary>
        /// is in of a prediacte ,such as where (select top 1 1 id from [user])=1
        /// </summary>
        public bool IsInPredicate { get; internal set; }

        protected IEqualityComparer<Expression> ExpressionComparer { get; }

        public SelectExpression(Type entityModelType, IMapper<Expression, Expression> mapper, string alias = "")
        {
            EntityModelType = entityModelType;
            Table = entityModelType.ToTable();
            Mapper = mapper;
            Alias = alias;
            Sets = new List<SetsExpression>();
            Joins = new List<JoinExpression>();
            Orders = new List<OrderExpression>();
            Groups = new List<GroupExpression>();
            Projections = new List<Expression>();
            ExpressionComparer = new ExpressionEqualityComparer();
        }

        public SelectExpression PushDownSubQuery(string alias)
        {
            var select = new SelectExpression(Type, Mapper, alias)
            {
                SubSelect = this,
                Table = null,
                DefaultIfEmpty = DefaultIfEmpty,
                QueryModel = QueryModel
            };

            select.AddProjectionRange(select.GenSelectProjections());
            select.QueryModel.Select = select;

            DefaultIfEmpty = false;
            OuterSelect = select;
            SourceSelect = select;

            return select;
        }

        public void AddJoin(JoinExpression joinQuery)
        {
            joinQuery.Select.OuterSelect = this;
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
            var exists = FindExistsProjection(exp);
            if (exists != null)
            {
                if (exists != exp || !ExpressionComparer.Equals(exp, exists))
                {
                    Mapper.Map(exp, exists);
                }

                return;
            }

            Projections.Add(exp);
        }

        protected Expression FindExistsProjection(Expression exp)
        {
            var aliasExpresion = exp.As<AliasExpression>()?.Expression;

            foreach (var item in Projections)
            {
                if (item == exp || ExpressionComparer.Equals(item, exp))
                {
                    return item;
                }

                if (item is AliasExpression alias)
                {
                    if (alias.Expression == exp || ExpressionComparer.Equals(alias.Expression, exp))
                    {
                        return alias;
                    }

                    if (aliasExpresion != null)
                    {
                        if (alias.Expression == aliasExpresion || ExpressionComparer.Equals(alias.Expression, aliasExpresion))
                        {
                            return alias;
                        }
                    }
                }
            }

            return null;
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
            var isEmpty =
                !IsDistinct &&
                Where == null &&
                Offset == null &&
                (SubSelect?.IsEmptyQuery() ?? true) &&
                Orders.Count == 0 &&
                Groups.Count == 0 &&
                Sets.Count == 0 &&
                Limit == 0;

            if (!isEmpty)
            {
                foreach (var item in Joins)
                {
                    if (!item.Select.IsEmptyQuery())
                    {
                        return false;
                    }
                }
            }

            return isEmpty;
        }

        public IEnumerable<Expression> GenSelectProjections()
        {
            var cols = new List<Expression>();
            if (SubSelect == null)
            {
                var modeTypeDescriptor = TypeDescriptorCacheFactory.Factory.Create(EntityModelType);

                foreach (var memberDescriptor in modeTypeDescriptor.MemberDescriptors)
                {
                    cols.Add(new ColumnExpression(this, memberDescriptor.Member));
                }

                foreach (var item in Joins.Select(item => item.Select))
                {
                    cols.AddRange(item.GenSelectProjections());
                }

                return cols;
            }

            foreach (var item in SubSelect.Projections)
            {
                ColumnExpression col = null;
                if (item.Is<AliasExpression>())
                {
                    col = new ColumnExpression(this, new Column(item.As<AliasExpression>().Alias), item.Type);
                }

                else if (item.Is<ColumnExpression>())
                {
                    col = item.As<ColumnExpression>().Clone();
                    col.Select = this;
                }

                if (item.Is<AggregateExpression>())
                {
                    col = new ColumnExpression(this, new Column(item.As<AggregateExpression>().Alias), item.Type);
                }

                if (col == null)
                {
                    continue;
                }

                Mapper.Map(item, col);
                //Mapper.Map(col, item);
                cols.Add(col);
            }

            return cols;
        }

        public SelectExpression Clone()
        {
            var select = new SelectExpression(EntityModelType, Mapper, Alias);

            select.Orders.AddRange(Orders.ToList());
            select.Groups.AddRange(Groups.ToList());
            select.Joins.AddRange(Joins.ToList());
            select.Sets.AddRange(Sets.ToList());
            select.SourceSelect = this;
            select.Limit = Limit;
            select.IsDistinct = IsDistinct;
            select.SubSelect = SubSelect?.Clone();
            select.QueryModel = QueryModel;
            select.Where = Where == null ? Where : new WhereExperssion(Where.Predicate);

            return select;
        }

        public bool ContainsSelectExpression(SelectExpression select)
        {
            if (select == this)
            {
                return true;
            }

            foreach (var item in Joins.Select(_ => _.Select))
            {
                if (item.ContainsSelectExpression(select))
                {
                    return true;
                }
            }

            return SubSelect?.ContainsSelectExpression(select) ?? false;
        }
    }
}