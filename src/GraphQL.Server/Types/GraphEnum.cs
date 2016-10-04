using System;
using System.Linq;
using GraphQL.Types;

namespace GraphQL.Server.Types
{
    public class GraphEnum<T> : EnumerationGraphType
    {
        public GraphEnum()
        {
            Name = typeof (T).Name;
            AddAllValues();
        }

        public void AddAllValues()
        {
            foreach (var value in Enum.GetValues(typeof(T)).OfType<T>())
            {
                AddValue(value);
            }
        }
        public void AddValue(T value)
        {
            AddValue(Enum.GetName(typeof(T), value), "", value);
        }
    }
}