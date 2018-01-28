using System.Collections.Generic;
using System.Linq.Expressions;
using Zarf.Metadata.Entities;

namespace Zarf.Generators
{
    public interface ISQLGenerator
    {
        /// <summary>
        /// 生成SQL
        /// </summary>
        /// <param name="exp">表达式</param>
        /// <param name="parameters">参数集</param>
        string Generate(Expression exp, List<DbParameter> parameters);

        /// <summary>
        /// 将Expression生成SQL,并附加到当前SQL
        /// </summary>
        void Attach(Expression exp);

        /// <summary>
        /// 附加SQL到当前SQL中
        /// </summary>
        void Attach(string text);
    }
}
