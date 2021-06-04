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
        private static StringBuilder TranslateToXml(String ofxSourceFile)
        {
            StringBuilder result = new StringBuilder();
            int level = 0;
            String line;

            if (!File.Exists(ofxSourceFile))
            {
                throw new FileNotFoundException("OFX source file not found: " + ofxSourceFile);
            }

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
        public static Extract GenerateExtract(String ofxSourceFile)
        {
            return GenerateExtract(ofxSourceFile, new ParserSettings());
        }

        public static Extract GenerateExtract(String ofxSourceFile, ParserSettings settings)
		{
			// Translating to XML file
			ExportToXml(ofxSourceFile, ofxSourceFile + ".xml");
			
            // Lendo o XML efetivamente
			XmlTextReader xmlTextReader = new XmlTextReader(ofxSourceFile + ".xml");

			Extract extrato = GetExtractByXmlExported(xmlTextReader, settings);

			return extrato;
		}

		private static Extract GetExtractByXmlExported(XmlTextReader xmlTextReader, ParserSettings settings)
		{
            if (settings == null) settings = new ParserSettings();

            // Variáveis úteis para o Parse
            String elementoSendoLido = "";
			Transaction transacaoAtual = null;

			// Variávies utilizadas para a leitura do XML
			HeaderExtract cabecalho = new HeaderExtract();
			BankAccount conta = new BankAccount();
			Extract extrato = new Extract(cabecalho, conta, "");

			Boolean temCabecalho = false;
			Boolean temDadosConta = false;
			try
			{
				while (xmlTextReader.Read())
				{
					if (xmlTextReader.NodeType == XmlNodeType.EndElement)
					{
						switch (xmlTextReader.Name)
						{
							case "STMTTRN":
								if (transacaoAtual != null)
								{
									extrato.AddTransaction(transacaoAtual);
									transacaoAtual = null;
								}
								break;
						}
					}
					if (xmlTextReader.NodeType == XmlNodeType.Element)
					{
						elementoSendoLido = xmlTextReader.Name;

						switch (elementoSendoLido)
						{
							case "STMTTRN":
								transacaoAtual = new Transaction();
								break;
						}
					}
					if (xmlTextReader.NodeType == XmlNodeType.Text)
					{
						switch (elementoSendoLido)
						{
							case "DTSERVER":
								cabecalho.ServerDate = ConvertOfxDateToDateTime(xmlTextReader.Value, extrato);
								temCabecalho = true;
								break;
							case "LANGUAGE":
								cabecalho.Language = xmlTextReader.Value;
								temCabecalho = true;
								break;
							case "ORG":
								cabecalho.BankName = xmlTextReader.Value;
								temCabecalho = true;
								break;
							case "DTSTART":
								extrato.InitialDate = ConvertOfxDateToDateTime(xmlTextReader.Value, extrato);
								break;
							case "DTEND":
								extrato.FinalDate = ConvertOfxDateToDateTime(xmlTextReader.Value, extrato);
								break;
							case "BANKID":
								conta.Bank = new Bank(GetBankId(xmlTextReader.Value, extrato), "");
								temDadosConta = true;
								break;
							case "BRANCHID":
								conta.AgencyCode = xmlTextReader.Value;
								temDadosConta = true;
								break;
							case "ACCTID":
								conta.AccountCode = xmlTextReader.Value;
								temDadosConta = true;
								break;
							case "ACCTTYPE":
								conta.Type = xmlTextReader.Value;
								temDadosConta = true;
								break;
							case "TRNTYPE":
								if (transacaoAtual != null) transacaoAtual.Type = xmlTextReader.Value;
								break;
							case "DTPOSTED":
								if (transacaoAtual != null) transacaoAtual.Date = ConvertOfxDateToDateTime(xmlTextReader.Value, extrato);
								break;
							case "TRNAMT":
								if (transacaoAtual != null) transacaoAtual.TransactionValue = GetTransactionValue(xmlTextReader.Value, extrato, settings);
								break;
							case "FITID":
								if (transacaoAtual != null) transacaoAtual.Id = xmlTextReader.Value;
								break;
							case "CHECKNUM":
								if (transacaoAtual != null) transacaoAtual.Checksum = Convert.ToInt64(xmlTextReader.Value);
								break;
							case "MEMO":
								if (transacaoAtual != null) transacaoAtual.Description = string.IsNullOrEmpty(xmlTextReader.Value) ? "" : xmlTextReader.Value.Trim().Replace("  ", " ");
								break;
						}
					}
				}
			}
			catch (XmlException xe)
			{
				throw new OFXParserException("Invalid OFX file!");
			}
			finally
			{
				xmlTextReader.Close();
			}

			if ((settings.IsValidateHeader && temCabecalho == false) ||
				(settings.IsValidateAccountData && temDadosConta == false))
			{
				throw new OFXParserException("Invalid OFX file!");
			}

			return extrato;
		}

		/// <summary>
		/// This method translate an OFX file to XML file, independent of the content.
		/// </summary>
		/// <param name="ofxSourceFile">Path of OFX source file</param>
		/// <param name="xmlNewFile">Path of the XML file, internally generated.</param>
		private static void ExportToXml(String ofxSourceFile, String xmlNewFile)
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
            } else {
                throw new FileNotFoundException("OFX source file not found: " + ofxSourceFile);
            }
        }

        /// <summary>
        /// This method return the correct closing tag string 
        /// </summary>
        /// <param name="content">Content of analysis</param>
        /// <returns>String with ending tag.</returns>
        private static String ReturnFinalTag(String content)
        {
            String finalReturn = "";

            if ((content.IndexOf("<") != -1) && (content.IndexOf(">") != -1))
            {
                int position1 = content.IndexOf("<");
                int position2 = content.IndexOf(">");
                if ((position2 - position1) > 2)
                {
                    finalReturn = content.Substring(position1, (position2 - position1) + 1);
                    finalReturn = finalReturn.Replace("<", "</");
                }
            }

            return finalReturn;
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
            {
                stringObject.AppendLine();
            }
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
        private static int GetPartOfOfxDate(String ofxDate, PartDateTime partDateTime)
        {
            int result = 0;

            if (partDateTime == PartDateTime.YEAR){
                result = Int32.Parse(ofxDate.Substring(0,4));

            } else if (partDateTime == PartDateTime.MONTH) {
                result = Int32.Parse(ofxDate.Substring(4, 2));

            } if (partDateTime == PartDateTime.DAY) {
                result = Int32.Parse(ofxDate.Substring(6, 2));

            } if (partDateTime == PartDateTime.HOUR) {
                if (ofxDate.Length >= 10)
                {
                    result = Int32.Parse(ofxDate.Substring(8, 2));
                }
                else
                {
                    result = 0;
                }

            } if (partDateTime == PartDateTime.MINUTE) {
                if (ofxDate.Length >= 12)
                {
                    result = Int32.Parse(ofxDate.Substring(10, 2));
                }
                else
                {
                    result = 0;
                }

            } if (partDateTime == PartDateTime.SECOND) {
                if (ofxDate.Length >= 14)
                {
                    result = Int32.Parse(ofxDate.Substring(12, 2));
                }
                else
                {
                    result = 0;
                }
            }
            return result;
        }

        /// <summary>
        /// Method that convert a OFX date string to DateTime object.
        /// </summary>
        /// <param name="ofxDate"></param>
        /// <param name="extract"></param>
        /// <returns></returns>
        private static DateTime ConvertOfxDateToDateTime(String ofxDate, Extract extract) {
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
            catch (Exception ex)
            {
                extract.ImportingErrors.Add(string.Format("Invalid datetime {0}", ofxDate));
            }
            return dateTimeReturned;
        }

        private static int GetBankId(string value, Extract extract)
        {
            int bankId;
            if (!int.TryParse(value, out bankId))
            {
                extract.ImportingErrors.Add(string.Format("Bank id isn't numeric value: {0}", value));
                bankId = 0;
            }
            return bankId;
        }

        private static double GetTransactionValue(string value, Extract extract, ParserSettings settings)
        {
            double returnValue = 0;
            try
            {
                if (settings.CustomConverterCurrency != null)
				{
                    returnValue = settings.CustomConverterCurrency.Invoke(value);
                }
                else
				{
                    returnValue = Convert.ToDouble(value.Replace('.', ','));
                }
            }
            catch (Exception ex)
            {
                extract.ImportingErrors.Add(string.Format("Invalid transaction value/amount: {0}", value));
            }
            return returnValue;
        }
    }
}
