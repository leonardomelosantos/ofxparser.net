using System;

namespace OFXParser.Entities
{
    public class Transaction
    {
        public String Type { get; set; }

        public DateTime Date { get; set; }

        public double TransactionValue { get; set; }

        public String Id { get; set; }

        public String Description { get; set; }

        public long Checksum { get; set; }

    }
}
