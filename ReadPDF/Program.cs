using HtmlAgilityPack;
using iTextSharp.text;
using iTextSharp.text.exceptions;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;
using Syncfusion.Pdf.Parsing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Document = iTextSharp.text.Document;

namespace ReadPDF
{

    public class Program
    {
        /*
        PDF1: 
        Mensagem erro: "Rebuild failed: Dictionary key endstream is not a name. at file pointer 506343"
        Solução aplicada na OPÇÃO 2 e 3 resolvem o esse incidente
        */
        public static string pdf1 { get; } = @"C:\ME\Anexos\MALDICAO_DO_PDF_1.pdf";

        /*
        PDF2: 
        Mensagem erro: "Rebuild failed: Dictionary key R is not a name. at file pointer 4169"
        Ainda sem solução
        */
        public static string pdf2 { get; } = @"C:\ME\Anexos\MALDICAO_DO_PDF_2.pdf";

        /*
        PDF3: 
        Quando reparado o PDF via site "https://www.ilovepdf.com/pt/reparar-pdf", funciona normalmente        
        */
        public static string pdf3 { get; } = @"C:\ME\Anexos\MALDICAO_DO_PDF_2_repaired.pdf";

        /*
        PDF4: 
        Quando reparado o PDF via nugget "Syncfusion.Pdf.Net.Core", não funcionou
        Feito um teste no metodo repairPDF() e salvando o PDF na area de trabalho para depois utiliza-lo aqui pra ver se iria funcionar
        */
        public static string pdf4 { get; } = @"C:\ME\Anexos\MALDICAO_DO_PDF_2_repaired_BY_Syncfusion_Pdf_Net_Core.pdf"; //reparado via nugget "Syncfusion.Pdf.Net.Core"

        static void Main(string[] args)
        {
            Byte[] bytes = File.ReadAllBytes(pdf1);
            String file = Convert.ToBase64String(bytes);

            var fileB64 = Convert.FromBase64String(file);

            Byte[] bytes1 = File.ReadAllBytes(pdf2);
            String file2 = Convert.ToBase64String(bytes1);

            var fileB641 = Convert.FromBase64String(file2);

            repairPDF();
            ConvertHTMLtoBase64();
        }

        private static void repairPDF()
        {
            using (FileStream pdfStream = new FileStream(pdf2, FileMode.Open, FileAccess.Read))
            {
                //load the corrupted document by setting the openAndRepair flag to true to repair the document.
                PdfLoadedDocument loadedPdfDocument = new PdfLoadedDocument(pdfStream, true);

                //Do PDF processing.

                //Save the document.
                using (FileStream outputStream = new FileStream(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MALDICAO_DO_PDF_2_repaired_BY_Syncfusion_Pdf_Net_Core.pdf"), FileMode.Create))
                {
                    loadedPdfDocument.Save(outputStream);
                }
                //Close the document.
                loadedPdfDocument.Close(true);
            }
        }

        private static Byte[] ConvertHTMLtoBase64()
        {
            Byte[] bytes;

            using (var ms = new MemoryStream())
            {
                using (var doc = new Document(PageSize.A4.Rotate()))
                {
                    doc.SetMargins(20, 20, 10, 10);
                    doc.SetMarginMirroringTopBottom(false);

                    using (var writer = PdfWriter.GetInstance(doc, ms))
                    {
                        doc.Open();

                        Invoices invoice = new Invoices();

                        var html = GetHtmPDF(invoice);


                        //var newHtml = System.Text.RegularExpressions.Regex.Replace(html, @"[^0-9a-zA-ZéúíóáÉÚÍÓÁèùìòàÈÙÌÒÀõãñÕÃÑêûîôâÊÛÎÔÂëÿüïöäËYÜÏÖÄçÇ\s]+?", string.Empty);
                        ////var valid = ContainsHTMLElements(html);

                        //string pattern = "(>>\\.? |Mrs\\.? |Miss |Ms\\.? )";


                        //var regex = new Regex("<(\"[^\"]*\"|'[^']*'|[^'\">])*>");
                        //var isValid = regex.IsMatch(html.Normalize());

                        //var existeCaracterEspecial = Regex.IsMatch(html, (@"[!""#$%&'()*+,-./:;?@[\\\]_`{|}~]"));
                        //var matches = regex.Matches(html);
                        //var tags = matches.OfType<Match>().Select(m => m.Groups[2].Value);
                        //tags.Dump();

                        var example_html = @"<p>This is <span class=""headline"" style=""text-decoration: underline;"">some</span> <strong>sample text</strong><span style=""color: red;"">!!!</span></p>";

                        using (var htmlWorker = new iTextSharp.text.html.simpleparser.HTMLWorker(doc))
                        {
                            using (var htmlMemoryStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(example_html)))
                            {
                                XMLWorkerHelper.GetInstance().ParseXHtml(writer, doc, htmlMemoryStream, Encoding.UTF8);
                            }
                        }
                        doc.Close();
                    }
                }

                bytes = ms.ToArray();
            }


            var fullFileName = pdf2;
            //var shortFileName = @"MALDICAO_DO_PDF_2.pdf";
            //var stream1 = GetFileStream(fullFileName, shortFileName);
            var stream1 = GetFileStream(fullFileName);
            var base64 = convertToBase64(stream1);
            var arrayBytes = Convert.FromBase64String(base64);

            var bytes1 = new List<byte[]>();

            bytes1.Add(arrayBytes);
            bytes1.Add(bytes);


            var mergedBytes = MergePDFs(bytes1);

            var testFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "test.pdf");
            System.IO.File.WriteAllBytes(testFile, mergedBytes);

            return bytes;
        }


        public static byte[] MergePDFs(List<byte[]> pdfFiles)
        {
            try
            {
                if (pdfFiles.Count > 1)
                {
                    PdfReader finalPdf;
                    PdfReader.unethicalreading = true;


                    Document pdfContainer;
                    MemoryStream msFinalPdf = new MemoryStream();
                    pdfContainer = new Document();
                    var pdfCopy = new PdfSmartCopy(pdfContainer, msFinalPdf);
                    pdfContainer.Open();
                    for (int k = 0; k < pdfFiles.Count; k++)
                    {
                        //Opção 1
                        //finalPdf = new PdfReader(pdfFiles[k]);

                        //Opção 2
                        var raf = new RandomAccessFileOrArray(pdfFiles[k]);
                        finalPdf = new PdfReader(raf: raf, ownerPassword: null);

                        //Opção 3
                        //var properties = new ReaderProperties();
                        //properties.SetPartialRead(true);
                        //
                        //Stream stream = new MemoryStream(pdfFiles[k]);
                        //finalPdf = new PdfReader(properties: properties, isp: stream);



                        for (int i = 1; i < finalPdf.NumberOfPages + 1; i++)
                        {

                            //finalPdf.ReleasePage(i);
                            //finalPdf.RemoveAnnotations();
                            //finalPdf.RemoveUsageRights();
                            //////finalPdf.RemoveUnusedObjects();
                            ////finalPdf.ResetLastXrefPartial();
                            ////finalPdf.ResetReleasePage();
                            //finalPdf.IsRebuilt();
                            ////finalPdf.EliminateSharedStreams();


                            var impPage = pdfCopy.GetImportedPage(finalPdf, i);
                            //impPage.SanityCheck();

                            pdfCopy.AddPage(impPage);
                        }
                        pdfCopy.FreeReader(finalPdf);
                        finalPdf.Close();

                    }
                    pdfCopy.Close();
                    pdfContainer.Close();
                    return msFinalPdf.ToArray();
                }
                else if (pdfFiles.Count == 1)
                {
                    return pdfFiles[0];
                }
                return null;
            }
            catch (InvalidPdfException ex)
            {
                throw new Exception($"PDF Inválido. {ex.Message} - {ex.GetType()}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na tentativa de merge dos PDFs. {ex.Message} - {ex.GetType()}", ex);
            }
        }



        public static Stream GetFileStream(string pFullPath, string pFileName = default)
        {
            pFileName = pFileName?.Replace(";", "_");

            FileInfo anexo = new FileInfo(pFullPath);
            if (!anexo.Exists)
                throw new Exception("O arquivo não foi encontrado");

            FileStream fileStream = File.Open(pFullPath, FileMode.Open);

            return fileStream;
        }

        public static string convertToBase64(Stream stream)
        {
            using (MemoryStream mem = new MemoryStream())
            {
                stream.CopyTo(mem);
                byte[] bytes = mem.ToArray();
                return Convert.ToBase64String(bytes);
            }
        }

        private static bool HtmlIsJustText(HtmlNode rootNode)
        {
            return rootNode.Descendants().All(n => n.NodeType == HtmlNodeType.Text);
        }

        public static bool ContainsHTMLElements(string text)
        {
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(text);
            return !HtmlIsJustText(doc.DocumentNode);
        }

        //bool isValidHTMLTag(string str)
        //{

        //    // Regex to check valid HTML tag.
        //    const regex pattern("<(\"[^\"]*\"|'[^']*'|[^'\">])*>");

        //    if (string.IsNullOrEmpty(str))
        //    {
        //        return false;
        //    }

        //    // Return true if the HTML tag
        //    // matched the ReGex
        //    if (regex_match(str, pattern))
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        private static string GetHtmPDF(Invoices invoice)
        {
            try
            {
                string html = "";
                //consumir método AdapterRecipient NotaFicalService
                html = $"<!DOCTYPE html> " +
                "<html> " +
                "<head><title>-</title> " +
                "</head> " +
                "<body> " +
                    "<div style='width: 100%; font: 12px Arial;'> " +
                        "<header style='margin-bottom: 20px; display: inline-block; ' width: 100%;'>" +
                        "<div>" +
                            "<div style='font-size: 10px; font-weight: 600; text-align: center;margin-bottom: 20px;'>" +
                                $"<span style='font-weight: 600; font-size: 10px;text-align: center;'>Mirrored Internal Copy of Invoice generated by Mercadoe. </span> " +
                            " </div>" +
                            "<div style='float: right;'> " +
                                "<div style='font-size: 16px; font-weight: 600; text-align: right;'>" +
                                  $"USD <(\"[^\"]*\"|'[^']*'|[^'\">])*>  <span style='font-weight: 600; font-size: 20px;'> {string.Format("{0:N}", invoice.TOTALDUE.GetValueOrDefault())}  </span> " +
                                " </div>" +
                                "<div style='font-size: 16px; font-weight: 600; text-align: right; color: #888'> " +
                                    "Created" + // nok "Acknowledged"
                                " </div> " +
                                "<div style='font-size: 16px; text-align: right;'> " +
                                    $"Issue on {invoice.ISSUEDATE.GetValueOrDefault().ToString("MM/dd/yyyy")}" +
                                " </div> " +
                            " </div> " +
                            "<div " +
                                "style='margin-bottom: 20px; min-width: 130px; height: 60px; font-size: 14px; font-weight: 600; border-right: 2px solid #ccc; margin-right: 20px; float: left; width: 200px;'> " +
                                "Invoice" +
                                $"<div style='font-size: 16px;'> {invoice.INVOICENUMBER}  </div> " +
                            " </div> " +
                            "<div style='margin-bottom: 20px; font-size: 18px; float: left; font-weight: 600; margin-right: 20px; font: 12px Arial;'> " +
                                "<h3 style='font-weight: normal; color: #888; margin: 0; font-size: 12px;'>Vendor</h3> " +
                                "<div style='font-size: 18px; font-weight: 600'> " +
                                " </div> " +
                                "<div style='margin-bottom: 10px'; color:#888;> " +
                                    "<span> " +
                                    " </span> " +
                          //Concatenar os dados para apresentação neste campo  conforme prototipo<COMPANYNAME Tipo= "String" > ALPINE GAS INC</ COMPANYNAME >
                          //VENDOR  Concatenar os dados para apresentação neste campo conforme prototipo < IE Tipo = "String" ></ IE >
                          " </div> " +
                                "<div style='margin-bottom: 10px'; color:#888;> " +
                                    "<span> " +
                                    " </span> | " +
                                    "<span> " +
                                    " </span> " +
                                    "<span> " +
                                    " </span> " +
                                " </div> " +
                                "<div> " +
                                " </div> " +
                            " </div> " +
                          " </div>" +
                        "</header> " +
                        "<section style='width: 100%; display: inline-block; margin-bottom: 20px;'> " +
                            "<h3 style='font-weight: normal; color: #888; margin-top: 20px; font-size: 12px;'>Recipient</h3> " +
                            "<div style='width: 100%;'> " +
                                "<h4> " +
                                "</h4>" +
                            " </div>" +
                            "<div style='display: flex;'>" +
                                "<div style='float: left; width: 25%; margin-bottom: 20px;'>" +
                                    "<div style='margin-bottom: 16px;'>" +
                                " </div>" +
                                " </div>" +
                            //"<div style='float: left; width: 25%; margin-bottom: 20px;'>" +
                            //    "<div style='margin-bottom: 16px; color: #888;'>Contact </div>" +
                            //    $"-" +
                            //" </div>" +
                            //"<div style='float: left; width: 33.33%;'>" +
                            //    "<div style='margin-bottom: 16px;'>" +
                            //        "Read: - " + //nok
                            //    " </div>" +
                            //" </div>" +
                            " </div>" +
                            "<div style='display: flex;'>" +
                            //"<div style='float: left; width: 25%; margin-bottom: 20px;'>" +
                            //     "<div style='margin-bottom: 16px;'>" +
                            //         " - " + // endereço
                            //     " </div>" +
                            // " </div>" +
                            //"<div style='float: left; width: 25%; margin-bottom: 20px;'>" +
                            //    "<div style='margin-bottom: 16px; color: #888;'>Phone: </div>" +
                            //        $"000-000-0000 - Fax: 000-000-0000" + // nok
                            //" </div>" +
                            //"<div style='float: left; width: 33.33%;'>" +
                            //    "<div style='margin-bottom: 16px;'>" +
                            //        "Last Reply: -" + // nok
                            //    " </div>" +
                            //" </div>" +
                            " </div>" +
                        "</section>" +
                        "<section style='width: 100%; margin-bottom: 20px; font-size: 18px; color: #000;'>" +
                            "<h3 style='margin-bottom: 20px; font-weight: 600; font-size: 18px; color: #000;'>" +
                                "Other Important Information" +
                            "</h3>" +
                            "<div style='font: 12px Arial;'>" +
                            "<div style='float: left; width: 33.33%; margin-bottom: 20px;'>" +
                                    "<div style='margin-bottom: 16px; color: #888;'>Discounts </div>" +
                                   $"USD {string.Format("{0:N}", invoice.DISCOUNTS.GetValueOrDefault())}" +
                                " </div>" +
                                "<div style='float: left; width: 33.33%; margin-bottom: 20px;'>" +
                                    "<div style='margin-bottom: 16px; color: #888;'>Shipping/ Handling Charges </div>" +
                                    $"USD {string.Format("{0:N}", invoice.SHIPPINGPRICE.GetValueOrDefault())}" +
                                " </div>" +
                                "<div style='float: left; width: 50%; margin-bottom: 20px;'>" +
                                    "<div style='margin-bottom: 16px; color: #888;'>Tax Amount </div>" +
                                     $"USD {string.Format("{0:N}", invoice.TAXES.GetValueOrDefault())}" +
                                " </div>" +
                                "<div style='float: left; margin-bottom: 20px;'>" +
                                    "<div style='margin-bottom: 16px; color: #888;'>Payment Terms </div>" +
                                    $"{invoice.PAYMENTTERMS}" +
                                " </div>" +
                            //"<div style='float: left; width: 25%; margin-bottom: 25px;'>" +
                            //    "<div style='margin-bottom: 16px; color: #888;'>Vendor Comments </div>" +
                            ////$"{invoice.MsgVendor}" +
                            //" </div>" +
                            " </div>" +
                        //"<div style='display: flex; font: 12px Arial;'>" +
                        //"<div style='float: left; width: 25%; margin-bottom: 20px;'>" +
                        //    "<div style='margin-bottom: 16px; color: #888;'>Expected Payment Date </div>" +
                        //$"-" + // nok
                        //" </div>" +
                        //"<div style='float: left; width: 25%; margin-bottom: 20px;'>" +
                        //    "<div style='margin-bottom: 16px; color: #888;'>Paid in </div>" +
                        //"-" +// nok
                        //" </div>" +
                        //"<div style='float: left; width: 25%; margin-bottom: 20px;'>" +
                        //    "<div style='margin-bottom: 16px; color: #888;'>Departure date </div>" +
                        //"-" +//nok
                        //" </div>" +
                        //" </div>" +
                        "</section>" +
                        "<section style='margin-bottom: 20px;'>" +
                            "<h3 style='margin-top: 20px; margin-bottom: 20px; font-weight: 600; font-size: 18px; color: #000;'>Items" +
                                "</h3>" +
                            "<table style=' width: 100%; border: none; border-collapse: collapse;'>" +
                                "<thead>" +
                                    "<tr>" +
                                        "<th style='border-bottom: 1px solid #ddd; padding: 20px 0; text-align: left;'>PO# - Line</th>" +
                                        "<th style='border-bottom: 1px solid #ddd; padding: 20px 0; text-align: left;'>Material</th>" +
                                        "<th style='border-bottom: 1px solid #ddd; padding: 20px 0; text-align: left;'>Description</th>" +
                                        "<th style='border-bottom: 1px solid #ddd; padding: 20px 0; text-align: left;'>VPN</th>" +
                                        "<th style='border-bottom: 1px solid #ddd; padding: 20px 0; text-align: left;'>QTY</th>" +
                                        "<th style='border-bottom: 1px solid #ddd; padding: 20px 0; text-align: left;'>Unit.</th>" +
                                        "<th style='border-bottom: 1px solid #ddd; padding: 20px 0; text-align: left;'>Unit. Value</th>" +
                                        "<th style='border-bottom: 1px solid #ddd; padding: 20px 0; text-align: left;'>Taxes</th>" +
                                        "<th style='border-bottom: 1px solid #ddd; padding: 20px 0; text-align: left;'>Total</th>" +
                                    "</tr>" +
                                "</thead>" +
                            "</table>" +
                        "</section>" +
                        "<table style='width: 100%;'>" +
                            "<tr>" +
                              "<td style='width: 60%;'></td>" +
                               "<td style='width: 40%;'>" +
                                "<table style='width: 230px; float: right;'>" +
                                  "<tr>" +
                                    "<td style='text-align: right; padding: 8px'>Subtotal</td>" +
                                     $"<td style='text-align: right; font-weight: bold; font-size: 14px;'>USD </td>" +
                                  "</tr>" +
                                  "<tr>" +
                                    "<td style='text-align: right; padding: 8px'>Discounts</td>" +
                                      $"<td style='text-align: right; font-weight: bold; font-size: 14px;'>USD </td>" +
                                  "</tr>" +
                                  "<tr>" +
                                    "<td style='text-align: right; padding: 8px'>Total</td>" +
                                      $"<td style='text-align: right; font-weight: bold; font-size: 14px;'>USD </td>" +
                                  "</tr>" +
                                  "<tr>" +
                                    "<td style='text-align: right; padding: 8px'>Total Taxes</td>" +
                                     $"<td style='text-align: right; font-weight: bold; font-size: 14px;'>USD </td>" +
                                  "</tr>" +
                                  "<tr>" +
                                    "<td style='text-align: right; padding: 8px'>Additional Charges</td>" +
                                    $"<td style='text-align: right; font-weight: bold; font-size: 14px;'>USD</td>" +
                                  "</tr>" +
                                  "<tr>" +
                                    "<td style='text-align: right; padding: 8px'>Amount Due</td>" +
                                    $"<td style='text-align: right; font-weight: bold; font-size: 16px;'>USD</td>" +
                                  "</tr>" +
                              "</table>" +
                              "</td>" +
                            "</tr>" +
                          "</table>" +
                    " </div>" +
                "</body>" +
                "</html>";

                return html;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na conversão do HTML. {ex.Message}");
            }
        }

        public class Invoices
        {
            public string SOURCE { get; set; }
            public string EINVOICE { get; set; }
            public DateTime? CREATIONDATE { get; set; }
            public string BUYERPHONE { get; set; }
            public string BUYER { get; set; }
            public string ERPVENDORCODE { get; set; }
            public int? ITEMQTY { get; set; }
            public string PAYMENTTERMS { get; set; }
            public string USER_LOGIN { get; set; }
            public int? USER { get; set; }
            public string VENDORTAXID { get; set; }
            public string BUYERTAXID { get; set; }
            public double? DISCOUNTS { get; set; }
            public double? TAXES { get; set; }
            public double? SHIPPINGPRICE { get; set; }
            public DateTime? HOURDATE { get; set; }
            public double? NETPRICE { get; set; }
            public double? TOTALDUEWOTAX { get; set; }
            public double? TOTALDUE { get; set; }
            public DateTime? ISSUEDATE { get; set; }
            public string INVOICENUMBER { get; set; }
            public int? MEVENDORCODE { get; set; }
            public string PONUMBER { get; set; }
            public string MEPONUMBER { get; set; }
            public int? MEINVOICEID { get; set; }
            public string ORIGINALINVOICENUMBER { get; set; }
            public DateTime? ORIGINALINVOICEDATE { get; set; }
        }
    }

    public class PDFParser
    {
        /// BT = Beginning of a text object operator 
        /// ET = End of a text object operator
        /// Td move to the start of next line
        ///  5 Ts = superscript
        /// -5 Ts = subscript

        /// <summary>
        /// The number of characters to keep, when extracting text.
        /// </summary>
        private static int _numberOfCharsToKeep = 15;

        /// <summary>
        /// Extracts a text from a PDF file.
        /// </summary>
        /// <param name="inFileName">the full path to the pdf file.</param>
        /// <param name="outFileName">the output file name.</param>
        /// <returns>the extracted text</returns>
        public PdfReader ExtractText(PdfReader reader, string outFileName)
        {
            StreamWriter outFile = null;
            try
            {
                // Create a reader for the given PDF file
                //PdfReader reader = new PdfReader(inFileName);
                //outFile = File.CreateText(outFileName);
                outFile = new StreamWriter(outFileName, false, System.Text.Encoding.UTF8);

                Console.Write("Processing: ");

                int totalLen = 68;
                float charUnit = ((float)totalLen) / (float)reader.NumberOfPages;
                int totalWritten = 0;
                float curUnit = 0;

                for (int page = 1; page <= reader.NumberOfPages; page++)
                {
                    outFile.Write(ExtractTextFromPDFBytes(reader.GetPageContent(page)) + " ");

                    // Write the progress.
                    if (charUnit >= 1.0f)
                    {
                        for (int i = 0; i < (int)charUnit; i++)
                        {
                            Console.Write("#");
                            totalWritten++;
                        }
                    }
                    else
                    {
                        curUnit += charUnit;
                        if (curUnit >= 1.0f)
                        {
                            for (int i = 0; i < (int)curUnit; i++)
                            {
                                Console.Write("#");
                                totalWritten++;
                            }
                            curUnit = 0;
                        }

                    }
                }

                if (totalWritten < totalLen)
                {
                    for (int i = 0; i < (totalLen - totalWritten); i++)
                    {
                        Console.Write("#");
                    }
                }
                return reader;
            }
            catch (Exception ex)
            {
                return reader;
            }
            finally
            {
                if (outFile != null) outFile.Close();
            }
        }

        /// <summary>
        /// This method processes an uncompressed Adobe (text) object 
        /// and extracts text.
        /// </summary>
        /// <param name="input">uncompressed</param>
        /// <returns></returns>
        public string ExtractTextFromPDFBytes(byte[] input)
        {
            if (input == null || input.Length == 0) return "";

            try
            {
                string resultString = "";

                // Flag showing if we are we currently inside a text object
                bool inTextObject = false;

                // Flag showing if the next character is literal 
                // e.g. '\\' to get a '\' character or '\(' to get '('
                bool nextLiteral = false;

                // () Bracket nesting level. Text appears inside ()
                int bracketDepth = 0;

                // Keep previous chars to get extract numbers etc.:
                char[] previousCharacters = new char[_numberOfCharsToKeep];
                for (int j = 0; j < _numberOfCharsToKeep; j++) previousCharacters[j] = ' ';


                for (int i = 0; i < input.Length; i++)
                {
                    char c = (char)input[i];
                    if (input[i] == 213)
                        c = "'".ToCharArray()[0];

                    if (inTextObject)
                    {
                        // Position the text
                        if (bracketDepth == 0)
                        {
                            if (CheckToken(new string[] { "TD", "Td" }, previousCharacters))
                            {
                                resultString += "\n\r";
                            }
                            else
                            {
                                if (CheckToken(new string[] { "'", "T*", "\"" }, previousCharacters))
                                {
                                    resultString += "\n";
                                }
                                else
                                {
                                    if (CheckToken(new string[] { "Tj" }, previousCharacters))
                                    {
                                        resultString += " ";
                                    }
                                }
                            }
                        }

                        // End of a text object, also go to a new line.
                        if (bracketDepth == 0 &&
                            CheckToken(new string[] { "ET" }, previousCharacters))
                        {

                            inTextObject = false;
                            resultString += " ";
                        }
                        else
                        {
                            // Start outputting text
                            if ((c == '(') && (bracketDepth == 0) && (!nextLiteral))
                            {
                                bracketDepth = 1;
                            }
                            else
                            {
                                // Stop outputting text
                                if ((c == ')') && (bracketDepth == 1) && (!nextLiteral))
                                {
                                    bracketDepth = 0;
                                }
                                else
                                {
                                    // Just a normal text character:
                                    if (bracketDepth == 1)
                                    {
                                        // Only print out next character no matter what. 
                                        // Do not interpret.
                                        if (c == '\\' && !nextLiteral)
                                        {
                                            resultString += c.ToString();
                                            nextLiteral = true;
                                        }
                                        else
                                        {
                                            if (((c >= ' ') && (c <= '~')) ||
                                                ((c >= 128) && (c < 255)))
                                            {
                                                resultString += c.ToString();
                                            }

                                            nextLiteral = false;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Store the recent characters for 
                    // when we have to go back for a checking
                    for (int j = 0; j < _numberOfCharsToKeep - 1; j++)
                    {
                        previousCharacters[j] = previousCharacters[j + 1];
                    }
                    previousCharacters[_numberOfCharsToKeep - 1] = c;

                    // Start of a text object
                    if (!inTextObject && CheckToken(new string[] { "BT" }, previousCharacters))
                    {
                        inTextObject = true;
                    }
                }

                return CleanupContent(resultString);
            }
            catch
            {
                return "";
            }
        }

        private string CleanupContent(string text)
        {
            string[] patterns = { @"\\\(", @"\\\)", @"\\226", @"\\222", @"\\223", @"\\224", @"\\340", @"\\342", @"\\344", @"\\300", @"\\302", @"\\304", @"\\351", @"\\350", @"\\352", @"\\353", @"\\311", @"\\310", @"\\312", @"\\313", @"\\362", @"\\364", @"\\366", @"\\322", @"\\324", @"\\326", @"\\354", @"\\356", @"\\357", @"\\314", @"\\316", @"\\317", @"\\347", @"\\307", @"\\371", @"\\373", @"\\374", @"\\331", @"\\333", @"\\334", @"\\256", @"\\231", @"\\253", @"\\273", @"\\251", @"\\221" };
            string[] replace = { "(", ")", "-", "'", "\"", "\"", "à", "â", "ä", "À", "Â", "Ä", "é", "è", "ê", "ë", "É", "È", "Ê", "Ë", "ò", "ô", "ö", "Ò", "Ô", "Ö", "ì", "î", "ï", "Ì", "Î", "Ï", "ç", "Ç", "ù", "û", "ü", "Ù", "Û", "Ü", "®", "™", "«", "»", "©", "'" };

            for (int i = 0; i < patterns.Length; i++)
            {
                string regExPattern = patterns[i];
                Regex regex = new Regex(regExPattern, RegexOptions.IgnoreCase);
                text = regex.Replace(text, replace[i]);
            }

            return text;
        }

        /// <summary>
        /// Check if a certain 2 character token just came along (e.g. BT)
        /// </summary>
        /// <param name="tokens">the searched token</param>
        /// <param name="recent">the recent character array</param>
        /// <returns></returns>
        private bool CheckToken(string[] tokens, char[] recent)
        {
            foreach (string token in tokens)
            {
                if ((recent[_numberOfCharsToKeep - 3] == token[0]) &&
                    (recent[_numberOfCharsToKeep - 2] == token[1]) &&
                    ((recent[_numberOfCharsToKeep - 1] == ' ') ||
                    (recent[_numberOfCharsToKeep - 1] == 0x0d) ||
                    (recent[_numberOfCharsToKeep - 1] == 0x0a)) &&
                    ((recent[_numberOfCharsToKeep - 4] == ' ') ||
                    (recent[_numberOfCharsToKeep - 4] == 0x0d) ||
                    (recent[_numberOfCharsToKeep - 4] == 0x0a))
                    )
                {
                    return true;
                }
            }
            return false;
        }
    }
}
