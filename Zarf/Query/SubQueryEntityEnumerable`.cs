using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Core;

namespace Zarf.Query
{
    public class EntityPropertyEnumerable<TEntity> : EntityEnumerable<TEntity>
    {
        public EntityPropertyEnumerable(Expression query, IQueryContext context, IQueryExecutor executor)
            : base(query, executor, context)
        {

        }

        public override IEnumerator<TEntity> GetEnumerator()
        {
            return Enumerator ?? (Enumerator = Executor.Execute<TEntity>(Expression, Context));
        }
    }
}
