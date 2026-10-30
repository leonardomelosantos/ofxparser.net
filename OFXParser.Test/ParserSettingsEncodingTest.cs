using OFXParser.Core;
using OFXParser.Entities;
using System.Text;

namespace OFXParser.Test
{
    [TestClass]
    public class ParserSettingsEncodingTest
    {
        private const string MEMO = "Pagamento ação – São João";

        private string ofxFile = string.Empty;

        [TestInitialize]
        public void Initialize()
        {
            ofxFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".ofx");
        }

        [TestCleanup]
        public void Cleanup()
        {
            File.Delete(ofxFile);
            File.Delete(ofxFile + ".xml");
        }

        [TestMethod]
        public void GenerateExtract_Windows1252File_WithEncoding_ReadsAccents()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            Encoding windows1252 = Encoding.GetEncoding(1252);
            WriteOfxFile(MEMO, windows1252);

            Extract extract = Parser.GenerateExtract(ofxFile, new ParserSettings { Encoding = windows1252 });

            Assert.AreEqual(MEMO, extract.Transactions[0].Description);
        }

        [TestMethod]
        public void GenerateExtract_Latin1File_WithEncoding_ReadsAccents()
        {
            WriteOfxFile("Pagamento ação", Encoding.Latin1);

            Extract extract = Parser.GenerateExtract(ofxFile, new ParserSettings { Encoding = Encoding.Latin1 });

            Assert.AreEqual("Pagamento ação", extract.Transactions[0].Description);
        }

        [TestMethod]
        public void GenerateExtract_Latin1File_WithoutEncoding_LosesAccents()
        {
            WriteOfxFile("Pagamento ação", Encoding.Latin1);

            Extract extract = Parser.GenerateExtract(ofxFile, new ParserSettings());

            Assert.AreNotEqual("Pagamento ação", extract.Transactions[0].Description);
        }

        [TestMethod]
        public void GenerateExtract_Utf8File_WithoutEncoding_ReadsAccents()
        {
            WriteOfxFile(MEMO, new UTF8Encoding(false));

            Extract extract = Parser.GenerateExtract(ofxFile, new ParserSettings());

            Assert.AreEqual(MEMO, extract.Transactions[0].Description);
        }

        [TestMethod]
        public void GenerateExtract_Utf8File_WithNullSettings_ReadsAccents()
        {
            WriteOfxFile(MEMO, new UTF8Encoding(false));

            Extract extract = Parser.GenerateExtract(ofxFile, null);

            Assert.AreEqual(MEMO, extract.Transactions[0].Description);
        }

        private void WriteOfxFile(string memo, Encoding encoding)
        {
            string content = string.Join(Environment.NewLine,
                "OFXHEADER:100",
                "<OFX>",
                "<STMTTRN>",
                "<TRNTYPE>DEBIT",
                "<MEMO>" + memo,
                "</STMTTRN>",
                "</OFX>");

            File.WriteAllText(ofxFile, content, encoding);
        }
    }
}
