namespace OFXParser.Entities;

/// <summary>
/// Represents a financial transaction parsed from an OFX file.
/// </summary>
public class Transaction
{
    /// <summary>Gets or sets the transaction type (e.g., DEBIT, CREDIT, CHECK).</summary>
    public string? Type { get; set; }

    /// <summary>Gets or sets the date the transaction was posted.</summary>
    public DateTime Date { get; set; }

    /// <summary>Gets or sets the transaction amount. Positive values indicate credits, negative indicate debits.</summary>
    public decimal TransactionValue { get; set; }

    /// <summary>Gets or sets the unique transaction identifier (FITID).</summary>
    public string? Id { get; set; }

    /// <summary>Gets or sets the transaction description or memo.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets the check number, when applicable.</summary>
    public long Checksum { get; set; }
}
