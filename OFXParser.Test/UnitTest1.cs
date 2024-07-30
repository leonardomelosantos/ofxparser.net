using Microsoft.VisualStudio.TestTools.UnitTesting;
using OFXParser.Core;
using OFXParser.Entities;

namespace OFXParser.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Extract extract = OFXParser.Parser.GenerateExtract("D:\\extrato.ofx", new ParserSettings());
        }
        
        [TestMethod]
        public void TestCheckSaldo()
        {
            Extract extract = OFXParser.Parser.GenerateExtract("D:\\extrato.ofx", new ParserSettings());

            Assert.IsNotNull(extract.Balance);
        }
    }
}
