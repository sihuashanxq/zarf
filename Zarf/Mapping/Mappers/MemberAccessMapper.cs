using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Zarf.Query
{
    public class MemberAccessMapper : IMemberAccessMapper
    {
        protected virtual Dictionary<MemberInfo, Expression> MemberExpressions { get; set; }

        public MemberAccessMapper()
        {
            MemberExpressions = new Dictionary<MemberInfo, Expression>();
        }

        public Expression GetMappedExpression(MemberInfo memberInfo)
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
    }
}
