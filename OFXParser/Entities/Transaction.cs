using System;

namespace OFXParser.Entities
{
    public class Transaction
    {
        public string Type { get; set; }

        public DateTime Date { get; set; }

        public double TransactionValue { get; set; }

        public string Id { get; set; }

        public string Description => !string.IsNullOrEmpty(Memo) ? Memo : Name;

        public long Checksum { get; set; }

        public string Memo { get; set; }

        public string Name { get; set; }
    }
}
