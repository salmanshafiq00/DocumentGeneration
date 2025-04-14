using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Globalization;
using ZXing;
using ZXing.OneD;
using ZXing.QrCode;
using ZXing.Rendering;

namespace DocumentGeneration.Endpoints;

public static class QuestPdfGenerate
{
    public static IEndpointRouteBuilder MapQuestPdfGenerate(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("questPdf");

        group.MapGet("get-invoice-pdf", (int? lineItemCount = 10) =>
        {
            // Generate invoice data
            var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // -----------------------------
                    // Header (single definition)
                    // -----------------------------
                    page.Header().Column(headerColumn =>
                    {
                        // Top centered Invoice title
                        //headerColumn.Item().AlignCenter().Text("Invoice")
                        //    .FontSize(20).Bold()
                        //    .PaddingBottom(10); // add some spacing

                        // Header row: left (company) and right (invoice info)
                        headerColumn.Item().Row(row =>
                        {
                            // Left side: company info
                            row.RelativeItem().Column(column =>
                            {
                                column.Item().Text("EasyPOS")
                                    .FontSize(16).Bold().FontColor(Colors.Blue.Medium);
                                column.Item().Text("Gorczany - Mitchell");
                                column.Item().Text("566 Jovan Shoals, East Edythe");
                                column.Item().Text("PA, 42103-3716, Eritrea");
                            });

                            // Right side: invoice details
                            row.RelativeItem().AlignRight().Column(column =>
                            {
                                column.Item().Text("INVOICE")
                                    .FontSize(16).Bold().FontColor(Colors.Blue.Medium);
                                column.Item().Text($"Invoice Number: {invoiceData.InvoiceNumber}");
                                column.Item().Text($"Date of Issue: {invoiceData.InvoiceDate:yyyy-MM-dd}");
                                column.Item().Text($"Due Date: {invoiceData.DueDate:yyyy-MM-dd}");
                            });
                        });
                    });

                    // -----------------------------
                    // Main Content
                    // -----------------------------
                    page.Content().Column(column =>
                    {
                        // INVOICE TO:
                        column.Item().PaddingVertical(15).Column(innerCol =>
                        {
                            innerCol.Item().Text("INVOICE TO:")
                                .Bold().FontColor(Colors.Blue.Medium);
                            innerCol.Item().Text(invoiceData.ClientName);
                            innerCol.Item().Text(invoiceData.ClientAddress);
                            innerCol.Item().Text($"{invoiceData.ClientCity}, {invoiceData.ClientState}, {invoiceData.ClientPostal}");
                            innerCol.Item().Text(invoiceData.ClientCountry);
                        });

                        // Line Items Table
                        column.Item().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4); // Description
                                columns.RelativeColumn(1); // Qty
                                columns.RelativeColumn(2); // Unit Price
                                columns.RelativeColumn(2); // Total
                            });

                            // Table Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Description").Bold();
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Qty").Bold().AlignCenter();
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Unit Price").Bold().AlignRight();
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Total").Bold().AlignRight();
                            });

                            // Table Content
                            foreach (var item in invoiceData.LineItems)
                            {
                                decimal price = decimal.Parse(item.Price, CultureInfo.InvariantCulture);
                                decimal total = price * item.Quantity;

                                table.Cell().Padding(5).Text(item.Name);
                                table.Cell().Padding(5).Text(item.Quantity.ToString()).AlignCenter();
                                table.Cell().Padding(5).Text($"${price:F2}").AlignRight();
                                table.Cell().Padding(5).Text($"${total:F2}").AlignRight();
                            }
                        });

                        // Summary Table (Right-Aligned), near the bottom
                        column.Item().PaddingTop(20).AlignRight().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                            });

                            table.Cell().Text("Subtotal").Bold();
                            table.Cell().Text($"${invoiceData.Subtotal:F2}").AlignRight();

                            table.Cell().Text("Discount").Bold();
                            table.Cell().Text($"${invoiceData.Discount:F2}").AlignRight();

                            table.Cell().Text("Subtotal Less Discount").Bold();
                            table.Cell().Text($"${invoiceData.SubtotalLessDiscount:F2}").AlignRight();

                            table.Cell().Text("Tax (0.1%)").Bold();
                            table.Cell().Text($"${invoiceData.TaxTotal:F2}").AlignRight();

                            table.Cell().Text("Balance Due").Bold().FontColor(Colors.Blue.Medium);
                            table.Cell().Text($"${invoiceData.BalanceDue:F2}")
                                .Bold().AlignRight().FontColor(Colors.Blue.Medium);
                        });

                        // Thank you / Payment Info
                        column.Item().PaddingTop(15).Column(msgCol =>
                        {
                            msgCol.Item().Text("Thank you for your business!")
                                .SemiBold().FontColor(Colors.Blue.Medium);
                            msgCol.Item().Text("Payment is due within 30 days. Late payments may be subject to additional fees.")
                                .FontSize(9).Italic();
                        });
                    });

                    // -----------------------------
                    // Footer
                    // -----------------------------
                    page.Footer()
                        .AlignRight()
                        .Text($"Printed on {DateTime.Now:yyyy-MM-dd HH:mm:ss} Page 1 of 1")
                        .FontColor(Colors.Grey.Medium);
                });
            });

            var pdfStream = new MemoryStream();
            document.GeneratePdf(pdfStream);
            pdfStream.Position = 0;

            return Results.File(pdfStream, "application/pdf", "invoice.pdf");
        })
        .WithName("questPdf-get-invoice-pdf")
        .WithOpenApi();

        group.MapGet("get-invoice-pdf2", (int? lineItemCount = 10) =>
        {
            // Generate invoice data
            var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

            QuestPDF.Settings.License = LicenseType.Community;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(30);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

                    // -----------------------------
                    // Header (ONLY the logo) - REDUCED HEIGHT
                    // -----------------------------
                    page.Header().Height(50).Row(headerRow => // Set fixed height for header
                    {
                        // Left side: only the logo
                        headerRow.RelativeItem().Column(column =>
                        {
                            // Add logo only
                            string logoPath = "wwwroot/easypos_logo.jpg";
                            if (File.Exists(logoPath))
                            {
                                column.Item().Height(40).Image(logoPath);
                            }
                        });

                        // Empty right side to balance the header
                        headerRow.RelativeItem();
                    });

                    // -----------------------------
                    // Main Content - REDUCED TOP PADDING
                    // -----------------------------
                    page.Content().PaddingTop(5).Column(column => // Reduced padding from default to 5
                    {
                        // Invoice details and company info (MOVED FROM HEADER TO CONTENT - first page only)
                        column.Item().Row(row =>
                        {
                            // Left side: company info
                            row.RelativeItem().Column(companyCol =>
                            {
                                companyCol.Item().Text("EasyPOS")
                                    .FontSize(16).Bold().FontColor(Colors.Blue.Medium);
                                companyCol.Item().Text("Gorczany - Mitchell");
                                companyCol.Item().Text("566 Jovan Shoals, East Edythe");
                                companyCol.Item().Text("PA, 42103-3716, Eritrea");
                            });

                            // Right side: invoice details
                            row.RelativeItem().AlignRight().Column(invoiceCol =>
                            {
                                invoiceCol.Item().Text("INVOICE")
                                    .FontSize(16).Bold().FontColor(Colors.Blue.Medium);
                                invoiceCol.Item().Text($"Invoice Number: {invoiceData.InvoiceNumber}");
                                invoiceCol.Item().Text($"Date of Issue: {invoiceData.InvoiceDate:yyyy-MM-dd}");
                                invoiceCol.Item().Text($"Due Date: {invoiceData.DueDate:yyyy-MM-dd}");
                            });
                        });

                        // INVOICE TO:
                        column.Item().PaddingVertical(15).Column(innerCol =>
                        {
                            innerCol.Item().Text("INVOICE TO:")
                                .Bold().FontColor(Colors.Blue.Medium);
                            innerCol.Item().Text(invoiceData.ClientName);
                            innerCol.Item().Text(invoiceData.ClientAddress);
                            innerCol.Item().Text($"{invoiceData.ClientCity}, {invoiceData.ClientState}, {invoiceData.ClientPostal}");
                            innerCol.Item().Text(invoiceData.ClientCountry);
                        });

                        // Line Items Table
                        column.Item().PaddingTop(15).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(4); // Description
                                columns.RelativeColumn(1); // Qty
                                columns.RelativeColumn(2); // Unit Price
                                columns.RelativeColumn(2); // Total
                            });

                            // Table Header
                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Description").Bold();
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Qty").Bold().AlignCenter();
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Unit Price").Bold().AlignRight();
                                header.Cell().Background(Colors.Blue.Lighten4).Padding(5)
                                    .Text("Total").Bold().AlignRight();
                            });

                            // Table Content
                            foreach (var item in invoiceData.LineItems)
                            {
                                decimal price = decimal.Parse(item.Price, CultureInfo.InvariantCulture);
                                decimal total = price * item.Quantity;

                                table.Cell().Padding(5).Text(item.Name);
                                table.Cell().Padding(5).Text(item.Quantity.ToString()).AlignCenter();
                                table.Cell().Padding(5).Text($"${price:F2}").AlignRight();
                                table.Cell().Padding(5).Text($"${total:F2}").AlignRight();
                            }

                            // Add bottom border to the table
                            table.Cell().ColumnSpan(4).BorderBottom(1).BorderColor(Colors.Grey.Medium);
                        });

                        // Summary Table (Right-Aligned), near the bottom
                        column.Item().PaddingTop(20).AlignRight().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                            });

                            table.Cell().Text("Subtotal").Bold();
                            table.Cell().Text($"${invoiceData.Subtotal:F2}").AlignRight();

                            table.Cell().Text("Discount").Bold();
                            table.Cell().Text($"${invoiceData.Discount:F2}").AlignRight();

                            table.Cell().Text("Subtotal Less Discount").Bold();
                            table.Cell().Text($"${invoiceData.SubtotalLessDiscount:F2}").AlignRight();

                            table.Cell().Text("Tax (0.1%)").Bold();
                            table.Cell().Text($"${invoiceData.TaxTotal:F2}").AlignRight();

                            table.Cell().Text("Balance Due").Bold().FontColor(Colors.Blue.Medium);
                            table.Cell().Text($"${invoiceData.BalanceDue:F2}")
                                .Bold().AlignRight().FontColor(Colors.Blue.Medium);
                        });

                        // Thank you / Payment Info
                        column.Item().PaddingTop(15).Column(msgCol =>
                        {
                            msgCol.Item().Text("Thank you for your business!")
                                .SemiBold().FontColor(Colors.Blue.Medium);
                            msgCol.Item().Text("Payment is due within 30 days. Late payments may be subject to additional fees.")
                                .FontSize(9).Italic();
                        });

                        // QR Code and Barcode
                        column.Item().PaddingTop(15).Row(codeRow =>
                        {
                            // QR Code
                            codeRow.RelativeItem().Height(80).Svg(size =>
                            {
                                var content = invoiceData.InvoiceNumber;

                                var writer = new QRCodeWriter();
                                var qrCode = writer.encode(content, BarcodeFormat.QR_CODE, (int)size.Width, (int)size.Height);
                                var renderer = new SvgRenderer { FontName = "Arial" };
                                return renderer.Render(qrCode, BarcodeFormat.QR_CODE, null).Content;
                            });

                            // Barcode
                            codeRow.RelativeItem().Height(80).Svg(size =>
                            {
                                var content = invoiceData.InvoiceNumber;

                                var writer = new Code128Writer();
                                var barcode = writer.encode(content, BarcodeFormat.CODE_128, (int)size.Width, (int)(size.Height * 0.4));
                                var renderer = new SvgRenderer { FontName = "Arial" };
                                return renderer.Render(barcode, BarcodeFormat.CODE_128, content).Content;
                            });
                        });
                    });

                    // -----------------------------
                    // Footer
                    // -----------------------------
                    page.Footer().Row(footerRow =>
                    {
                        // Print date
                        footerRow.RelativeItem().Text($"Printed on {DateTime.Now:yyyy-MM-dd HH:mm:ss}")
                            .FontColor(Colors.Grey.Medium);

                        // Right side - Page Numbers
                        footerRow.RelativeItem().AlignRight().AlignMiddle()
                        .Text(text =>
                        {
                            text.Span("Page ").FontColor(Colors.Grey.Medium);
                            text.CurrentPageNumber().FontColor(Colors.Grey.Medium);
                            text.Span(" of ").FontColor(Colors.Grey.Medium);
                            text.TotalPages().FontColor(Colors.Grey.Medium);
                        });
                    });
                });
            });

            var pdfStream = new MemoryStream();
            document.GeneratePdf(pdfStream);
            pdfStream.Position = 0;

            return Results.File(pdfStream, "application/pdf", "invoice.pdf");
        })
        .WithName("questPdf-get-invoice-pdf2")
        .WithOpenApi();

        return routes;
    }
}
