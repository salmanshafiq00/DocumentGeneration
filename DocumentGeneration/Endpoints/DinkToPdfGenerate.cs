using DinkToPdf;
using DinkToPdf.Contracts;
using System.Diagnostics;

namespace DocumentGeneration.Endpoints;

public static class DinkToPdfGenerate
{
    public static IEndpointRouteBuilder MapDinkToPdfGenerate(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("dinkToPdf");

        group.MapGet("get-invoice-pdf", async (HttpContext context, int? lineItemCount = 10) =>
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                // Get the DinkToPdf converter from services
                var converter = context.RequestServices.GetRequiredService<IConverter>();

                // Generate invoice data
                var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

                // Create HTML content from template - use the new CSS-based template
                string htmlContent = await UtilitiesExtension.GenerateHtmlContent<InvoiceData>(invoiceData, "invoice_dinktopdf");

                // Log time spent generating HTML
                context.Response.Headers.Append("X-HTML-Generation-Time", $"{stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Restart();

                // Generate PDF
                byte[] pdfBytes = GeneratePdfFromHtml(htmlContent, converter, invoiceData.InvoiceNumber);

                // Log time spent generating PDF
                context.Response.Headers.Append("X-PDF-Generation-Time", $"{stopwatch.ElapsedMilliseconds}ms");

                return Results.File(pdfBytes, "application/pdf", $"invoice-{invoiceData.InvoiceNumber}.pdf");
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "PDF Generation Failed",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
            finally
            {
                stopwatch.Stop();
            }
        })
        .WithName("dinkToPdf-get-invoice-pdf")
        .WithOpenApi();

        group.MapGet("get-invoice-preview", async (int? lineItemCount = 10) =>
        {
            var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);
            // Return the new CSS-based template for preview
            //string htmlContent = await UtilitiesExtension.GenerateHtmlContent<InvoiceData>(invoiceData, "invoice_dinktopdf");
            string htmlContent = await UtilitiesExtension.GenerateHtmlContent<InvoiceData>(invoiceData, "invoice_dinktopdf");
            return Results.Content(htmlContent, "text/html");
        })
        .WithName("dinkToPdf-get-invoice-preview")
        .WithOpenApi();

        return routes;
    }

    public static byte[] GeneratePdfFromHtml(string htmlContent, IConverter converter, string invoiceNumber)
    {
        // Get current date-time in required format
        string printedOn = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        var headerSettings = new HeaderSettings
        {
            Center = $"Invoice #{invoiceNumber}",  // ✅ Shows invoice number in header
            FontSize = 10,
            Line = false,  // ✅ Adds a separator line below the header
            Spacing = 5
        };

        var footerSettings = new FooterSettings
        {
            Left = $"Printed on {printedOn}",  // ✅ Shows date-time on the left
            Right = "Page [page] of [toPage]", // ✅ Shows page numbers on the right
            FontSize = 9,
            Line = false,  
            Spacing = 5
        };

        var globalSettings = new GlobalSettings
        {
            ColorMode = ColorMode.Color,
            Orientation = Orientation.Portrait,
            PaperSize = PaperKind.A4,
            Margins = new MarginSettings { Top = 10, Bottom = 25, Left = 4, Right = 4 }, // ✅ Increased bottom margin
            DocumentTitle = $"Invoice #{invoiceNumber}",
            DPI = 300, // ✅ High DPI for better quality
            UseCompression = true
        };

        var objectSettings = new ObjectSettings
        {
            PagesCount = true,  // ✅ Ensures [page] and [toPage] work
            HtmlContent = htmlContent,
            WebSettings = {
                DefaultEncoding = "utf-8",
                PrintMediaType = true,
                EnableJavascript = true,  // ✅ Ensures JavaScript execution
                LoadImages = true         // ✅ Ensures images are loaded
            },
            HeaderSettings = headerSettings,
            FooterSettings = footerSettings
        };

        var pdf = new HtmlToPdfDocument()
        {
            GlobalSettings = globalSettings,
            Objects = { objectSettings }
        };

        return converter.Convert(pdf);
    }
}