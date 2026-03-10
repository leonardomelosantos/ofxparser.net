using OFXParser.Entities;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace OFXParser.Core;

/// <summary>
/// Provides methods to parse OFX (Open Financial Exchange) files into structured .NET objects.
/// </summary>
public static class Parser
{
    #region Constants

    private const string MESSAGE_BANK_ID_ISNT_NUMERIC_VALUE = "Bank id isn't numeric value: {0}";
    private const string MESSAGE_INVALID_DATETIME = "Invalid datetime: {0}";
    private const string MESSAGE_INVALID_TRANSACTION_VALUE_AMOUNT = "Invalid transaction value/amount: {0}";
    private const string MESSAGE_OFX_FILE_NOT_FOUND = "OFX source file not found: {0}";

    #endregion

    #region Public methods

    /// <summary>
    /// Parses an OFX file and returns an <see cref="Extract"/> object with the file data.
    /// Uses default <see cref="ParserSettings"/>.
    /// </summary>
    /// <param name="ofxSourceFile">Full path of the OFX file.</param>
    /// <returns>An <see cref="Extract"/> object with OFX file data.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the OFX file is not found.</exception>
    /// <exception cref="OFXParserException">Thrown when the OFX file has an invalid format.</exception>
    public static Extract GenerateExtract(string ofxSourceFile)
        => GenerateExtract(ofxSourceFile, new ParserSettings());

    /// <summary>
    /// Parses an OFX file and returns an <see cref="Extract"/> object with the file data.
    /// </summary>
    /// <param name="ofxSourceFile">Full path of the OFX file.</param>
    /// <param name="settings">Parser settings to control validation and currency conversion.</param>
    /// <returns>An <see cref="Extract"/> object with OFX file data.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the OFX file is not found.</exception>
    /// <exception cref="OFXParserException">Thrown when the OFX file has an invalid format.</exception>
    public static Extract GenerateExtract(string ofxSourceFile, ParserSettings settings)
    {
        var xmlContent = TranslateOfxToXml(ofxSourceFile);
        return ParseXmlContent(xmlContent, settings);
    }

    /// <summary>
    /// Asynchronously parses an OFX file and returns an <see cref="Extract"/> object.
    /// Uses default <see cref="ParserSettings"/>.
    /// </summary>
    /// <param name="ofxSourceFile">Full path of the OFX file.</param>
    /// <returns>A task that resolves to an <see cref="Extract"/> object with OFX file data.</returns>
    public static Task<Extract> GenerateExtractAsync(string ofxSourceFile)
        => GenerateExtractAsync(ofxSourceFile, new ParserSettings());

    /// <summary>
    /// Asynchronously parses an OFX file and returns an <see cref="Extract"/> object.
    /// </summary>
    /// <param name="ofxSourceFile">Full path of the OFX file.</param>
    /// <param name="settings">Parser settings to control validation and currency conversion.</param>
    /// <returns>A task that resolves to an <see cref="Extract"/> object with OFX file data.</returns>
    public static async Task<Extract> GenerateExtractAsync(string ofxSourceFile, ParserSettings settings)
    {
        var xmlContent = await TranslateOfxToXmlAsync(ofxSourceFile);
        return ParseXmlContent(xmlContent, settings);
    }

    #endregion

    #region Private methods

    /// <summary>
    /// Translates an OFX file into XML format in-memory (no temp files).
    /// </summary>
    private static StringBuilder TranslateOfxToXml(string ofxSourceFile)
    {
        if (!File.Exists(ofxSourceFile))
            throw new FileNotFoundException(string.Format(MESSAGE_OFX_FILE_NOT_FOUND, ofxSourceFile));

        var result = new StringBuilder();
        int level = 0;

        using var streamReader = File.OpenText(ofxSourceFile);
        string? line;
        while ((line = streamReader.ReadLine()) != null)
        {
            ProcessOfxLine(line.Trim(), result, ref level);
        }

        return result;
    }

    /// <summary>
    /// Asynchronously translates an OFX file into XML format in-memory.
    /// </summary>
    private static async Task<StringBuilder> TranslateOfxToXmlAsync(string ofxSourceFile)
    {
        if (!File.Exists(ofxSourceFile))
            throw new FileNotFoundException(string.Format(MESSAGE_OFX_FILE_NOT_FOUND, ofxSourceFile));

        var result = new StringBuilder();
        int level = 0;

        using var streamReader = File.OpenText(ofxSourceFile);
        string? line;
        while ((line = await streamReader.ReadLineAsync()) != null)
        {
            ProcessOfxLine(line.Trim(), result, ref level);
        }

        return result;
    }

    private static void ProcessOfxLine(string line, StringBuilder result, ref int level)
    {
        if (line.StartsWith("</") && line.EndsWith(">"))
        {
            AddTabs(result, level, true);
            level--;
            result.Append(line);
        }
        else if (line.StartsWith("<") && line.EndsWith(">"))
        {
            // Handle possible (but not allowed) empty OFX tags
            if (line is "<BALAMT>" or "<PRINYTD>" or "<PRINLTD>")
            {
                AddTabs(result, level + 1, true);
                result.Append(line);
                result.Append(ReturnFinalTag(line));
            }
            else
            {
                level++;
                AddTabs(result, level, true);
                result.Append(line);
            }
        }
        else if (line.StartsWith("<") && !line.EndsWith(">"))
        {
            AddTabs(result, level + 1, true);
            result.Append(line);
            result.Append(ReturnFinalTag(line));
        }
    }

    private static Extract ParseXmlContent(StringBuilder xmlContent, ParserSettings settings)
    {
        var fullXml = $"<?xml version=\"1.0\"?>{Environment.NewLine}{xmlContent}";
        using var stringReader = new StringReader(fullXml);
        var xmlSettings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Ignore,
            IgnoreWhitespace = true,
            IgnoreComments = true
        };
        using var xmlReader = XmlReader.Create(stringReader, xmlSettings);
        return GetExtractByXmlReader(xmlReader, settings);
    }

    private static Extract GetExtractByXmlReader(XmlReader xmlReader, ParserSettings settings)
    {
        string currentElement = string.Empty;
        Transaction? currentTransaction = null;

        var header = new HeaderExtract();
        var bankAccount = new BankAccount();
        var extract = new Extract(header, bankAccount, string.Empty);

        bool hasHeader = false;
        bool hasAccountInfoData = false;

        try
        {
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.EndElement)
                {
                    if (xmlReader.Name == "STMTTRN" && currentTransaction != null)
                    {
                        extract.AddTransaction(currentTransaction);
                        currentTransaction = null;
                    }
                }
                else if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    currentElement = xmlReader.Name;

                    if (currentElement == "STMTTRN")
                        currentTransaction = new Transaction();
                }
                else if (xmlReader.NodeType == XmlNodeType.Text)
                {
                    string value = xmlReader.Value;

                    switch (currentElement)
                    {
                        case "DTSERVER":
                            header.ServerDate = ConvertOfxDateToDateTime(value, extract);
                            hasHeader = true;
                            break;
                        case "LANGUAGE":
                            header.Language = value;
                            hasHeader = true;
                            break;
                        case "ORG":
                            header.BankName = value;
                            hasHeader = true;
                            break;
                        case "DTSTART":
                            extract.InitialDate = ConvertOfxDateToDateTime(value, extract);
                            break;
                        case "DTEND":
                            extract.FinalDate = ConvertOfxDateToDateTime(value, extract);
                            break;
                        case "BANKID":
                            bankAccount.Bank = new Bank(TryGetBankId(value, extract), string.Empty);
                            hasAccountInfoData = true;
                            break;
                        case "BRANCHID":
                            bankAccount.AgencyCode = value;
                            hasAccountInfoData = true;
                            break;
                        case "ACCTID":
                            bankAccount.AccountCode = value;
                            hasAccountInfoData = true;
                            break;
                        case "ACCTTYPE":
                            bankAccount.Type = value;
                            hasAccountInfoData = true;
                            break;
                        case "TRNTYPE" when currentTransaction != null:
                            currentTransaction.Type = value;
                            break;
                        case "DTPOSTED" when currentTransaction != null:
                            currentTransaction.Date = ConvertOfxDateToDateTime(value, extract);
                            break;
                        case "TRNAMT" when currentTransaction != null:
                            currentTransaction.TransactionValue = ParseDecimalValue(value, extract, settings);
                            break;
                        case "FITID" when currentTransaction != null:
                            currentTransaction.Id = value;
                            break;
                        case "CHECKNUM" when currentTransaction != null:
                            if (long.TryParse(value, out long checksum))
                                currentTransaction.Checksum = checksum;
                            break;
                        case "MEMO" when currentTransaction != null:
                            currentTransaction.Description = string.IsNullOrEmpty(value)
                                ? string.Empty
                                : value.Trim().Replace("  ", " ", StringComparison.Ordinal);
                            break;
                        case "BALAMT":
                            extract.FinalBalance = ParseDecimalValue(value, extract, settings);
                            break;
                    }
                }
            }
        }
        catch (XmlException xe)
        {
            throw new OFXParserException($"Invalid OFX file! Internal message: {xe.Message}");
        }

        if ((settings.IsValidateHeader && !hasHeader) ||
            (settings.IsValidateAccountData && !hasAccountInfoData))
        {
            throw new OFXParserException("Invalid OFX file!");
        }

        return extract;
    }

    private static string ReturnFinalTag(string content)
    {
        int position1 = content.IndexOf('<');
        int position2 = content.IndexOf('>');

        if (position1 == -1 || position2 == -1 || (position2 - position1) <= 2)
            return string.Empty;

        string tag = content[position1..(position2 + 1)];
        return tag.Replace("<", "</", StringComparison.Ordinal);
    }

    private static void AddTabs(StringBuilder stringObject, int lengthTabs, bool newLine)
    {
        if (newLine)
            stringObject.AppendLine();

        for (int j = 1; j < lengthTabs; j++)
            stringObject.Append('\t');
    }

    private static int GetPartOfOfxDate(string ofxDate, PartDateTime partDateTime) => partDateTime switch
    {
        PartDateTime.YEAR   => int.Parse(ofxDate[0..4], CultureInfo.InvariantCulture),
        PartDateTime.MONTH  => int.Parse(ofxDate[4..6], CultureInfo.InvariantCulture),
        PartDateTime.DAY    => int.Parse(ofxDate[6..8], CultureInfo.InvariantCulture),
        PartDateTime.HOUR   => ofxDate.Length >= 10 ? int.Parse(ofxDate[8..10], CultureInfo.InvariantCulture) : 0,
        PartDateTime.MINUTE => ofxDate.Length >= 12 ? int.Parse(ofxDate[10..12], CultureInfo.InvariantCulture) : 0,
        PartDateTime.SECOND => ofxDate.Length >= 14 ? int.Parse(ofxDate[12..14], CultureInfo.InvariantCulture) : 0,
        _ => 0
    };

    private static DateTime ConvertOfxDateToDateTime(string ofxDate, Extract extract)
    {
        try
        {
            int year   = GetPartOfOfxDate(ofxDate, PartDateTime.YEAR);
            int month  = GetPartOfOfxDate(ofxDate, PartDateTime.MONTH);
            int day    = GetPartOfOfxDate(ofxDate, PartDateTime.DAY);
            int hour   = GetPartOfOfxDate(ofxDate, PartDateTime.HOUR);
            int minute = GetPartOfOfxDate(ofxDate, PartDateTime.MINUTE);
            int second = GetPartOfOfxDate(ofxDate, PartDateTime.SECOND);

            return new DateTime(year, month, day, hour, minute, second, DateTimeKind.Unspecified);
        }
        catch (Exception)
        {
            extract.ImportingErrors.Add(string.Format(CultureInfo.InvariantCulture, MESSAGE_INVALID_DATETIME, ofxDate));
            return DateTime.MinValue;
        }
    }

    private static int TryGetBankId(string value, Extract extract)
    {
        if (int.TryParse(value, out int bankId))
            return bankId;

        extract.ImportingErrors.Add(string.Format(CultureInfo.InvariantCulture, MESSAGE_BANK_ID_ISNT_NUMERIC_VALUE, value));
        return 0;
    }

    private static decimal ParseDecimalValue(string value, Extract extract, ParserSettings settings)
    {
        try
        {
            if (settings.CustomConverterCurrency != null)
                return settings.CustomConverterCurrency(value);

            if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal result))
                return result;

            // Fallback: try replacing comma decimal separator
            if (decimal.TryParse(value.Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out result))
                return result;

            throw new FormatException($"Cannot parse '{value}' as decimal.");
        }
        catch (Exception)
        {
            extract.ImportingErrors.Add(string.Format(CultureInfo.InvariantCulture, MESSAGE_INVALID_TRANSACTION_VALUE_AMOUNT, value));
            return 0m;
        }
    }

    #endregion
}
