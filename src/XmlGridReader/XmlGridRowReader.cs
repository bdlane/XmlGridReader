using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace XmlGridReader
{
    public class XmlGridRowReader : IDisposable
    {
        private static readonly XmlReaderSettings settings =
            new XmlReaderSettings { IgnoreWhitespace = true };

        private readonly XmlReader reader;
        private int currentColumnIndex = 0;

        public XmlGridRowReader(string xml)
        {
            reader = XmlReader.Create(new StringReader(xml), settings);
        }

        public List<string> Columns { get; private set; } = new List<string>();

        public List<string> Values { get; private set; } = new List<string>();

        /// <summary>
        /// Moves the reader to the grid, positioned at the first row
        /// </summary>
        public bool MoveToGrid()
        {
            // TODO: Needs to be more robust - consider if called
            // multiple times
            reader.MoveToContent(); // <Root>
            reader.Read();

            // If there are no rows, the reader will now either be
            // NodeType.None in the case of a self-closing tag,
            // or </Root> otherwise.

            return reader.NodeType == XmlNodeType.Element;
        }

        /// <summary>
        /// Reads the current row and advances the reader to the start of the
        /// next row, or to the end of the root element.
        /// </summary>
        /// <returns><see cref="true"/> if the row was read, <see cref="false"/> otherwise.</returns>
        public bool ReadRow()
        {
            // We should either be on a <Row>, </Root> or NodeType.None

            // Reset the row
            // Is this performant?
            // We currently only need to get the column names on the first row
            currentColumnIndex = 0;
            Columns.Clear();
            Values.Clear();

            if (reader.NodeType != XmlNodeType.Element)
            {
                return false;
            }

            // Advances from <Row> to <Col>
            // TODO: check this is <Col>, and not an empty row -> exception
            reader.Read();

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                Columns.Add(reader.Name);
                Values.Add(reader.ReadElementContentAsString()); // Advances to next <Col> OR </Row>
            }

            // Advances from </Row> to
            //  <Row> OR </Data>
            reader.Read();

            return true;
        }

        /// <summary>
        /// Reads the current column value as a string, and advances the
        /// column index. Call this method after calling <see cref="ReadRow"/>.
        /// </summary>
        public string ReadColumnValue() => Values[currentColumnIndex++];

        public void Dispose() => reader.Dispose();
    }
}
