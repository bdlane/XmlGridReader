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

    public int NumberOfPages { get; set; }

    public DateTime DatePublished { get; set; }
}
```
Simply deserialize the XML into a collection of DTOs:
```csharp
IEnumerable<Book> books = Reader.Read<Book>(xml);
```

## Why did I create XmlGridReader?

At work, I needed to migrate some data access code from using SQL to using a web API that returned table-like data in XML. The original SQL code used [Dapper](https://github.com/StackExchange/Dapper), and I wanted to keep the benefits of a simple convention-based deserializer.

The web API and the XML it returns has some idiosyncrasies, which have influenced the design of this library.

## Limitations

If the type argument of `Read<T>(string)` is a class (excluding `string`), the data will either be mapped to the class' public settable properties, or to the class' single non-parameterless constructor. The 'columns' in the data must match the order of parameters or properties in the target.

Empty/null cells must be represented as empty elements i.e. `<element></element>`, not omitted or with the `xsi:nil` attribute.
