namespace OFXParser.Entities;

/// <summary>
/// Represents a bank account parsed from an OFX file.
/// </summary>
public class BankAccount
{
    /// <summary>Gets or sets the account type (e.g., CHECKING, SAVINGS).</summary>
    public string? Type { get; set; }

    /// <summary>Gets or sets the branch/agency code.</summary>
    public string? AgencyCode { get; set; }

    /// <summary>Gets or sets the bank associated with this account.</summary>
    public Bank? Bank { get; set; }

    /// <summary>Gets or sets the account number.</summary>
    public string? AccountCode { get; set; }
}
