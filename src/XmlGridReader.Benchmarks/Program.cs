using BenchmarkDotNet.Running;

namespace XmlGridReader.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            //var data = new Benchmarks
            //{
            //    NumberOfRecords = 2
            //}.XmlReaderBenchmark();

            var summary = BenchmarkRunner.Run<Benchmarks>();
        }
    }
}
