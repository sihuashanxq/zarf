using System.Data;

namespace Zarf.Mapping
{
    public interface IObjectActivator
    {
        object CreateInstance(IDataReader dataReader);
    }
}
