using OFXParser.Entities;

namespace OFXParser.Test
{
    [TestClass]
    public class ParserInvalidContentTest
    {
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
        [DataRow("<MEMO>Compra A & B", "Compra A & B")]
        [DataRow("<MEMO>Compra A&B", "Compra A&B")]
        [DataRow("<MEMO>Compra A & B</MEMO>", "Compra A & B")]
        [DataRow("<MEMO>AT&T &amp; A&B", "AT&T & A&B")]
        [DataRow("<MEMO>Compra A &amp; B", "Compra A & B")]
        [DataRow("<MEMO>Compra &lt;A&gt;", "Compra <A>")]
        [DataRow("<MEMO>Compra a&#231;&#xE3;o", "Compra ação")]
        public void GenerateExtract_MemoWithAmpersand_KeepsDescription(string memoLine, string expected)
        {
            WriteOfxFile(memoLine);

            Extract extract = Parser.GenerateExtract(ofxFile);

            Assert.AreEqual(expected, extract.Transactions[0].Description);
        }

        [TestMethod]
        [DataRow("ABC123")]
        [DataRow("14102025184012020000")] // Sent by Santander: exceeds the limit of long.
        public void GenerateExtract_InvalidChecknum_AddsImportingError(string checknum)
        {
            WriteOfxFile("<CHECKNUM>" + checknum);

            Extract extract = Parser.GenerateExtract(ofxFile);

            Assert.AreEqual(1, extract.Transactions.Count);
            Assert.AreEqual(0L, extract.Transactions[0].Checksum);
            Assert.AreEqual(1, extract.ImportingErrors.Count);
            StringAssert.Contains(extract.ImportingErrors[0], checknum);
        }

        [TestMethod]
        public void GenerateExtract_ValidChecknum_ReadsValue()
        {
            WriteOfxFile("<CHECKNUM>1234");

            Extract extract = Parser.GenerateExtract(ofxFile);

            Assert.AreEqual(1234L, extract.Transactions[0].Checksum);
            Assert.AreEqual(0, extract.ImportingErrors.Count);
        }

        private void WriteOfxFile(string transactionLine)
        {
            File.WriteAllLines(ofxFile, new[]
            {
                "OFXHEADER:100",
                "<OFX>",
                "<STMTTRN>",
                "<TRNTYPE>DEBIT",
                transactionLine,
                "</STMTTRN>",
                "</OFX>"
            });
        }
    }
}
