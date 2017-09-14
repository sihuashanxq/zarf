using System.Data;

namespace Zarf.Mapping
{
    public interface IEntityMemberValueProvider
    {
        object GetValue(IDataReader dataReader);
    }
}
