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

## Pipelines de CI/CD (GitHub Actions)

O projeto utiliza dois arquivos de workflow independentes no GitHub Actions, cada um com uma responsabilidade distinta. Essa separação é intencional e reflete um princípio importante de design de pipelines: **cada workflow deve ter um único motivo para existir e um único gatilho**.

### `ci.yml` — Integração Contínua

**Gatilho:** abertura ou atualização de um Pull Request com destino a `master` ou `main`.

Esse workflow representa a **porta de entrada** para qualquer mudança no código. Seu objetivo é responder a uma pergunta simples: *"esse código está correto e não quebra nada?"*

Ele executa:
1. Build da solução em modo Release
2. Todos os testes unitários
3. Coleta de cobertura de código
4. Publicação dos resultados diretamente no Pull Request

O resultado aparece como um **check obrigatório no PR**. Enquanto ele não passar, o merge fica bloqueado. O workflow é **rápido, barato e descartável** — pode rodar dezenas de vezes ao dia sem nenhum efeito colateral no mundo externo.

### `publish.yml` — Entrega Contínua

**Gatilho:** criação de uma tag com formato `v<semver>` (ex: `v2.0.0`).

Esse workflow representa uma **decisão deliberada de lançamento**. Criar uma tag é um ato intencional que sinaliza: *"essa versão está pronta para ser consumida pelo mundo"*. Ele executa:

1. Build e testes novamente — para garantir que o código tagueado está íntegro
2. Pack do pacote NuGet estampando a versão extraída da tag
3. Push para o [NuGet.org](https://www.nuget.org/packages/OFXParser/) usando a secret `NUGET_API_KEY`

Ao contrário do CI, esse workflow tem **efeitos permanentes e irreversíveis**: uma vez publicada, uma versão no NuGet.org não pode ser sobrescrita ou removida.

### Por que dois arquivos e não um só?

| | `ci.yml` | `publish.yml` |
|---|---|---|
| **Gatilho** | Pull Request | Tag de versão |
| **Frequência** | Muitas vezes ao dia | Poucas vezes ao ano |
| **Efeito externo** | Nenhum | Publica no NuGet.org |
| **Objetivo** | Validar mudanças | Entregar valor |
| **Falha esperada?** | Sim, é parte do ciclo | Não — deve ser confiável |

Unir os dois em um único arquivo traria problemas: o job de publish rodaria em todo PR (desnecessário e perigoso), ou exigiria condicionais complexas que tornam o workflow difícil de ler e manter. Arquivos separados tornam a **intenção explícita** e o **escopo controlado**.

### Como publicar uma nova versão

```bash
# 1. Certifique-se de que o PR foi aprovado e o merge foi feito
# 2. Crie a tag localmente com o número da versão desejada
git tag v2.1.0

# 3. Envie a tag para o repositório — isso dispara o publish.yml
git push origin v2.1.0
```

> A secret `NUGET_API_KEY` deve estar configurada em **Settings → Secrets and variables → Actions** do repositório no GitHub.

## Contribuição

Contribuições são bem-vindas! Sinta-se à vontade para abrir issues ou pull requests.

## Licença

Este projeto está licenciado sob a licença MIT.

# How to download

It's possible download this lib by using Nuget.org. https://www.nuget.org/packages/OFXParser/
