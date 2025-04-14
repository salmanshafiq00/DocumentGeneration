using Microsoft.Playwright;

namespace DocumentGeneration.Endpoints;

public static class PlaywrightPdfGenerate
{
    public static IEndpointRouteBuilder MapPlaywrightPdfGenerate(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("playwrightPdf");

        group.MapGet("get-invoice-pdf", async (int? lineItemCount = 10) =>
        {
            // Generate invoice data
            var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

            // Create HTML content from template
            string htmlContent = await UtilitiesExtension.GenerateHtmlContent<InvoiceData>(invoiceData, "invoice");

            // Generate PDF
            byte[] pdfBytes = await GeneratePdfFromHtml(htmlContent);

            return Results.File(pdfBytes, "application/pdf", $"invoice-{invoiceData.InvoiceNumber}.pdf");
        })
        .WithName("playwrightPdf-get-invoice-pdf")
        .WithOpenApi();

        return routes;
    }

    private static async Task<byte[]> GeneratePdfFromHtml(string htmlContent)
    {
        // Ensure Playwright is installed
        using var playwright = await Playwright.CreateAsync();

        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var page = await browser.NewPageAsync();

        // Load HTML content
        await page.SetContentAsync(htmlContent);

        // Get current date and format it
        string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Generate PDF with header and footer
        return await page.PdfAsync(new PagePdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,

            // Header configuration
            DisplayHeaderFooter = true,
            HeaderTemplate = @"
                <div style='width:100%; font-size:12px; padding:10px 20px; font-weight:bold; text-align:center;'>
                    Invoice Details
                </div>",

            // Footer configuration
            FooterTemplate = $@"
                <div style='width:100%; font-size:10px; padding:10px 20px; display:flex; justify-content:space-between;'>
                    <span style='text-align:left;'>Printed on {currentDateTime}</span>
                    <span style='text-align:right;'>Page <span class='pageNumber'></span> of <span class='totalPages'></span></span>
                </div>",

            // Margin configuration (corrected)
            Margin = new Margin
            {
                Top = "60px",
                Bottom = "40px",
                Left = "20px",
                Right = "20px"
            }
        });
    }
}
