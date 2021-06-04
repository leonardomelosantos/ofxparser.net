using System;
using System.Collections.Generic;
using System.Text;

namespace OFXParser.Core
{
    public class ParserSettings
    {
        public bool IsValidateHeader { get; set; }
        public bool IsValidateAccountData { get; set; }
		
        /// <summary>
        /// Method reference to execute your cutom strategy convertion.
        /// </summary>
        public Func<string, double> CustomConverterCurrency { get; set; }
	}
}
