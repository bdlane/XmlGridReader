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
    public static partial class Reader
    {
        private static readonly XmlReaderSettings settings =
            new XmlReaderSettings { IgnoreWhitespace = true };

        public static IEnumerable<T> Read<T>(string xml)
        {
            if (xml is null)
            {
                throw new ArgumentNullException(nameof(xml));
            }

            // Might need to pass XML/fields to allow varying order
            var deserializer = GetDeserializer(typeof(T), xml);

            var result = new List<T>();


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
        private static Func<XmlReader, object> GetDeserializer(
            Type type, string xml)
        {
            return TypeDeserializerCache.GetDeserializer(type, xml);
        }

        private static Func<XmlReader, object> CreateDeserializer(
            Type type, List<string> fields)
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

            return CreateComplexTypePropDerializer(type, fields);
        }

        private static Func<XmlReader, object> CreateComplexTypePropDerializer(
            Type type, List<string> fields)
        {
            // Check for duplicates in fields?
            var props = type.GetProperties();

            // Assumes
            //  - nodes and props have same casing
            //  - has correct number of nodes
            var orderedProps = fields.Select(f => props.Single(p => p.Name == f));
            var paramReaderExp = Expression.Parameter(typeof(XmlReader), "reader");

            var readElementContentAsStringMethodInfo =
                typeof(XmlReader).GetMethod(
                    nameof(XmlReader.ReadElementContentAsString),
                    new Type[] { });

            var initializerExps = orderedProps.Select(p =>
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

            // Need to assign variables,
            // as the reader can only be called in order,
            // but the params may be in a different order
            // Then assign the variables to the ctor params
            // in the correct order.

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

        private static Dictionary<Type, TypeConverter> typeConverters =
            new Dictionary<Type, TypeConverter>();

        private static TypeConverter GetTypeConverter(Type type)
        {
            if (!typeConverters.TryGetValue(type, out var typeConverter))
            {
                lock (typeConverters)
                {
                    if (typeConverters.TryGetValue(type, out typeConverter))
                    {
                        return typeConverter;
                    }

                    typeConverter = TypeDescriptor.GetConverter(type);
                    typeConverters.Add(type, typeConverter);
                }
            }

            return typeConverter;
        }
    }
}
