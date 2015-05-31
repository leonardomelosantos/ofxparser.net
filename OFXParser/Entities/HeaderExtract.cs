using System;

namespace OFXParser.Entities
{
    public class HeaderExtract
    {
        public String Status { get; set; }

        public String Language { get; set; }

        public DateTime ServerDate { get; set; }

        public String BankName { get; set; }

        public HeaderExtract()
        {
        }

        public HeaderExtract(String status, String language, DateTime serverDate, String bankName)
        {
            this.Status = status;
            this.Language = language;
            this.ServerDate = serverDate;
            this.BankName = bankName;
        }

        

    }
}
