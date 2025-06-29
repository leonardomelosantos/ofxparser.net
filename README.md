# OFXParser.NET

OFXParser.NET é uma biblioteca C# para leitura e tradução de arquivos OFX (Open Financial Exchange), facilitando a integração de dados financeiros em aplicações .NET.

## Recursos

- Leitura e parsing de arquivos OFX.
- Extração de transações bancárias, saldos e informações de conta.
- Fácil integração em projetos C#.
- Disponível via NuGet.

## Exemplo de Uso

```csharp
string pathOFX = "D:\\data\\MyFile.ofx";
Extract ofxParsed = OFXParser.Parser.GenerateExtract(pathOFX);
if (ofxParsed != null)
{
    foreach (var transaction in ofxParsed.Transactions)
    {
        // Faça algo com a transação
    }
}
```

## Instalação

Você pode instalar a biblioteca via NuGet:

```shell
dotnet add package OFXParser
```

Ou via Package Manager:

```shell
Install-Package OFXParser
```

[Veja no NuGet.org](https://www.nuget.org/packages/OFXParser/)

## Documentação

- **Parser.GenerateExtract(path)**: Lê o arquivo OFX e retorna um objeto `Extract` com as transações.
- **Extract.Transactions**: Lista de transações extraídas do arquivo.
- **Extract.Account**: Informações da conta bancária.

## Contribuição

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues ou pull requests.

## Licença

Este projeto está licenciado sob a licença MIT.

# How to download

It's possible download this lib by using Nuget.org. https://www.nuget.org/packages/OFXParser/
