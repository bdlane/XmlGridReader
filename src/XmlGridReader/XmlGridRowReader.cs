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

        public XmlGridRowReader(string xml)
        {
            reader = XmlReader.Create(new StringReader(xml), settings);
        }

        // Consider return type...
        public List<string> Columns { get; private set; } =
            new List<string>();

        public List<string> Values { get; private set; } =
            new List<string>();

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

        public bool ReadRow()
        {
            // Assume we either start on
            //  <Row>, OR </Data>

            // Is this performant?
            current = 0;
            Columns.Clear();
            Values.Clear();

            if (reader.NodeType == XmlNodeType.EndElement
                || reader.NodeType == XmlNodeType.None)
            {
                return false;
            }

            // Really, we only need the column names
            // on the first row... for now?

            // Advances from <Row> to <Col>, OR
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

        int current = 0;
        // Just for the moment...
        public string ReadColumnValue()
        {
            return Values[current++];
        }

        public void Dispose() => reader.Dispose();
    }
}
