using System.Collections.Generic;
using System.Linq;
using AutoFixture.Xunit2;
using FluentAssertions;
using Xunit;

namespace XmlGridReader.Tests
{
    public class SingleColumnTests
    {
        [Theory, AutoData]
        public void Given_SingleRowStringColumn_When_Read_Returns_CollectionOfStrings(string s)
        {
            // Arrange
            var expected = new List<string> { s };

            var xml =
                $"<Data>\r\n" +
                $"  <Row>\r\n" +
                $"      <Col1>{s}</Col1>\r\n" +
                $"  </Row>\r\n" +
                $"</Data>";

            // Act
            var actual = Reader.Read<string>(xml);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory, AutoData]
        public void Given_MultipleRowStringColumn_When_Read_Returns_CollectionOfStrings(IEnumerable<string> expected)
        {
            // Arrange
            var rowTemplate =
                "  <Row>\r\n" +
                "      <Col1>{0}</Col1>\r\n" +
                "  </Row>\r\n";

            var rows = expected.Select(s => string.Format(rowTemplate, s))
                .Aggregate((c, n) => c + n);

            var xml = $"<Data>\r\n{rows}</Data>";

            // Act
            var actual = Reader.Read<string>(xml);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
