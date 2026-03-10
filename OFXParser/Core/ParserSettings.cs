namespace OFXParser.Core;

/// <summary>
/// Configuration settings for the OFX parser.
/// </summary>
public class ParserSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether to validate that a header section exists in the OFX file.
    /// Default is <c>false</c>.
    /// </summary>
    public bool IsValidateHeader { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to validate that account data exists in the OFX file.
    /// Default is <c>false</c>.
    /// </summary>
    public bool IsValidateAccountData { get; set; }

    /// <summary>
    /// Gets or sets an optional custom currency conversion function.
    /// When provided, this function is used instead of the default culture-invariant decimal parser.
    /// </summary>
    public Func<string, decimal>? CustomConverterCurrency { get; set; }
}
