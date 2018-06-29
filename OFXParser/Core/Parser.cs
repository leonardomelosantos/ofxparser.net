using System;
using System.Text;
using System.IO;
using System.Xml;
using OFXParser.Core;
using OFXParser.Entities;

namespace OFXParser
{
    public class Parser
    {
        /// <summary>
        /// This method translate an OFX file to XML tags, independent of the content.
        /// </summary>
        /// <param name="ofxSourceFile">OFX source file</param>
        /// <returns>XML tags in StringBuilder object.</returns>
        private static StringBuilder TranslateToXml(String ofxSourceFile)
        {
            StringBuilder resultado = new StringBuilder();
            int nivel = 0;
            String linha;

            if (!File.Exists(ofxSourceFile))
            {
                throw new FileNotFoundException("OFX source file not found: " + ofxSourceFile);
            }

            StreamReader sr = File.OpenText(ofxSourceFile);
            while ((linha = sr.ReadLine()) != null)
            {
                linha = linha.Trim();

                if (linha.StartsWith("</") && linha.EndsWith(">"))
                {
                    AddTabs(resultado, nivel, true);
                    nivel--;
                    resultado.Append(linha);
                }
                else if (linha.StartsWith("<") && linha.EndsWith(">"))
                {
                    nivel++;
                    AddTabs(resultado, nivel, true);
                    resultado.Append(linha);
                }
                else if (linha.StartsWith("<") && !linha.EndsWith(">"))
                {
                    AddTabs(resultado, nivel + 1, true);
                    resultado.Append(linha);
                    resultado.Append(ReturnFinalTag(linha));
                }
            }
            sr.Close();

            return resultado;
        }

        public static Extract GetExtract(String ofxSourceFile, ParserSettings settings)
        {
            if (settings == null) settings = new ParserSettings();

            Boolean temCabecalho = false;
            Boolean temDadosConta = false;
            
            // Translating to XML file
            ExportToXml(ofxSourceFile, ofxSourceFile + ".xml");

            // Vari�veis �teis para o Parse
            String elementoSendoLido = "";
            Transaction transacaoAtual = null;

            // Vari�vies utilizadas para a leitura do XML
            HeaderExtract cabecalho = new HeaderExtract();
            BankAccount conta = new BankAccount();
            Extract extrato = new Extract(cabecalho, conta, "");

            // Lendo o XML efetivamente
            XmlTextReader meuXml = new XmlTextReader(ofxSourceFile + ".xml");
            try
            {
                while (meuXml.Read())
                {
                    if (meuXml.NodeType == XmlNodeType.EndElement)
                    {
                        switch (meuXml.Name)
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
                    if (meuXml.NodeType == XmlNodeType.Element)
                    {
                        elementoSendoLido = meuXml.Name;

                        switch (elementoSendoLido)
                        {
                            case "STMTTRN":
                                transacaoAtual = new Transaction();
                                break;
                        }
                    }
                    if (meuXml.NodeType == XmlNodeType.Text)
                    {
                        switch (elementoSendoLido)
                        {
                            case "DTSERVER":
                                cabecalho.ServerDate = ConvertOfxDateToDateTime(meuXml.Value, extrato);
                                temCabecalho = true;
                                break;
                            case "LANGUAGE":
                                cabecalho.Language = meuXml.Value;
                                temCabecalho = true;
                                break;
                            case "ORG":
                                cabecalho.BankName = meuXml.Value;
                                temCabecalho = true;
                                break;
                            case "DTSTART":
                                extrato.InitialDate = ConvertOfxDateToDateTime(meuXml.Value, extrato);
                                break;
                            case "DTEND":
                                extrato.FinalDate = ConvertOfxDateToDateTime(meuXml.Value, extrato);
                                break;
                            case "BANKID":
                                conta.Bank = new Bank(GetBankId(meuXml.Value, extrato), "");
                                temDadosConta = true;
                                break;
                            case "BRANCHID":
                                conta.AgencyCode = meuXml.Value;
                                temDadosConta = true;
                                break;
                            case "ACCTID":
                                conta.AccountCode = meuXml.Value;
                                temDadosConta = true;
                                break;
                            case "ACCTTYPE":
                                conta.Type = meuXml.Value;
                                temDadosConta = true;
                                break;
                            case "TRNTYPE":
                                transacaoAtual.Type = meuXml.Value;
                                break;
                            case "DTPOSTED":
                                transacaoAtual.Date = ConvertOfxDateToDateTime(meuXml.Value, extrato);
                                break;
                            case "TRNAMT":
                                transacaoAtual.TransactionValue = GetTransactionValue(meuXml.Value, extrato);
                                break;
                            case "FITID":
                                transacaoAtual.Id = meuXml.Value;
                                break;
                            case "CHECKNUM":
                                transacaoAtual.Checksum = Convert.ToInt64(meuXml.Value);
                                break;
                            case "MEMO":
                                transacaoAtual.Description = string.IsNullOrEmpty(meuXml.Value) ? "" : meuXml.Value.Trim().Replace("  ", " ");
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
                meuXml.Close();
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

                    if (File.Exists(xmlNewFile))
                        File.Delete(xmlNewFile);

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
            String returnFinal = "";

            if ((content.IndexOf("<") != -1) && (content.IndexOf(">") != -1))
            {
                int position1 = content.IndexOf("<");
                int position2 = content.IndexOf(">");
                if ((position2 - position1) > 2)
                {
                    returnFinal = content.Substring(position1, (position2 - position1) + 1);
                    returnFinal = returnFinal.Replace("<", "</");
                }
            }

            return returnFinal;
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
        /// <param name="PartDateTimeEnum">Part of date</param>
        /// <returns></returns>
        private static int GetPartOfOfxDate(String ofxDate, PartDateTimeEnum PartDateTimeEnum)
        {
            int result = 0;

            if (PartDateTimeEnum == PartDateTimeEnum.YEAR){
                result = Int32.Parse(ofxDate.Substring(0,4));

            } else if (PartDateTimeEnum == PartDateTimeEnum.MONTH) {
                result = Int32.Parse(ofxDate.Substring(4, 2));

            } if (PartDateTimeEnum == PartDateTimeEnum.DAY) {
                result = Int32.Parse(ofxDate.Substring(6, 2));

            } if (PartDateTimeEnum == PartDateTimeEnum.HOUR) {
                if (ofxDate.Length >= 10)
                {
                    result = Int32.Parse(ofxDate.Substring(8, 2));
                }
                else
                {
                    result = 0;
                }

            } if (PartDateTimeEnum == PartDateTimeEnum.MINUTE) {
                if (ofxDate.Length >= 12)
                {
                    result = Int32.Parse(ofxDate.Substring(10, 2));
                }
                else
                {
                    result = 0;
                }

            } if (PartDateTimeEnum == PartDateTimeEnum.SECOND) {
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
                int year = GetPartOfOfxDate(ofxDate, PartDateTimeEnum.YEAR);
                int month = GetPartOfOfxDate(ofxDate, PartDateTimeEnum.MONTH);
                int day = GetPartOfOfxDate(ofxDate, PartDateTimeEnum.DAY);
                int hour = GetPartOfOfxDate(ofxDate, PartDateTimeEnum.HOUR);
                int minute = GetPartOfOfxDate(ofxDate, PartDateTimeEnum.MINUTE);
                int second = GetPartOfOfxDate(ofxDate, PartDateTimeEnum.SECOND);

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

        private static double GetTransactionValue(string value, Extract extract)
        {
            double returnValue = 0;
            try
            {
                returnValue = Convert.ToDouble(value.Replace('.', ','));
            }
            catch (Exception ex)
            {
                extract.ImportingErrors.Add(string.Format("Invalid transaction value: {0}", value));
            }
            return returnValue;
        }
    }
}
