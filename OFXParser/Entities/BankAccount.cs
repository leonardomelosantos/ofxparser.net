namespace OFXParser.Entities
{
    public class BankAccount
    {
        public string Type { get; set; }

        public string AgencyCode { get; set; }

        public Bank Bank { get; set; }

        public string AccountCode { get; set; }
    }
}
