using System;
using System.Text;

namespace OFXParser.Core
{
    public class ParserSettings
    {
        public bool IsValidateHeader { get; set; }
        public bool IsValidateAccountData { get; set; }

        /// <summary>
        /// Method reference to execute your custom convertion strategy.
        /// </summary>
        public Func<string, double> CustomConverterCurrency { get; set; }

        /// <summary>
        /// Encoding used to read the OFX file. If it is null, the file is read as UTF-8.
        /// A byte order mark (BOM) in the file takes precedence over this value.
        /// On .NET Core and .NET 5+, code pages like Windows-1252 require registering
        /// CodePagesEncodingProvider before calling Encoding.GetEncoding.
        /// </summary>
        public Encoding Encoding { get; set; }
    }
}
