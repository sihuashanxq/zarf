using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Core;
using Zarf.Extensions;
using Zarf.SqlServer.Core;
using Zarf.Update;
using Zarf.Update.Commands;
using Zarf.Update.Compilers;
using Zarf.Update.Executors;
using System.Linq;
namespace Zarf
{
    public class SqlServerDbContext : DbContext
    {
        public IDbService DbService { get; }

        public SqlServerDbContext(string connectionString)
        {
            DbService = new SqlServerDbService(connectionString);
        }

        public override int AddRange<TEntity>(IEnumerable<TEntity> entities)
        {
            var entries = new List<EntityEntry>();
            foreach (var entity in entities)
            {
                entries.Add(EntityEntry.Create(entity, EntityState.Insert));
            }

            var modifyOperation = new DbModifyOperation(entries, null);
            var modifyCommand = new CompositeModifyOperationCompiler().Compile(modifyOperation);
            var count = 0;
            foreach (var item in modifyCommand)
            {
                count += new DbInsertCommandExecutor(new SqlServerDataBaseFacade(new SqlServerDbCommandFacade(DbService)), new SqlServer.Builders.SqlServerTextBuilder()).Execute(item.As<DbInsertCommand>());
            }

            return count;
        }

        public override int Update<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            var identityMember = GetIdentityMember(identity);
            var entry = EntityEntry.Create(entity, EntityState.Update);
            var modifyOperation = new DbModifyOperation(new[] { entry }, identityMember);
            var modifyCommand = new CompositeModifyOperationCompiler().Compile(modifyOperation).FirstOrDefault();

            return new DbUpdateCommandExecutor(new SqlServerDataBaseFacade(new SqlServerDbCommandFacade(DbService)), new SqlServer.Builders.SqlServerTextBuilder()).Execute(modifyCommand.As<DbUpdateCommand>());
        }

        public override int Delete<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> identity)
        {
            var identityMember = GetIdentityMember(identity);
            var entry = EntityEntry.Create(entity, EntityState.Delete);
            var modifyOperation = new DbModifyOperation(new[] { entry }, identityMember);
            var modifyCommand = new CompositeModifyOperationCompiler().Compile(modifyOperation).FirstOrDefault();
            return new DbDeleteCommandExecutor(new SqlServerDataBaseFacade(new SqlServerDbCommandFacade(DbService)), new SqlServer.Builders.SqlServerTextBuilder()).Execute(modifyCommand.As<DbDeleteCommand>());
        }

        private static MemberDescriptor GetIdentityMember(Expression identity)
        {
            var member = identity?.As<LambdaExpression>()?.Body?.As<MemberExpression>()?.Member;
            if (identity != null && member == null)
            {
                throw new NotImplementedException("argument keyMember must as a MemberAccessExpression!");
            }

            if (member == null)
            {
                return null;
            }

            return new MemberDescriptor(member);
        }
    }
}
