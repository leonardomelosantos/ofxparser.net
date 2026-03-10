using OFXParser.Core;
using OFXParser.Entities;

namespace OFXParser.Test;

[TestClass]
public class ParserTests
{
    // Helper to get the path of a test data file relative to the output directory
    private static string TestDataPath(string fileName)
        => Path.Combine(AppContext.BaseDirectory, "TestData", fileName);

    // ─── Complete Extract Tests ─────────────────────────────────────────────

    [TestMethod]
    public void GenerateExtract_CompleteFile_ReturnsExtractWithCorrectTransactionCount()
    {
        var extract = Parser.GenerateExtract(TestDataPath("complete_extract.ofx"));

        Assert.AreEqual(3, extract.Transactions.Count,
            "Complete OFX file should yield 3 transactions.");
    }

    [TestMethod]
    public void GenerateExtract_CompleteFile_ParsesHeaderCorrectly()
    {
        var extract = Parser.GenerateExtract(TestDataPath("complete_extract.ofx"));

        Assert.AreEqual("POR", extract.Header.Language);
        Assert.AreEqual("Banco do Brasil", extract.Header.BankName);
        Assert.AreEqual(new DateTime(2025, 1, 1, 12, 0, 0), extract.Header.ServerDate);
    }

    [TestMethod]
    public void GenerateExtract_CompleteFile_ParsesBankAccountCorrectly()
    {
        var extract = Parser.GenerateExtract(TestDataPath("complete_extract.ofx"));

        Assert.AreEqual(1, extract.BankAccount.Bank?.Code);
        Assert.AreEqual("1234-5", extract.BankAccount.AgencyCode);
        Assert.AreEqual("12345-6", extract.BankAccount.AccountCode);
        Assert.AreEqual("CHECKING", extract.BankAccount.Type);
    }

    [TestMethod]
    public void GenerateExtract_CompleteFile_ParsesExtractDatesCorrectly()
    {
        var extract = Parser.GenerateExtract(TestDataPath("complete_extract.ofx"));

        Assert.AreEqual(new DateTime(2025, 1, 1, 0, 0, 0), extract.InitialDate);
        Assert.AreEqual(new DateTime(2025, 1, 31, 23, 59, 59), extract.FinalDate);
    }

    [TestMethod]
    public void GenerateExtract_CompleteFile_ParsesFinalBalanceCorrectly()
    {
        var extract = Parser.GenerateExtract(TestDataPath("complete_extract.ofx"));

        Assert.AreEqual(2264.50m, extract.FinalBalance,
            "Final balance should match BALAMT value.");
    }

    [TestMethod]
    public void GenerateExtract_CompleteFile_ParsesDebitTransactionCorrectly()
    {
        var extract = Parser.GenerateExtract(TestDataPath("complete_extract.ofx"));

        var debit = extract.Transactions[0];
        Assert.AreEqual("DEBIT", debit.Type);
        Assert.AreEqual(new DateTime(2025, 1, 5, 12, 0, 0), debit.Date);
        Assert.AreEqual(-150.50m, debit.TransactionValue);
        Assert.AreEqual("20250105001", debit.Id);
        Assert.AreEqual("Compra Supermercado", debit.Description);
    }

    [TestMethod]
    public void GenerateExtract_CompleteFile_ParsesCreditTransactionCorrectly()
    {
        var extract = Parser.GenerateExtract(TestDataPath("complete_extract.ofx"));

        var credit = extract.Transactions[1];
        Assert.AreEqual("CREDIT", credit.Type);
        Assert.AreEqual(2500.00m, credit.TransactionValue);
        Assert.AreEqual("Salario Janeiro", credit.Description);
    }

    [TestMethod]
    public void GenerateExtract_CompleteFile_ParsesChecksumCorrectly()
    {
        var extract = Parser.GenerateExtract(TestDataPath("complete_extract.ofx"));

        var transactionWithCheck = extract.Transactions[2];
        Assert.AreEqual(1234L, transactionWithCheck.Checksum);
    }

    [TestMethod]
    public void GenerateExtract_CompleteFile_HasNoImportingErrors()
    {
        var extract = Parser.GenerateExtract(TestDataPath("complete_extract.ofx"));

        Assert.AreEqual(0, extract.ImportingErrors.Count,
            "A well-formed OFX file should produce no importing errors.");
    }

    // ─── Minimal Extract Tests ──────────────────────────────────────────────

    [TestMethod]
    public void GenerateExtract_MinimalFile_ReturnsExtractWithOneTransaction()
    {
        var extract = Parser.GenerateExtract(TestDataPath("minimal_extract.ofx"));

        Assert.AreEqual(1, extract.Transactions.Count);
        Assert.AreEqual(500.00m, extract.Transactions[0].TransactionValue);
    }

    [TestMethod]
    public void GenerateExtract_MinimalFile_ParsesBankIdCorrectly()
    {
        var extract = Parser.GenerateExtract(TestDataPath("minimal_extract.ofx"));

        Assert.AreEqual(341, extract.BankAccount.Bank?.Code);
    }

    [TestMethod]
    public void GenerateExtract_MinimalFile_HasNoImportingErrors()
    {
        var extract = Parser.GenerateExtract(TestDataPath("minimal_extract.ofx"));

        Assert.AreEqual(0, extract.ImportingErrors.Count);
    }

    // ─── Decimal Parsing Tests ──────────────────────────────────────────────

    [TestMethod]
    public void GenerateExtract_NegativeDecimalValues_ParsedCorrectly()
    {
        var extract = Parser.GenerateExtract(TestDataPath("comma_decimal_extract.ofx"));

        Assert.AreEqual(1, extract.Transactions.Count);
        Assert.AreEqual(-1234.56m, extract.Transactions[0].TransactionValue);
        Assert.AreEqual(-1234.56m, extract.FinalBalance);
    }

    // ─── Custom Currency Converter Tests ────────────────────────────────────

    [TestMethod]
    public void GenerateExtract_WithCustomCurrencyConverter_UsesConverter()
    {
        var settings = new ParserSettings
        {
            CustomConverterCurrency = value => decimal.Parse(
                value.Replace(',', '.'),
                System.Globalization.CultureInfo.InvariantCulture)
        };

        var extract = Parser.GenerateExtract(TestDataPath("minimal_extract.ofx"), settings);

        Assert.AreEqual(500.00m, extract.Transactions[0].TransactionValue);
    }

    // ─── Validation Settings Tests ──────────────────────────────────────────

    [TestMethod]
    public void GenerateExtract_WithHeaderValidation_ThrowsWhenHeaderMissing()
    {
        var settings = new ParserSettings { IsValidateHeader = true };

        Assert.ThrowsException<OFXParserException>(() =>
            Parser.GenerateExtract(TestDataPath("minimal_extract.ofx"), settings),
            "Parser should throw when header is required but absent.");
    }

    [TestMethod]
    public void GenerateExtract_WithHeaderValidation_SucceedsWhenHeaderPresent()
    {
        var settings = new ParserSettings { IsValidateHeader = true };

        var extract = Parser.GenerateExtract(TestDataPath("complete_extract.ofx"), settings);

        Assert.IsNotNull(extract);
        Assert.AreEqual("POR", extract.Header.Language);
    }

    [TestMethod]
    public void GenerateExtract_WithAccountValidation_ThrowsWhenAccountDataMissing()
    {
        var settings = new ParserSettings { IsValidateAccountData = true };

        Assert.ThrowsException<OFXParserException>(() =>
            Parser.GenerateExtract(TestDataPath("empty_extract.ofx"), settings),
            "Parser should throw when account data is required but absent.");
    }

    [TestMethod]
    public void GenerateExtract_WithAccountValidation_SucceedsWhenAccountPresent()
    {
        var settings = new ParserSettings { IsValidateAccountData = true };

        var extract = Parser.GenerateExtract(TestDataPath("minimal_extract.ofx"), settings);

        Assert.IsNotNull(extract.BankAccount);
        Assert.IsNotNull(extract.BankAccount.AccountCode);
    }

    // ─── File Not Found Tests ───────────────────────────────────────────────

    [TestMethod]
    public void GenerateExtract_NonExistentFile_ThrowsFileNotFoundException()
    {
        Assert.ThrowsException<FileNotFoundException>(() =>
            Parser.GenerateExtract("/non/existent/path/file.ofx"));
    }

    // ─── Async Tests ─────────────────────────────────────────────────────────

    [TestMethod]
    public async Task GenerateExtractAsync_CompleteFile_ReturnsCorrectTransactionCount()
    {
        var extract = await Parser.GenerateExtractAsync(TestDataPath("complete_extract.ofx"));

        Assert.AreEqual(3, extract.Transactions.Count);
    }

    [TestMethod]
    public async Task GenerateExtractAsync_CompleteFile_ParsesBalanceCorrectly()
    {
        var extract = await Parser.GenerateExtractAsync(TestDataPath("complete_extract.ofx"));

        Assert.AreEqual(2264.50m, extract.FinalBalance);
    }

    [TestMethod]
    public async Task GenerateExtractAsync_NonExistentFile_ThrowsFileNotFoundException()
    {
        await Assert.ThrowsExceptionAsync<FileNotFoundException>(async () =>
            await Parser.GenerateExtractAsync("/non/existent/file.ofx"));
    }

    // ─── Empty Extract Tests ────────────────────────────────────────────────

    [TestMethod]
    public void GenerateExtract_EmptyExtract_ReturnsNoTransactions()
    {
        var extract = Parser.GenerateExtract(TestDataPath("empty_extract.ofx"));

        Assert.AreEqual(0, extract.Transactions.Count,
            "Empty extract file should have zero transactions.");
    }

    // ─── Entity Tests ───────────────────────────────────────────────────────

    [TestMethod]
    public void Extract_AddTransaction_IncreasesTransactionCount()
    {
        var extract = new Extract(new HeaderExtract(), new BankAccount(), "OK");
        var transaction = new Transaction { Type = "CREDIT", TransactionValue = 100m };

        extract.AddTransaction(transaction);

        Assert.AreEqual(1, extract.Transactions.Count);
        Assert.AreEqual(100m, extract.Transactions[0].TransactionValue);
    }

    [TestMethod]
    public void Extract_ImportingErrors_StartsEmpty()
    {
        var extract = new Extract(new HeaderExtract(), new BankAccount(), "OK");

        Assert.AreEqual(0, extract.ImportingErrors.Count);
    }

    [TestMethod]
    public void Bank_Constructor_SetsCodeAndName()
    {
        var bank = new Bank(341, "Itaú Unibanco");

        Assert.AreEqual(341, bank.Code);
        Assert.AreEqual("Itaú Unibanco", bank.Name);
    }
}
