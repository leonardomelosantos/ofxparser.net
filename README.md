# ofxparser.net
Biblioteca desenvolvida em C# que traduz arquivos OFX e gera a instância de uma classe que representa o arquivo.

Exemplo de código:

```
string caminhoDoArquivoOFX = "D:\\data\\meuArquivo.ofx";
Extract extratoBancario = OFXParser.Parser.GenerateExtract(caminhoDoArquivoOFX);
if (extratoBancario != null)
{
    foreach (var transacao in extratoBancario.Transactions)
    {
       // Fazer alguma coisa com a transação.    
    }
}
```
