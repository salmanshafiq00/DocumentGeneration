using MigraDoc.DocumentObjectModel;
using MigraDoc.Rendering;
using System.Globalization;

namespace DocumentGeneration.Endpoints;

// Extension method to map the PDFSharp endpoint
public static class PdfSharpPdfGenerate
{
    public static IEndpointRouteBuilder MapPdfSharpPdfGenerate(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("pdfsharp");

        group.MapGet("get-invoice-pdf", (int? lineItemCount = 10) =>
        {
            // 1) Retrieve or build your invoice data
            var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

            // 2) Create MigraDoc document
            var document = new Document();
            document.Info.Title = "Invoice";

            // Default style
            var style = document.Styles["Normal"];
            style.Font.Name = "Arial";
            style.Font.Size = 12;

            // 3) Create one section, set margins
            var section = document.AddSection();
            section.PageSetup.PageFormat = PageFormat.A4;
            section.PageSetup.TopMargin = Unit.FromCentimeter(2);
            section.PageSetup.BottomMargin = Unit.FromCentimeter(2);
            section.PageSetup.LeftMargin = Unit.FromCentimeter(2);
            section.PageSetup.RightMargin = Unit.FromCentimeter(2);

            // 4) Add "INVOICE" heading at the top
            var titlePara = section.AddParagraph("INVOICE");
            titlePara.Format.Font.Size = 30;
            titlePara.Format.Font.Bold = true;
            titlePara.Format.Font.Color = Colors.Black;
            titlePara.Format.SpaceBefore = Unit.FromPoint(0);
            titlePara.Format.SpaceAfter = Unit.FromPoint(10);
            titlePara.Format.Alignment = ParagraphAlignment.Left;

            // 5) Add a 3-column table for company/client/invoice details
            var infoTable = section.AddTable();
            infoTable.Borders.Width = 0;  // no borders
            infoTable.AddColumn(Unit.FromCentimeter(6));
            infoTable.AddColumn(Unit.FromCentimeter(6));
            infoTable.AddColumn(Unit.FromCentimeter(6));

            var infoRow = infoTable.AddRow();

            // -- Left column: Company Info
            var companyPara = infoRow.Cells[0].AddParagraph();
            companyPara.AddFormattedText("Company Name", TextFormat.Bold);
            companyPara.Format.Font.Color = Colors.Black;
            companyPara.AddLineBreak();
            companyPara.AddText(invoiceData.CompanyAddress);
            companyPara.AddLineBreak();
            companyPara.AddText($"{invoiceData.CompanyCity}, {invoiceData.CompanyState}");
            companyPara.AddLineBreak();
            companyPara.AddText(invoiceData.CompanyCountry);
            companyPara.AddLineBreak();
            companyPara.AddText(invoiceData.CompanyPostal);

            // -- Middle column: "INVOICE TO"
            var clientPara = infoRow.Cells[1].AddParagraph();
            clientPara.AddFormattedText("INVOICE TO:", TextFormat.Bold);
            clientPara.Format.Font.Color = Colors.Black;
            clientPara.AddLineBreak();
            clientPara.AddText(invoiceData.ClientName);
            clientPara.AddLineBreak();
            clientPara.AddText(invoiceData.ClientAddress);
            clientPara.AddLineBreak();
            clientPara.AddText($"{invoiceData.ClientCity}, {invoiceData.ClientState}");
            clientPara.AddLineBreak();
            clientPara.AddText(invoiceData.ClientCountry);
            clientPara.AddLineBreak();
            clientPara.AddText(invoiceData.ClientPostal);

            // -- Right column: Invoice Details (right-aligned)
            var detailsPara = infoRow.Cells[2].AddParagraph();
            //detailsPara.Format.Alignment = ParagraphAlignment.Right;
            detailsPara.AddFormattedText("Invoice Number", TextFormat.Bold);
            detailsPara.Format.Font.Color = Colors.Black;
            detailsPara.AddLineBreak();
            detailsPara.AddText($"#{invoiceData.InvoiceNumber}");
            detailsPara.AddLineBreak();
            detailsPara.AddFormattedText("Date of Invoice", TextFormat.Bold);
            detailsPara.AddLineBreak();
            detailsPara.AddText(invoiceData.InvoiceDate);
            detailsPara.AddLineBreak();
            detailsPara.AddFormattedText("Due Date", TextFormat.Bold);
            detailsPara.AddLineBreak();
            detailsPara.AddText(invoiceData.DueDate);

            // Add a little vertical space
            section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(20);

            // 6) Create the line-items table
            var lineTable = section.AddTable();
            lineTable.Borders.Width = 0.5;
            lineTable.Format.Alignment = ParagraphAlignment.Center;

            // columns
            // Adjusted columns to include Serial No
            lineTable.AddColumn(Unit.FromCentimeter(1.5)); // SL
            lineTable.AddColumn(Unit.FromCentimeter(7.5)); // DESCRIPTION (Adjusted width)
            lineTable.AddColumn(Unit.FromCentimeter(2.5)); // QTY
            lineTable.AddColumn(Unit.FromCentimeter(2.5)); // UNIT PRICE
            lineTable.AddColumn(Unit.FromCentimeter(2.5)); // TOTAL

            // Header row
            var headerRow = lineTable.AddRow();
            headerRow.Shading.Color = new Color(43, 74, 88); // dark background

            var cSL = headerRow.Cells[0].AddParagraph("SL");
            cSL.Format.Font.Color = Colors.White;
            cSL.Format.Font.Bold = true;
            cSL.Format.Alignment = ParagraphAlignment.Center;

            var c0 = headerRow.Cells[1].AddParagraph("DESCRIPTION");
            c0.Format.Font.Color = Colors.White;
            c0.Format.Font.Bold = true;
            c0.Format.Alignment = ParagraphAlignment.Left;
            c0.Format.LeftIndent = Unit.FromPoint(5);

            var c1 = headerRow.Cells[2].AddParagraph("QTY");
            c1.Format.Font.Color = Colors.White;
            c1.Format.Font.Bold = true;
            c1.Format.Alignment = ParagraphAlignment.Center;

            var c2 = headerRow.Cells[3].AddParagraph("UNIT PRICE");
            c2.Format.Font.Color = Colors.White;
            c2.Format.Font.Bold = true;
            c2.Format.Alignment = ParagraphAlignment.Center;

            var c3 = headerRow.Cells[4].AddParagraph("TOTAL");
            c3.Format.Font.Color = Colors.White;
            c3.Format.Font.Bold = true;
            c3.Format.Alignment = ParagraphAlignment.Center;


            // Data rows
            for (int i = 0; i < invoiceData.LineItems.Count; i++)
            {
                var item = invoiceData.LineItems[i];
                var price = decimal.Parse(item.Price, CultureInfo.InvariantCulture);
                var total = price * item.Quantity;

                var row = lineTable.AddRow();
                row.Shading.Color = (i % 2 == 0)
                    ? new Color(230, 230, 230)
                    : new Color(240, 240, 240);

                // Serial No (SL)
                var slPara = row.Cells[0].AddParagraph((i + 1).ToString());
                slPara.Format.Alignment = ParagraphAlignment.Center;

                var descPara = row.Cells[1].AddParagraph(item.Name);
                descPara.Format.Alignment = ParagraphAlignment.Left;
                descPara.Format.LeftIndent = Unit.FromPoint(5);

                var qtyPara = row.Cells[2].AddParagraph(item.Quantity.ToString());
                qtyPara.Format.Alignment = ParagraphAlignment.Center;

                var pricePara = row.Cells[3].AddParagraph($"${price:F2}");
                pricePara.Format.Alignment = ParagraphAlignment.Right;

                var totalPara = row.Cells[4].AddParagraph($"${total:F2}");
                totalPara.Format.Alignment = ParagraphAlignment.Right;
            }

            // Some space before summary
            section.AddParagraph().Format.SpaceAfter = Unit.FromPoint(20);

            // 7) Summary table (right-aligned)
            var summaryTable = section.AddTable();
            summaryTable.Borders.Width = 0;
            summaryTable.AddColumn(Unit.FromCentimeter(10)); // alignment
            summaryTable.AddColumn(Unit.FromCentimeter(4));  // label
            summaryTable.AddColumn(Unit.FromCentimeter(3));  // value

            void AddSummaryRow(string label, string value, bool isBold = false)
            {
                var row = summaryTable.AddRow();
                row.Cells[0].AddParagraph(""); // empty alignment cell

                var labelPara = row.Cells[1].AddParagraph(label);
                labelPara.Format.Font.Bold = true;
                labelPara.Format.Alignment = ParagraphAlignment.Left;

                var valuePara = row.Cells[2].AddParagraph(value);
                valuePara.Format.Font.Bold = isBold;
                valuePara.Format.Alignment = ParagraphAlignment.Right;
            }

            AddSummaryRow("SUBTOTAL", $"${invoiceData.Subtotal:F2}");
            AddSummaryRow("DISCOUNT", $"${invoiceData.Discount:F2}");
            AddSummaryRow("SUBTOTAL LESS DISCOUNT", $"${invoiceData.SubtotalLessDiscount:F2}");
            AddSummaryRow("TAX RATE", $"{invoiceData.TaxRate * 100:F0}%");
            AddSummaryRow("TAX TOTAL", $"${invoiceData.TaxTotal:F2}");
            AddSummaryRow("BALANCE DUE", $"${invoiceData.BalanceDue:F2}", true);

            // 8) Finally, NOTES and TERMS at the bottom
            section.AddParagraph().Format.SpaceBefore = Unit.FromPoint(20);

            var notesTitle = section.AddParagraph("NOTES:");
            notesTitle.Format.Font.Bold = true;
            notesTitle.Format.SpaceAfter = Unit.FromPoint(5);

            var notesPara = section.AddParagraph(invoiceData.Notes);
            notesPara.Format.SpaceAfter = Unit.FromPoint(20);

            var termsTitle = section.AddParagraph("TERMS AND CONDITIONS:");
            termsTitle.Format.Font.Bold = true;
            termsTitle.Format.SpaceAfter = Unit.FromPoint(5);

            section.AddParagraph(invoiceData.Terms);

            // 9) Render to PDF
            var pdfRenderer = new PdfDocumentRenderer(unicode: true)
            {
                Document = document
            };
            pdfRenderer.RenderDocument();

            // 10) Return as File
            var pdfStream = new MemoryStream();
            pdfRenderer.PdfDocument.Save(pdfStream);
            pdfStream.Position = 0;

            return Results.File(pdfStream, "application/pdf", "invoice.pdf");
        })
        .WithName("pdfsharp-get-invoice-pdf")
        .WithOpenApi();



        return routes;
    }
}
