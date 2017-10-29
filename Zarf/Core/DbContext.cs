using System.Reflection;
using Zarf.Builders;

namespace Zarf
{
    public abstract class DbContext
    {
        public IDbQuery<TEntity> DbQuery<TEntity>()
        {
            return new DbQuery<TEntity>(new DbQueryProvider());
        }

        public static ISqlTextBuilder SqlBuilder { get; set; }

        public abstract void Add<TEntity>(TEntity entity);

        public abstract int Update<TEntity>(TEntity entity);

        public abstract int Delete<TEntity>(TEntity entity);

        public object GetMemberValue(object instance, MemberInfo member)
        {
            if (member is PropertyInfo)
            {
                return (member as PropertyInfo).GetValue(instance);
            }

            return (member as FieldInfo).GetValue(instance);
        }
    }
}
