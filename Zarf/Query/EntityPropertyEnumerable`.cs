using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Core;

namespace Zarf.Queries
{
    public class EntityPropertyEnumerable<TEntity> : EntityEnumerable<TEntity>
    {
        public EntityPropertyEnumerable(Expression query, IQueryContext context, IDbContextParts dbContextParts)
            : base(query, dbContextParts, context)
        {
  
        }

        public override IEnumerator<TEntity> GetEnumerator()
        {
            return Enumerator ?? (Enumerator = Interpreter.Execute<TEntity>(Expression, Context));
        }
    }
}
