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

            var xml = $"<Data><Col1><{s}/Col1></Data>";

            // Act
            var actual = Reader.Read<string>(xml);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }

        [Theory, AutoData]
        public void Given_MultipleRowStringColumn_When_Read_Returns_CollectionOfStrings(IEnumerable<string> expected)
        {
            // Arrange
            var rows = expected.Select(s => $"<Col1>{s}</Col1>").Aggregate((c, n) => c + n);
            var xml = $"<Data>{rows}</Data>";

            // Act
            var actual = Reader.Read<string>(xml);

            // Assert
            actual.Should().BeEquivalentTo(expected);
        }
    }
}
