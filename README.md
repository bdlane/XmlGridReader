# XmlGridReader

A library to simplify deserializing data tables from XML. 

No need to manually traverse the DOM and project results using LINQ to XML, or be limited by the constraints of `XmlSerializer` (classes must be public, and have parameterless constructor).

## How to use it
Given the following XML:

```xml
<Data>
  <Book>
    <Title>Anna Karenina</Title>
    <PublicationDate>1878-03-27</PublicationDate>
    <NumberOfPages>864</NumberOfPages>
  </Book>
  <Book>
    <Title>To Kill a Mockingbird</Title>
    <PublicationDate>1960-07-11</PublicationDate>
    <NumberOfPages>281</NumberOfPages>
  </Book>
</Data>
```
And a DTO:
```csharp
public class Book
{
    public string Title { get; set; }

    public DateTime PublicationDate { get; set; }

    public int NumberOfPages { get; set; }
}
```
Simply deserialize the XML into a collection of DTOs:
```csharp
IEnumerable<Book> books = Reader.Read<Book>(xml);
```

## Why did I create XmlGridReader?

I needed to migrate some data access code from using SQL to using a web API that returned table-like data in XML. The original SQL code used [Dapper](https://github.com/StackExchange/Dapper), and I wanted to keep the benefits of a simple convention-based deserializer.

The web API and the XML it returns has some idiosyncrasies, which have influenced the design of this library.

## Limitations

If the type argument of `Read<T>(string)` is a class (excluding `string`), the data will either be mapped to the class' public settable properties, or to the class' single non-parameterless constructor. The 'columns' in the data must match the order of parameters or properties in the target.

Empty/null cells must be represented as empty elements i.e. `<element></element>`, not omitted or with the `xsi:nil` attribute.

## Benchmarks

``` ini

BenchmarkDotNet=v0.12.1, OS=macOS Catalina 10.15.7 (19H2) [Darwin 19.6.0]
Intel Core i5-1038NG7 CPU 2.00GHz, 1 CPU, 8 logical and 4 physical cores
.NET Core SDK=5.0.100
  [Host]     : .NET Core 3.1.10 (CoreCLR 4.700.20.51601, CoreFX 4.700.20.51901), X64 RyuJIT
  DefaultJob : .NET Core 3.1.10 (CoreCLR 4.700.20.51601, CoreFX 4.700.20.51901), X64 RyuJIT


```
|                    Method | NumberOfRecords |     Mean |     Error |    StdDev |   Median | Ratio | RatioSD | Rank |
|-------------------------- |---------------- |---------:|----------:|----------:|---------:|------:|--------:|-----:|
|             XmlSerializer |            1000 | 1.784 ms | 0.0351 ms | 0.0651 ms | 1.762 ms |  1.00 |    0.00 |    2 |
|                 LinqToXml |            1000 | 2.010 ms | 0.0390 ms | 0.0478 ms | 2.005 ms |  1.11 |    0.05 |    3 |
|                 XmlReader |            1000 | 1.132 ms | 0.0270 ms | 0.0789 ms | 1.135 ms |  0.65 |    0.05 |    1 |
|  XmlGridReader_Properties |            1000 | 2.718 ms | 0.0760 ms | 0.2204 ms | 2.743 ms |  1.54 |    0.12 |    4 |
| XmlGridReader_Constructor |            1000 | 2.630 ms | 0.0666 ms | 0.1964 ms | 2.577 ms |  1.56 |    0.10 |    4 |
