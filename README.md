# Overview
Library developed with C# that reads and translate OFX files (financial files), like a parser to help your own C# application. It's possible download this lib by using Nuget.org.

Example:

```
string pathOFX = "D:\\data\\MyFile.ofx";
Extract ofxParsed = OFXParser.Parser.GenerateExtract(pathOFX);
if (ofxParsed != null)
{
    foreach (var transaction in ofxParsed.Transactions)
    {
       // Do something with the transaction    
    }
}
```
