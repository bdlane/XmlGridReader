using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Xunit;

namespace XmlGridReader.Tests
{
    public class DynamicTypeTests
    {
        [Theory, AutoData]
        public void When_ReadNonGeneric_Returns_CollectionOfDynamicBooks(IEnumerable<Book> books)
        {
            // Arrange
            var expected = books.Select(b =>
            {
                var item = new ExpandoObject();

                item.TryAdd(nameof(Book.Title), b.Title);
                item.TryAdd(nameof(Book.NumberOfPages), b.NumberOfPages.ToString());
                item.TryAdd(nameof(Book.DatePublished), b.DatePublished.ToString("o"));

                return item;
            });

            var rowTemplate =
                "  <Row>\r\n" +
                "      <Title>{0}</Title>\r\n" +
                "      <NumberOfPages>{1}</NumberOfPages>\r\n" +
                "      <DatePublished>{2}</DatePublished>\r\n" +
                "  </Row>\r\n";

            var rows = books.Select(
                b => string.Format(
                    rowTemplate,
                    b.Title,
                    b.NumberOfPages,
                    b.DatePublished.ToString("o")))
                .Aggregate((c, n) => c + n);

            var xml = $"<Data>\r\n{rows}</Data>";

            // Act
            var actual = Reader.Read(xml);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        public class Book
        {
            public string Title { get; set; }

            public int NumberOfPages { get; set; }

            public DateTime DatePublished { get; set; }
        }
    }
}
