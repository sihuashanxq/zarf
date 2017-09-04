using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

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

        public void UpdateExpression(Expression oldMapExpression, Expression newMapExpression)
        {
            foreach (var item in MemberExpressions)
            {
                if (item.Value == oldMapExpression)
                {
                    MemberExpressions[item.Key] = newMapExpression;
                }
            }
        }
    }
}
