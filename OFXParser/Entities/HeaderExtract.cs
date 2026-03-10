namespace OFXParser.Entities;

/// <summary>
/// Represents the header section of an OFX file.
/// </summary>
public class HeaderExtract
{
    /// <summary>Gets or sets the header status.</summary>
    public string? Status { get; set; }

    /// <summary>Gets or sets the language code (e.g., ENG, POR).</summary>
    public string? Language { get; set; }

    /// <summary>Gets or sets the server response date (DTSERVER).</summary>
    public DateTime ServerDate { get; set; }

    /// <summary>Gets or sets the organization or bank name (ORG).</summary>
    public string? BankName { get; set; }

    /// <summary>Initializes a new empty instance of <see cref="HeaderExtract"/>.</summary>
    public HeaderExtract() { }

    /// <summary>
    /// Initializes a new instance of <see cref="HeaderExtract"/> with all fields.
    /// </summary>
    public HeaderExtract(string? status, string? language, DateTime serverDate, string? bankName)
    {
        Status = status;
        Language = language;
        ServerDate = serverDate;
        BankName = bankName;
    }
}
