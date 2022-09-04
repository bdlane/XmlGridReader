using System;
using System.Collections.Generic;
using System.Linq;

namespace XmlGridReader
{
    public static partial class Reader
    {
        private class TypeDeserializerCache
        {
            private static readonly Dictionary<Type, TypeDeserializerCache> byType =
                new Dictionary<Type, TypeDeserializerCache>();

            public static Func<XmlGridRowReader, object> GetDeserializer(
                Type type, XmlGridRowReader reader)
            {
                if (!byType.TryGetValue(type, out var deserializers))
                {
                    lock (byType)
                    {
                        if (!byType.TryGetValue(type, out deserializers))
                        {
                            deserializers = new TypeDeserializerCache(type);
                        }

                        byType.Add(type, deserializers);
                    }
                }

                return deserializers.GetDeserializer(reader);
            }

            private Type type;
            private readonly Dictionary<DeserializerKey, Func<XmlGridRowReader, object>> deserializers =
                new Dictionary<DeserializerKey, Func<XmlGridRowReader, object>>();

            public TypeDeserializerCache(Type type)
            {
                this.type = type;
            }

            private Func<XmlGridRowReader, object> GetDeserializer(
                XmlGridRowReader reader)
            {
                reader.ReadRow();

                if (reader.Columns.Count == 0)
                {
                    throw new InvalidOperationException("No columns");
                }

                var key = new DeserializerKey(reader.Columns);

                if (!deserializers.TryGetValue(key, out var deserializer))
                {
                    lock (deserializers)
                    {
                        if (!deserializers.TryGetValue(key, out deserializer))
                        {
                            deserializer = CreateDeserializer(type, reader.Columns);
                            deserializers.Add(key, deserializer);
                        }
                    }
                }

                return deserializer;
            }
        }

        private class DeserializerKey : IEquatable<DeserializerKey>
        {
            private readonly int hashCode;
            private readonly List<string> fields;

            public DeserializerKey(List<string> fields)
            {
                this.fields = new List<string>(fields);

                hashCode = ComputeHashCode(fields);
            }

            private int ComputeHashCode(List<string> fields)
            {
                unchecked
                {
                    int hash = 19;

                    foreach (var field in fields)
                    {
                        hash = hash * 31 + field.GetHashCode();
                    }
                    return hash;
                }
            }

            public override int GetHashCode() => hashCode;

            public bool Equals(DeserializerKey other)
            {
                if (hashCode != other.hashCode || fields.Count != other.fields.Count)
                {
                    return false;
                }

                for (int i = 0; i < fields.Count; i++)
                {
                    if (fields[i] != other.fields[i])
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
