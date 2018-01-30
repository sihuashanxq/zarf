namespace Zarf.Metadata.Entities
{
    public class DbParameter
    {
        public object Value { get; }

        public string Name { get; }

        public DbParameter(string name, object value)
        {
            Value = value;
            Name = name;
        }
    }
}
