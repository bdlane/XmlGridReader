using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Mathematics;

namespace XmlGridReader.Benchmarks
{
    [RankColumn(NumeralSystem.Arabic)]
    [MemoryDiagnoser]
    public class Benchmarks
    {
        private string xml;
        private XmlSerializer serializer;

        public Benchmarks()
        {
            serializer = new XmlSerializer(
                typeof(List<Book>),
                new XmlRootAttribute("Data"));
        }

        [Params(1000)]
        public int NumberOfRecords
        {
            set
            {
                xml = new BenchmarkFixture().GenerateXml(value);
            }
        }

        [Benchmark(Baseline = true, Description = "XmlSerializer")]
        public List<Book> XmlSerializerBenchmark()
        {
            using var reader = XmlReader.Create(new StringReader(xml));
            return (List<Book>)serializer.Deserialize(reader);
        }

        [Benchmark]
        public List<Book> LinqToXml()
        {
            var doc = XDocument.Parse(xml);
            var rows = doc.Root.Elements();

            var books = rows.Select(r => new Book
            {
                Title = r.Element("Title").Value,
                NumberOfPages = int.Parse(r.Element("NumberOfPages").Value),
                DatePublished = DateTime.Parse(r.Element("DatePublished").Value)
            });

            return books.ToList();
        }

        [Benchmark(Description = "XmlReader")]
        public List<Book> XmlReaderBenchmark()
        {
            var results = new List<Book>();

            using var reader = XmlReader.Create(
                new StringReader(xml),
                new XmlReaderSettings { IgnoreWhitespace = true });

            reader.MoveToContent(); // <Data>

            while (reader.Read() && reader.NodeType != XmlNodeType.EndElement)
            {
                reader.Read(); // <Title>

                var book = new Book
                {
                    Title = reader.ReadElementContentAsString(),
                    NumberOfPages = reader.ReadElementContentAsInt(),
                    DatePublished = reader.ReadElementContentAsDateTime()
                };

                results.Add(book);
            }

            return results;
        }

        [Benchmark]
        public List<Book> XmlGridReader_Properties()
        {
            return Reader.Read<Book>(xml).ToList();
        }

        [Benchmark]
        public List<Magazine> XmlGridReader_Constructor()
        {
            return Reader.Read<Magazine>(xml).ToList();
        }
    }
}
