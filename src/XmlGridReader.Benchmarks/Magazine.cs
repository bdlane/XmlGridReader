using System;

namespace XmlGridReader.Benchmarks
{
    public class Magazine
    {
        public Magazine(string title, int numberOfPages, DateTime datePublished)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            NumberOfPages = numberOfPages;
            DatePublished = datePublished;
        }

        public string Title { get; }

        public int NumberOfPages { get; }

        public DateTime DatePublished { get; }
    }
}
