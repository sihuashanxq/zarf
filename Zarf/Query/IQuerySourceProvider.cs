using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Zarf.Query
{
    public interface IQuerySourceProvider
    {
        Expression GetQuerySource(Expression expression);

        /// <summary>
        /// new {User}.Where(item=>item.User.Name)
        /// </summary>
        /// <param name="memberInfo"></param>
        /// <returns></returns>
        Expression GetQuerySource(MemberInfo memberInfo);
    }
}
