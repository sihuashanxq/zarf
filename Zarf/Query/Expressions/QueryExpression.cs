using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Entities;
using Zarf.Extensions;
using Zarf.Mapping;

namespace Zarf.Query.Expressions
{
    public class QueryExpression : Expression
    {
        protected Type TypeOfExpression { get; set; }

        public Type MainType { get; }

        public Table Table { get; set; }

        public string Alias { get; }

        public override Type Type => TypeOfExpression;

        public override ExpressionType NodeType => ExpressionType.Extension;

        public List<ColumnDescriptor> Projections { get; }

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

        public QueryExpression(Type typeOfEntity, string alias = "")
        {
            Sets = new List<SetsExpression>();
            Joins = new List<JoinExpression>();
            Orders = new List<OrderExpression>();
            Groups = new List<GroupExpression>();
            Projections = new List<ColumnDescriptor>();
            TypeOfExpression = typeOfEntity;
            MainType = typeOfEntity;
            Alias = alias;
            Table = typeOfEntity.ToTable();
        }

        public QueryExpression PushDownSubQuery(string fromTableAlias, Func<QueryExpression, QueryExpression> subQueryHandle = null)
        {
            var query = new QueryExpression(Type, fromTableAlias)
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

        public void AddJoin(JoinExpression table)
        {
            Joins.Add(table);
        }

        public void AddProjections(IEnumerable<ColumnDescriptor> projections)
        {
            Projections.Clear();
            Projections.AddRange(projections);
        }

        public void AddWhere(Expression predicate)
        {
            if (predicate == null)
            {
                return;
            }

            if (Where == null)
            {
                Where = new WhereExperssion(predicate);
            }
            else
            {
                Where.Combine(predicate);
            }
        }

        /// <summary>
        /// 是否一个空查询
        /// 引用一个Table
        /// </summary>
        /// <returns></returns>
        public bool IsEmptyQuery()
        {
            return
                !IsDistinct &&
                //!DefaultIfEmpty &&
                Where == null &&
                Offset == null &&
                SubQuery == null &&
                Projections.Count == 0 &&
                Orders.Count == 0 &&
                Groups.Count == 0 &&
                Sets.Count == 0 &&
                Joins.Count == 0 &&
                Limit == 0;
        }

        public IEnumerable<ColumnExpression> GenerateTableColumns()
        {
            var typeOfEntity = TypeDescriptorCacheFactory.Factory.Create(Type);
            foreach (var memberDescriptor in typeOfEntity.MemberDescriptors)
            {
                yield return new ColumnExpression(
                    this,
                    memberDescriptor.Member,
                    memberDescriptor.Name);
            }
        }

        public void ChangeTypeOfExpression(Type typeOfExpression)
        {
            TypeOfExpression = typeOfExpression;
        }
    }
}