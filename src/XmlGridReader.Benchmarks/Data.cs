using System.Collections.Generic;
using System.Xml.Serialization;

namespace XmlGridReader.Benchmarks
{
    public class Data
    {
        [XmlElement(ElementName = "Book")]
        public List<SerializableBook> Books { get; set; }
    }
}
