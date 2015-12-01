# ofxparser.net
Biblioteca desenvolvida em C# que traduz arquivos OFX e gera a instância de uma classe que representa o arquivo.

Exemplo de código:

Extract extratoBancario = OFXParser.Parser.GenerateExtract(caminhoDoArquivoOFX);

if (extratoBancario != null)
{
	foreach (var transacao in extratoBancario.Transactions)
	{

	}
}
