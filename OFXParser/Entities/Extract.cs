using System;
using System.Collections;
using System.Collections.Generic;

namespace OFXParser.Entities
{
    public class Extract
    {
        public HeaderExtract Header { get; set; }

        public BankAccount BankAccount { get; set; }

        public String Status { get; set; }

        public DateTime InitialDate { get; set; }

        public DateTime FinalDate { get; set; }

        public double FinalBalance { get; set; }

        public IList<Transaction> Transactions { get; set; }

        public IList<string> ImportingErrors { get; private set; }

        public Extract(HeaderExtract header, BankAccount bankAccount,
            String status, DateTime initialDate, DateTime finalDate)
        {
            Init(header, bankAccount, status);

            this.InitialDate = initialDate;
            this.FinalDate = finalDate;
        }

        public Extract(HeaderExtract header, BankAccount bankAccount,
            String status)
        {
            Init(header, bankAccount, status);
        }

        private void Init(HeaderExtract header, BankAccount bankAccount, String status)
        {
            this.Header = header;
            this.BankAccount = bankAccount;
            this.Status = status;
            this.Transactions = new List<Transaction>();
            this.ImportingErrors = new List<string>();
        }

        public void AddTransaction(Transaction transaction)
        {
            if (this.Transactions == null)
            {
                this.Transactions = new List<Transaction>();
            }
            this.Transactions.Add(transaction);
        }
    }
}
