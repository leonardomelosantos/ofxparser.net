namespace OFXParser.Entities;

/// <summary>
/// Represents a bank identified by a numeric code and name.
/// </summary>
public class Bank
{
    /// <summary>Gets or sets the bank's numeric code.</summary>
    public int Code { get; set; }

    /// <summary>Gets or sets the bank's name.</summary>
    public string Name { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="Bank"/> with the specified code and name.
    /// </summary>
    /// <param name="code">Numeric bank code.</param>
    /// <param name="name">Bank name.</param>
    public Bank(int code, string name)
    {
        Code = code;
        Name = name;
    }
}
