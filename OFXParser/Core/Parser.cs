using System;
using System.Text;
using System.IO;
using System.Xml;
using OFXParser.Core;
using OFXParser.Entities;

namespace OFXParser
{
    public static class Parser
    {
        /// <summary>
        /// This method translate an OFX file to XML tags, independent of the content.
        /// </summary>
        /// <param name="ofxSourceFile">OFX source file</param>
        /// <returns>XML tags in StringBuilder object.</returns>
        private static StringBuilder TranslateToXml(string ofxSourceFile)
        {
            StringBuilder result = new StringBuilder();
            int level = 0;
            string line;

            if (!File.Exists(ofxSourceFile))
                throw new FileNotFoundException("OFX source file not found: " + ofxSourceFile);

            StreamReader sr = File.OpenText(ofxSourceFile);
            while ((line = sr.ReadLine()) != null)
            {
                line = line.Trim();

                if (line.StartsWith("</") && line.EndsWith(">"))
                {
                    AddTabs(result, level, true);
                    level--;
                    result.Append(line);
                }
                else if (line.StartsWith("<") && line.EndsWith(">"))
                {
                    //ADJUST FOR POSSIBLE (BUT NOT ALLOWED) EMPTY OFX TAGS
                    if (line == "<BALAMT>" || line == "<PRINYTD>" || line == "<PRINLTD>")
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
            sr.Close();

            return result;
        }

        /// <summary>
        /// Extract object with OFX file data. This method checks the OFX file.
        /// </summary>
        /// <param name="ofxSourceFile">Full path of OFX file</param>
        /// <returns>Extract object with OFX file data.</returns>
        public static Extract GenerateExtract(string ofxSourceFile)
        {
            return GenerateExtract(ofxSourceFile, new ParserSettings());
        }

        public static Extract GenerateExtract(string ofxSourceFile, ParserSettings settings)
        {
            ExportToXml(ofxSourceFile, ofxSourceFile + ".xml"); // Translating to XML file

            XmlTextReader xmlTextReader = new XmlTextReader(ofxSourceFile + ".xml"); // Reading XML

            return GetExtractByXmlExported(xmlTextReader, settings);
        }

        private static Extract GetExtractByXmlExported(XmlTextReader xmlTextReader, ParserSettings settings)
        {
            if (settings == null)
                settings = new ParserSettings();

            // Variables used by Parser
            string currentElement = "";
            Transaction currentTransaction = null;

            // Variables used to read XML
            HeaderExtract header = new HeaderExtract();
            BankAccount bankAccount = new BankAccount();
            Extract extract = new Extract(header, bankAccount, "");

            bool hasHeader = false;
            bool hasAccountInfoData = false;
            try
            {
                while (xmlTextReader.Read())
                {
                    if (xmlTextReader.NodeType == XmlNodeType.EndElement)
                    {
                        switch (xmlTextReader.Name)
                        {
                            case "STMTTRN":
                                if (currentTransaction != null)
                                {
                                    extract.AddTransaction(currentTransaction);
                                    currentTransaction = null;
                                }
                                break;
                        }
                    }
                    if (xmlTextReader.NodeType == XmlNodeType.Element)
                    {
                        currentElement = xmlTextReader.Name;

                        switch (currentElement)
                        {
                            case "STMTTRN":
                                currentTransaction = new Transaction();
                                break;
                        }
                    }
                    if (xmlTextReader.NodeType == XmlNodeType.Text)
                    {
                        switch (currentElement)
                        {
                            case "DTSERVER":
                                header.ServerDate = ConvertOfxDateToDateTime(xmlTextReader.Value, extract);
                                hasHeader = true;
                                break;
                            case "LANGUAGE":
                                header.Language = xmlTextReader.Value;
                                hasHeader = true;
                                break;
                            case "ORG":
                                header.BankName = xmlTextReader.Value;
                                hasHeader = true;
                                break;
                            case "DTSTART":
                                extract.InitialDate = ConvertOfxDateToDateTime(xmlTextReader.Value, extract);
                                break;
                            case "DTEND":
                                extract.FinalDate = ConvertOfxDateToDateTime(xmlTextReader.Value, extract);
                                break;
                            case "BANKID":
                                bankAccount.Bank = new Bank(GetBankId(xmlTextReader.Value, extract), "");
                                hasAccountInfoData = true;
                                break;
                            case "BRANCHID":
                                bankAccount.AgencyCode = xmlTextReader.Value;
                                hasAccountInfoData = true;
                                break;
                            case "ACCTID":
                                bankAccount.AccountCode = xmlTextReader.Value;
                                hasAccountInfoData = true;
                                break;
                            case "ACCTTYPE":
                                bankAccount.Type = xmlTextReader.Value;
                                hasAccountInfoData = true;
                                break;
                            case "TRNTYPE":
                                if (currentTransaction != null) currentTransaction.Type = xmlTextReader.Value;
                                break;
                            case "DTPOSTED":
                                if (currentTransaction != null) currentTransaction.Date = ConvertOfxDateToDateTime(xmlTextReader.Value, extract);
                                break;
                            case "TRNAMT":
                                if (currentTransaction != null) currentTransaction.TransactionValue = GetTransactionValue(xmlTextReader.Value, extract, settings);
                                break;
                            case "FITID":
                                if (currentTransaction != null) currentTransaction.Id = xmlTextReader.Value;
                                break;
                            case "CHECKNUM":
                                if (currentTransaction != null) currentTransaction.Checksum = Convert.ToInt64(xmlTextReader.Value);
                                break;
                            case "MEMO":
                                if (currentTransaction != null) currentTransaction.Description = string.IsNullOrEmpty(xmlTextReader.Value) ? "" : xmlTextReader.Value.Trim().Replace("  ", " ");
                                break;
                        }
                    }
                }
            }
            catch (XmlException xe)
            {
                throw new OFXParserException($"Invalid OFX file! Internal message: {xe.Message}");
            }
            finally
            {
                xmlTextReader.Close();
            }

            if ((settings.IsValidateHeader && hasHeader == false) ||
                (settings.IsValidateAccountData && hasAccountInfoData == false))
            {
                throw new OFXParserException("Invalid OFX file!");
            }

            return extract;
        }

        /// <summary>
        /// This method translate an OFX file to XML file, independent of the content.
        /// </summary>
        /// <param name="ofxSourceFile">Path of OFX source file</param>
        /// <param name="xmlNewFile">Path of the XML file, internally generated.</param>
        private static void ExportToXml(string ofxSourceFile, string xmlNewFile)
        {
            if (System.IO.File.Exists(ofxSourceFile))
            {
                if (xmlNewFile.ToLower().EndsWith(".xml"))
                {
                    // Translating the OFX file to XML format
                    StringBuilder ofxTranslated = TranslateToXml(ofxSourceFile);

                    // Verifying if target file exists
                    if (System.IO.File.Exists(xmlNewFile))
                    {
                        System.IO.File.Delete(xmlNewFile);
                    }

                    // Writing data into target file
                    StreamWriter sw = File.CreateText(xmlNewFile);
                    sw.WriteLine(@"<?xml version=""1.0""?>");
                    sw.WriteLine(ofxTranslated.ToString());
                    sw.Close();
                }
                else
                {
                    throw new ArgumentException("Name of new XML file is not valid: " + xmlNewFile);
                }
            }
            else
            {
                throw new FileNotFoundException("OFX source file not found: " + ofxSourceFile);
            }
        }

        /// <summary>
        /// This method return the correct closing tag string 
        /// </summary>
        /// <param name="content">Content of analysis</param>
        /// <returns>String with ending tag.</returns>
        private static string ReturnFinalTag(string content)
        {
            string finalTagReturn = "";

            if ((content.IndexOf("<") != -1) && (content.IndexOf(">") != -1))
            {
                int position1 = content.IndexOf("<");
                int position2 = content.IndexOf(">");
                if ((position2 - position1) > 2)
                {
                    finalTagReturn = content.Substring(position1, (position2 - position1) + 1);
                    finalTagReturn = finalTagReturn.Replace("<", "</");
                }
            }
            return finalTagReturn;
        }

        /// <summary>
        /// This method add tabs into lines of xml file, to best identation.
        /// </summary>
        /// <param name="stringObject">Line of content</param>
        /// <param name="lengthTabs">Length os tabs to add into content</param>
        /// <param name="newLine">Is it new line?</param>
        private static void AddTabs(StringBuilder stringObject, int lengthTabs, bool newLine)
        {
            if (newLine)
                stringObject.AppendLine();

            for (int j = 1; j < lengthTabs; j++)
            {
                stringObject.Append("\t");
            }
        }

        /// <summary>
        /// Method that return a part of date. Is is used internally when the dates are reading.
        /// </summary>
        /// <param name="ofxDate">Date</param>
        /// <param name="partDateTime">Part of date</param>
        /// <returns></returns>
        private static int GetPartOfOfxDate(string ofxDate, PartDateTime partDateTime)
        {
            if (partDateTime == PartDateTime.YEAR)
            {
                return int.Parse(ofxDate.Substring(0, 4));

            }
            else if (partDateTime == PartDateTime.MONTH)
            {
                return int.Parse(ofxDate.Substring(4, 2));

            }
            else if (partDateTime == PartDateTime.DAY)
            {
                return int.Parse(ofxDate.Substring(6, 2));

            }
            else if (partDateTime == PartDateTime.HOUR)
            {
                if (ofxDate.Length >= 10)
                    return int.Parse(ofxDate.Substring(8, 2));

            }
            else if (partDateTime == PartDateTime.MINUTE)
            {
                if (ofxDate.Length >= 12)
                    return int.Parse(ofxDate.Substring(10, 2));

            }
            else if (partDateTime == PartDateTime.SECOND)
            {
                if (ofxDate.Length >= 14)
                    return int.Parse(ofxDate.Substring(12, 2));

            }
            return 0;
        }

        /// <summary>
        /// Method that convert a OFX date string to DateTime object.
        /// </summary>
        /// <param name="ofxDate"></param>
        /// <param name="extract"></param>
        /// <returns></returns>
        private static DateTime ConvertOfxDateToDateTime(String ofxDate, Extract extract)
        {
            DateTime dateTimeReturned = DateTime.MinValue;
            try
            {
                int year = GetPartOfOfxDate(ofxDate, PartDateTime.YEAR);
                int month = GetPartOfOfxDate(ofxDate, PartDateTime.MONTH);
                int day = GetPartOfOfxDate(ofxDate, PartDateTime.DAY);
                int hour = GetPartOfOfxDate(ofxDate, PartDateTime.HOUR);
                int minute = GetPartOfOfxDate(ofxDate, PartDateTime.MINUTE);
                int second = GetPartOfOfxDate(ofxDate, PartDateTime.SECOND);

                dateTimeReturned = new DateTime(year, month, day, hour, minute, second);
            }
            catch (Exception)
            {
                extract.ImportingErrors.Add(string.Format("Invalid datetime {0}", ofxDate));
            }
            return dateTimeReturned;
        }

        private static int GetBankId(string value, Extract extract)
        {
            if (int.TryParse(value, out int bankId))
                return bankId;

            extract.ImportingErrors.Add(string.Format("Bank id isn't numeric value: {0}", value));
            return 0;
        }

        private static double GetTransactionValue(string value, Extract extract, ParserSettings settings)
        {
            try
            {
                if (settings.CustomConverterCurrency != null)
                    return settings.CustomConverterCurrency.Invoke(value);
                else
                    return Convert.ToDouble(value.Replace('.', ','));
            }
            catch (Exception)
            {
                extract.ImportingErrors.Add(string.Format("Invalid transaction value/amount: {0}", value));
            }
            return 0;
        }
    }
}
