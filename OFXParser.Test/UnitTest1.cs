﻿using OFXParser.Core;
using OFXParser.Entities;

namespace OFXParser.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            Extract extract = OFXParser.Parser.GenerateExtract("E:\\extract.ofx", new ParserSettings());
        }
    }
}
