using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Zarf.Query.Expressions;

namespace Zarf.Query.Internals.ModelTypes
{
    /// <summary>
    /// QueryModel 类型生成器
    /// 查询结果包含引用了外部列的子查询 生成原有类型的子类,记录关联关系
    /// 因此匿名类型需要注意某些时候的子查询无法使用
    /// </summary>
    internal static class QueryModelTypeGenerator
    {
        private static ConcurrentDictionary<string, QueryModelTypeCache> ModelTypeCaches { get; }

        private static ModuleBuilder ModuleBuilder { get; }

        static QueryModelTypeGenerator()
        {
            ModelTypeCaches = new ConcurrentDictionary<string, QueryModelTypeCache>();

            ModuleBuilder = AssemblyBuilder
                .DefineDynamicAssembly(new AssemblyName("__SubQueryExtension__"), AssemblyBuilderAccess.RunAndCollect)
                .DefineDynamicModule("__SubQueryExtension__Module__");
        }

        public static QueryModelTypeDescriptor GenRealtionType(Type modelType, List<ColumnExpression> relatedColumns)
        {
            if (modelType.IsSealed)
            {
                throw new Exception("sealed type not supported filter in an subquery!");
            }

            var typeName = modelType.Name + relatedColumns
                .Select(item => item.Type.GetHashCode())
                .Concat(new[] { modelType.GetHashCode() })
                .Aggregate((hashCode, t) => hashCode + (hashCode * 37 ^ t));

            var typeCache = ModelTypeCaches.GetOrAdd(typeName, (key) =>
            {
                var fields = new Dictionary<string, Type>();
                var typeBuilder = ModuleBuilder.DefineType(
                    typeName,
                    TypeAttributes.Class,
                    modelType);

                for (var i = 0; i < relatedColumns.Count; i++)
                {
                    fields["__RealtionId__" + i] = relatedColumns[i].Type;
                    typeBuilder.DefineField("__RealtionId__" + i, relatedColumns[i].Type, FieldAttributes.Public | FieldAttributes.SpecialName);
                }

                return new QueryModelTypeCache
                {
                    ModelType = typeBuilder.CreateType(),
                    Fields = fields
                };
            });

            var fieldsMap = new Dictionary<string, ColumnExpression>();
            var fieldNames = new HashSet<string>();

            for (var i = 0; i < relatedColumns.Count; i++)
            {
                foreach (var item in typeCache.Fields.Where(kv => !fieldNames.Contains(kv.Key)))
                {
                    if (relatedColumns[i].Type == item.Value)
                    {
                        fieldsMap[item.Key] = relatedColumns[i];
                        fieldNames.Add(item.Key);
                        break;
                    }
                }
            }

            return new QueryModelTypeDescriptor()
            {
                FieldMaps = fieldsMap,
                SubModelType = typeCache.ModelType
            };
        }
    }
}
