using System;
using System.Collections.Generic;

namespace OFXParser.Entities
{
    /// <summary>
    /// Represents a financial extract parsed from an OFX file, containing account information,
    /// transactions, balances, and import status.
    /// </summary>
    public class Extract
    {
        /// <summary>
        /// Gets or sets the header information of the extract.
        /// </summary>
        public HeaderExtract Header { get; set; }

        /// <summary>
        /// Gets or sets the bank account information.
        /// </summary>
        public BankAccount BankAccount { get; set; }

        /// <summary>
        /// Gets or sets the status of the extract.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the initial date of the extract period.
        /// </summary>
        public DateTime InitialDate { get; set; }

        /// <summary>
        /// Gets or sets the final date of the extract period.
        /// </summary>
        public DateTime FinalDate { get; set; }

        /// <summary>
        /// Gets or sets the final balance of the extract.
        /// </summary>
        public double FinalBalance { get; set; }

        /// <summary>
        /// Gets or sets the list of transactions in the extract.
        /// </summary>
        public IList<Transaction> Transactions { get; set; }

        /// <summary>
        /// Gets the list of errors encountered during import.
        /// </summary>
        public IList<string> ImportingErrors { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Extract"/> class with header, account, status, and period dates.
        /// </summary>
        /// <param name="header">Header information.</param>
        /// <param name="bankAccount">Bank account information.</param>
        /// <param name="status">Status of the extract.</param>
        /// <param name="initialDate">Initial date of the extract period.</param>
        /// <param name="finalDate">Final date of the extract period.</param>
        public Extract(HeaderExtract header, BankAccount bankAccount,
            string status, DateTime initialDate, DateTime finalDate)
        {
            Init(header, bankAccount, status);

            this.InitialDate = initialDate;
            this.FinalDate = finalDate;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Extract"/> class with header, account, and status.
        /// </summary>
        /// <param name="header">Header information.</param>
        /// <param name="bankAccount">Bank account information.</param>
        /// <param name="status">Status of the extract.</param>
        public Extract(HeaderExtract header, BankAccount bankAccount, string status)
        {
            Init(header, bankAccount, status);
        }

        /// <summary>
        /// Initializes the extract with header, account, and status.
        /// </summary>
        /// <param name="header">Header information.</param>
        /// <param name="bankAccount">Bank account information.</param>
        /// <param name="status">Status of the extract.</param>
        private void Init(HeaderExtract header, BankAccount bankAccount, string status)
        {
            this.Header = header;
            this.BankAccount = bankAccount;
            this.Status = status;
            this.Transactions = new List<Transaction>();
            this.ImportingErrors = new List<string>();
        }

        /// <summary>
        /// Adds a transaction to the extract.
        /// </summary>
        /// <param name="transaction">The transaction to add.</param>
        public void AddTransaction(Transaction transaction)
        {
            if (this.Transactions == null)
                this.Transactions = new List<Transaction>();

            this.Transactions.Add(transaction);
        }
    }
}
