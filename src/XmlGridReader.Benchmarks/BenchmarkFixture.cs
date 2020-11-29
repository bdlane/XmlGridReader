using System.IO;
using System.Linq;
using System.Xml.Serialization;
using AutoFixture;

namespace XmlGridReader.Benchmarks
{
    internal class BenchmarkFixture
    {
        internal string GenerateXml(int number)
        {
            var fixture = new Fixture();

            var books = fixture.CreateMany<Book>(number)
                .Select(b => new SerializableBook
                {
                    Title = b.Title,
                    NumberOfPages = b.NumberOfPages.ToString(),
                    DatePublished = b.DatePublished.ToString("o")
                });

            var data = new Data
            {
                Books = books.ToList()
            };

            var ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            var serializer = new XmlSerializer(typeof(Data));

            using var textWriter = new StringWriter();

            serializer.Serialize(textWriter, data, ns);

            return textWriter.ToString();
        }
    }
}
