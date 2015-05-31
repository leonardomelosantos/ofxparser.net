using System;

namespace OFXParser.Entities
{
    public class BankAccount
    {

        public String Type { get; set; }

        public String AgencyCode { get; set; }

        public Bank Bank { get; set; }

        public String AccountCode { get; set; }

    }
}
