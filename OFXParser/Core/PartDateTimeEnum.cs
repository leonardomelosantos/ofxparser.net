namespace OFXParser.Core;

/// <summary>
/// Represents the component parts of an OFX date string (YYYYMMDDHHMMSS).
/// </summary>
internal enum PartDateTime
{
    DAY,
    MONTH,
    YEAR,
    HOUR,
    MINUTE,
    SECOND
}
