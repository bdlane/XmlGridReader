using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace XmlGridReader
{
    public static partial class Reader
    {
        private class TypeDeserializerCache
        {
            private static readonly Dictionary<Type, TypeDeserializerCache> byType =
                new Dictionary<Type, TypeDeserializerCache>();

            public static Func<XmlReader, object> GetDeserializer(
                Type type, string xml)
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

                return deserializers.GetDeserializer(xml);
            }

            private Type type;
            private readonly Dictionary<DeserializerKey, Func<XmlReader, object>> deserializers =
                new Dictionary<DeserializerKey, Func<XmlReader, object>>();

            public TypeDeserializerCache(Type type)
            {
                this.type = type;
            }

            private static Func<XmlReader, object> d;

            private Func<XmlReader, object> GetDeserializer(string xml)
            {
                // Needs locks

                //List<string> fields = null;// GetFields(xml);
                List<string> fields = GetFields(xml);
                //if (d is null)
                //{
                //    d = CreateDeserializer(type, fields);
                //}

                //return d;

                var key = new DeserializerKey(fields);

                if (!deserializers.TryGetValue(key, out var deserializer))
                {
                    deserializer = CreateDeserializer(type, fields);
                    deserializers.Add(key, deserializer);
                }

                return deserializer;
            }

            private static XmlReaderSettings settings = new XmlReaderSettings { IgnoreWhitespace = true };

            private static List<string> GetFields(string xml)
            {
                var result = new List<string>();

                // Assumes XML is well formed
                using (var reader = XmlReader.Create(new StringReader(xml), settings))
                {
                    // TODO: add test for XML declaration
                    reader.MoveToContent(); // <Data>
                    //return result;
                    reader.Read(); // Row

                    while (reader.Read() && reader.NodeType != XmlNodeType.EndElement) // Column
                    {
                        result.Add(reader.LocalName);
                        reader.Read(); // Text content
                        reader.Read(); // End element
                    }
                }

                return result;
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
                if (hashCode != other.hashCode || fields.Count() != other.fields.Count())
                {
                    return false;
                }

                for (int i = 0; i < fields.Count(); i++)
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
