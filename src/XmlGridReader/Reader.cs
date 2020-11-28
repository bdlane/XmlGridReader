using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;

namespace XmlGridReader
{
    public class Reader
    {
        public static IEnumerable<T> Read<T>(string xml)
        {
            if (xml is null)
            {
                throw new ArgumentNullException(nameof(xml));
            }

            // Might need to move this to get first row columns
            var deserializer = GetDeserializer(typeof(T));

            var result = new List<T>();

            var settings = new XmlReaderSettings { IgnoreWhitespace = true };

            using (var reader = XmlReader.Create(new StringReader(xml), settings))
            {
                reader.Read(); // <Data>

                while (reader.Read() && reader.NodeType != XmlNodeType.EndElement) // Row
                {
                    reader.Read();
                    result.Add((T)deserializer(reader));
                }

            }

            return result;
        }

        private static Func<XmlReader, object> GetDeserializer(Type type)
        {
            var converter = GetTypeConverter(type);

            return new Func<XmlReader, object>(r =>
            {
                return converter.ConvertFromInvariantString(
                    r.ReadElementContentAsString());
            });
        }

        private static TypeConverter GetTypeConverter(Type type)
        {
            // TODO: implement caching
            //  will need to implement locking
            return TypeDescriptor.GetConverter(type);
        }
    }
}
