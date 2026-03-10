namespace OFXParser.Entities;

/// <summary>
/// Represents a financial extract parsed from an OFX file, containing account information,
/// transactions, balances, and import errors.
/// </summary>
public class Extract
{
    /// <summary>Gets or sets the header information of the extract.</summary>
    public HeaderExtract Header { get; set; }

    /// <summary>Gets or sets the bank account information.</summary>
    public BankAccount BankAccount { get; set; }

    /// <summary>Gets or sets the extract status.</summary>
    public string? Status { get; set; }

    /// <summary>Gets or sets the start date of the extract period.</summary>
    public DateTime InitialDate { get; set; }

    /// <summary>Gets or sets the end date of the extract period.</summary>
    public DateTime FinalDate { get; set; }

    /// <summary>Gets or sets the closing balance at the end of the extract period.</summary>
    public decimal FinalBalance { get; set; }

    /// <summary>Gets or sets the list of transactions in the extract.</summary>
    public IList<Transaction> Transactions { get; set; } = [];

    /// <summary>Gets the list of non-fatal errors encountered during parsing.</summary>
    public IList<string> ImportingErrors { get; } = [];

    /// <summary>
    /// Initializes a new instance of <see cref="Extract"/> with header, account, and status.
    /// </summary>
    public Extract(HeaderExtract header, BankAccount bankAccount, string? status)
    {
        Header = header;
        BankAccount = bankAccount;
        Status = status;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="Extract"/> with header, account, status, and period dates.
    /// </summary>
    public Extract(HeaderExtract header, BankAccount bankAccount, string? status, DateTime initialDate, DateTime finalDate)
        : this(header, bankAccount, status)
    {
        InitialDate = initialDate;
        FinalDate = finalDate;
    }

    /// <summary>Adds a transaction to the extract.</summary>
    /// <param name="transaction">The transaction to add.</param>
    public void AddTransaction(Transaction transaction)
    {
        Transactions ??= [];
        Transactions.Add(transaction);
    }
}
