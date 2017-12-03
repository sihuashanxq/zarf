using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Zarf.Extensions;
using Zarf.Query.Expressions;

namespace Zarf.Query
{
    public class EntityMemberSourceMappingProvider : IEntityMemberSourceMappingProvider
    {
        protected virtual Dictionary<MemberInfo, Expression> MemberExpressions { get; set; }

        public EntityMemberSourceMappingProvider()
        {
            MemberExpressions = new Dictionary<MemberInfo, Expression>();
        }

        public Expression GetExpression(MemberInfo memberInfo)
        {
            if (IsMapped(memberInfo))
            {
                return MemberExpressions[memberInfo];
            }

            return null;
        }

        public bool IsMapped(MemberInfo memberInfo)
        {
            return MemberExpressions.ContainsKey(memberInfo);
        }

        public void Map(MemberInfo memberInfo, Expression mapExpression)
        {
            MemberExpressions[memberInfo] = mapExpression;
        }

        public void UpdateExpression(Expression oMappedExpression, Expression nMappedExpression)
        {
            var exps = MemberExpressions;
            MemberExpressions = new Dictionary<MemberInfo, Expression>();

            foreach (var item in exps)
            {
                if (item.Value == oMappedExpression ||
                    oMappedExpression.Equals(item.Value.As<QueryExpression>().Container))
                {
                    MemberExpressions[item.Key] = nMappedExpression;
                    continue;
                }

                MemberExpressions[item.Key] = item.Value;
            }
        }
    }
}
