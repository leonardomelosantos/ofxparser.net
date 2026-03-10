namespace OFXParser.Core;

/// <summary>
/// Exception thrown when an OFX file cannot be parsed due to format or content errors.
/// </summary>
public class OFXParserException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="OFXParserException"/> with the specified message.
    /// </summary>
    /// <param name="message">The error message.</param>
    public OFXParserException(string message) : base(message) { }
}
