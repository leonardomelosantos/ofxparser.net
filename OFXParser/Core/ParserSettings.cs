using System;

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
    }
}
