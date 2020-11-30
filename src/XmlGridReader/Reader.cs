using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

            // Might need to pass XML/fields to allow varying order
            var deserializer = GetDeserializer(typeof(T));

            var result = new List<T>();

            var settings = new XmlReaderSettings { IgnoreWhitespace = true };

            // Assumes XML is well formed
            using (var reader = XmlReader.Create(new StringReader(xml), settings))
            {
                // TODO: add test for XML declaration
                reader.MoveToContent(); // <Data>

                while (reader.Read() && reader.NodeType != XmlNodeType.EndElement) // Row
                {
                    reader.Read();
                    result.Add((T)deserializer(reader));
                }
            }

            return result;
        }

        private static Dictionary<Type, Func<XmlReader, object>> deserializers =
            new Dictionary<Type, Func<XmlReader, object>>();

        private static Func<XmlReader, object> GetDeserializer(Type type)
        {
            // TODO: make this thread-safe
            if (deserializers.TryGetValue(type, out var deserializer))
            {
                return deserializer;
            }

            deserializer = CreateDeserializer(type);
            deserializers.Add(type, deserializer);

            return deserializer;
        }

        private static Func<XmlReader, object> CreateDeserializer(Type type)
        {
            if (type == typeof(string) || type.IsValueType)
            {
                return CreateValueTypeDeserializer(type);
            }

            var ctors = type.GetConstructors();

            // TODO: deal with more then ctor?
            if (ctors.Any(c => c.GetParameters().Any()))
            {
                return CreateComplexTypeCtorDeserializer(type);
            }

            return CreateComplexTypePropDerializer(type);
        }

        private static Func<XmlReader, object> CreateComplexTypePropDerializer(Type type)
        {
            // Assumes
            //  - nodes and props are in the same order
           //   - have same casing
            //  - has correct number of nodes            
            var paramReaderExp = Expression.Parameter(typeof(XmlReader), "reader");

            var readElementContentAsStringMethodInfo =
                typeof(XmlReader).GetMethod(
                    nameof(XmlReader.ReadElementContentAsString),
                    new Type[] { });

            var initializerExps = type.GetProperties().Select(p =>
            {
                var readContentExp = Expression.Call(
                    paramReaderExp,
                    readElementContentAsStringMethodInfo);

                var castExp = GetTypeConverterExpression(p.PropertyType, readContentExp);

                return Expression.Bind(p, castExp);
            });

            var initExpression = Expression.MemberInit(
                Expression.New(type),
                initializerExps);

            return Expression.Lambda<Func<XmlReader, object>>(initExpression, paramReaderExp).Compile();
        }

        private static Func<XmlReader, object> CreateComplexTypeCtorDeserializer(Type type)
        {
            // TODO: cache XmlReader types?
            var ctor = type.GetConstructors().Single();

            var paramReaderExp = Expression.Parameter(typeof(XmlReader), "reader");

            var readElementContentAsStringMethodInfo =
                typeof(XmlReader).GetMethod(
                    nameof(XmlReader.ReadElementContentAsString),
                    new Type[] { });

            // Assumes
            //  - nodes and ctor params are in the same order
            //  - has correct number of nodes
            var argExps = ctor.GetParameters().Select(p =>
            {
                var readContentExp = Expression.Call(
                    paramReaderExp,
                    readElementContentAsStringMethodInfo);

                return GetTypeConverterExpression(p.ParameterType, readContentExp);
            });

            var newExp = Expression.New(ctor, argExps);

            return Expression.Lambda<Func<XmlReader, object>>(newExp, paramReaderExp).Compile();
        }


        private static Expression GetTypeConverterExpression(
            Type type, Expression rawValueAccessorExp)
        {
            var getConverterExp = Expression.Call(
                    getTypeConverterMethod,
                    Expression.Constant(type));

            var convertFromStringExp = Expression.Call(
                getConverterExp,
                ConvertFromInvariantStringMethod,
                new[] { rawValueAccessorExp });

            return Expression.Convert(
                convertFromStringExp, type);
        }

        private static MethodInfo getTypeConverterMethod =
                typeof(Reader).GetMethod(
                    nameof(Reader.GetTypeConverter),
                    BindingFlags.NonPublic | BindingFlags.Static);

        private static MethodInfo ConvertFromInvariantStringMethod =
                typeof(TypeConverter).GetMethod(
                    nameof(TypeConverter.ConvertFromInvariantString),
                    new[] { typeof(string) });

        private static Func<XmlReader, object> CreateValueTypeDeserializer(Type type)
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
