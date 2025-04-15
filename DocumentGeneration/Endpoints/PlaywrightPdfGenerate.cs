using Microsoft.Playwright;

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

        // New endpoint for invoice with QR code and barcode
        group.MapGet("get-invoice-pdf-qrcode", async (IWebHostEnvironment webHostEnvironment, int? lineItemCount = 10) =>
        {
            // Generate invoice data
            var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

            // Generate QR code and barcode
            string qrCodeDataUrl = UtilitiesExtension.GenerateQRCodeDataUrl(invoiceData.InvoiceNumber);
            string barcodeDataUrl = UtilitiesExtension.GenerateBarcodeDataUrl(invoiceData.InvoiceNumber);

            // Add barcode and QR code to the invoice data
            invoiceData.QRCodeDataUrl = qrCodeDataUrl;
            invoiceData.BarcodeDataUrl = barcodeDataUrl;

            // Create HTML content from template
            string htmlContent = await UtilitiesExtension.GenerateHtmlContent<InvoiceData>(invoiceData, "invoice_with_qr");

            // Get the absolute path to the logo
            string logoFilePath = Path.Combine(webHostEnvironment.WebRootPath, "easypos_logo.jpg");

            // Convert logo to base64 data URL
            string logoBase64 = Convert.ToBase64String(File.ReadAllBytes(logoFilePath));
            string logoDataUrl = $"data:image/jpeg;base64,{logoBase64}";

            // Generate PDF
            byte[] pdfBytes = await GeneratePdfFromHtmlWithLogo(htmlContent, logoDataUrl);

            return Results.File(pdfBytes, "application/pdf", $"invoice-{invoiceData.InvoiceNumber}.pdf");
        })
        .WithName("playwrightPdf-get-invoice-pdf-qrcode")
        .WithOpenApi(operation => {
            operation.Summary = "Generate invoice PDF with QR code";
            operation.Description = "Creates a professional invoice PDF with company details, line items, QR code, and barcode";
            operation.Parameters[0].Description = "Number of line items to include in the invoice";
            return operation;
        });

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
            // Margin configuration
            Margin = new Margin
            {
                Top = "60px",
                Bottom = "40px",
                Left = "20px",
                Right = "20px"
            }
        });
    }

    private static async Task<byte[]> GeneratePdfFromHtmlWithLogo(string htmlContent, string logoDataUrl)
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
            // Header configuration with logo
            DisplayHeaderFooter = true,
            HeaderTemplate = $@"
                <div style='width:100%; display:flex; justify-content:center;'>
                    <img src='{logoDataUrl}' style='height:80px; max-width:100%;' alt='EasyPOS Logo'>
                </div>",
            // Footer configuration
            FooterTemplate = $@"
                <div style='width:100%; font-size:10px; padding:10px 20px; display:flex; justify-content:space-between;'>
                    <span style='text-align:left;'>Printed on {currentDateTime}</span>
                    <span style='text-align:right;'>Page <span class='pageNumber'></span> of <span class='totalPages'></span></span>
                </div>",
            // Margin configuration
            Margin = new Margin
            {
                Top = "40px",
                Bottom = "40px",
                Left = "20px",
                Right = "20px"
            }
        });
    }

    
}