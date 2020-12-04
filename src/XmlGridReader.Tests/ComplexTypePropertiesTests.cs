using AutoFixture.Xunit2;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace XmlGridReader.Tests
{
    public class ComplexTypePropertiesTests
    {
        [Theory, AutoData]
        public void Given_MultipleRows_When_Read_Returns_CollectionOfBooks(IEnumerable<Book> expected)
        {
            // Arrange
            var rowTemplate =
                "  <Row>\r\n" +
                "      <Title>{0}</Title>\r\n" +
                "      <NumberOfPages>{1}</NumberOfPages>\r\n" +
                "      <DatePublished>{2}</DatePublished>\r\n" +
                "  </Row>\r\n";

            var rows = expected.Select(
                b => string.Format(
                    rowTemplate,
                    b.Title,
                    b.NumberOfPages,
                    b.DatePublished.ToString("o")))
                .Aggregate((c, n) => c + n);

            var xml = $"<Data>\r\n{rows}</Data>";

            // Act
            var actual = Reader.Read<Book>(xml);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory, AutoData]
        public void Given_ColumnsOutOfOrder_When_Read_Returns_CollectionOfBooks(IEnumerable<Book> expected)
        {
            // Arrange
            var rowTemplate =
                "  <Row>\r\n" +
                "      <Title>{0}</Title>\r\n" +
                "      <DatePublished>{2}</DatePublished>\r\n" +
                "      <NumberOfPages>{1}</NumberOfPages>\r\n" +
                "  </Row>\r\n";

            var rows = expected.Select(
                b => string.Format(
                    rowTemplate,
                    b.Title,
                    b.NumberOfPages,
                    b.DatePublished.ToString("o")))
                .Aggregate((c, n) => c + n);

            var xml = $"<Data>\r\n{rows}</Data>";

            // Act
            var actual = Reader.Read<Book>(xml);

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
