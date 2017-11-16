using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Core;

namespace Zarf.Query
{
    public class EntityPropertyEnumerable<TEntity> : EntityEnumerable<TEntity>
    {
        private IMemberValueCache _memValueCache;

        private IDbContextParts _dbContextParts;

        public EntityPropertyEnumerable(Expression query, IMemberValueCache memValueCache, IDbContextParts dbContextParts)
            : base(query, dbContextParts)
        {
            _memValueCache = memValueCache;
            _dbContextParts = dbContextParts;
        }

        public override IEnumerator<TEntity> GetEnumerator()
        {
            return Enumerator ?? (Enumerator = Interpreter.Execute<TEntity>(Expression, QueryContextFacotry.Factory.CreateContext(memValue: _memValueCache, dbContextParts: _dbContextParts)));
        }
    }
}
