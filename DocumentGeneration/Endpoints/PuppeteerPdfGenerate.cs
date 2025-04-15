using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace DocumentGeneration.Endpoints;

public static class PuppeteerPdfGenerate
{
    public static IEndpointRouteBuilder MapPuppeteerPdfGenerate(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("puppeteerPdf");

        group.MapGet("get-invoice-preview", async (int? lineItemCount = 10) =>
        {
            var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

            // Create HTML content from template
            string htmlContent = await UtilitiesExtension.GenerateHtmlContent<InvoiceData>(invoiceData, "invoice_with_qr");

            return Results.Content(htmlContent, "text/html");
        })
        .WithName("puppeteer-get-invoice-preview")
        .WithOpenApi();

        group.MapGet("get-invoice-pdf", async (IWebHostEnvironment webHostEnvironment, int? lineItemCount = 10) =>
        {
            // Generate invoice data
            var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);

            // Create HTML content from template
            string htmlContent = await UtilitiesExtension.GenerateHtmlContent<InvoiceData>(invoiceData, "invoice");

            // Generate PDF
            // Convert HTML to PDF using PuppeteerSharp
            byte[] pdfBytes = await GeneratePdfFromHtml(htmlContent, webHostEnvironment);

            return Results.File(pdfBytes, "application/pdf", $"invoice-{invoiceData.InvoiceNumber}.pdf");
        })
        .WithName("puppeteerPdf-get-invoice-pdf")
        .WithOpenApi();

        /// <summary>
        /// Generates an invoice PDF document with QR code and barcode
        /// </summary>
        /// <param name="webHostEnvironment">Web host environment to access static resources</param>
        /// <param name="lineItemCount">Optional parameter to specify the number of line items in the invoice (default: 10)</param>
        /// <returns>PDF file as a downloadable response</returns>
        /// <remarks>
        /// This endpoint creates a PDF invoice with:
        /// - Company and client information
        /// - Detailed line items for products/services
        /// - QR code and barcode for invoice tracking
        /// - Subtotal, discount, tax calculations
        /// - Header with logo and footer with page numbers
        /// </remarks>
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

            // Generate PDF
            byte[] pdfBytes = await GeneratePdfFromHtml(htmlContent, webHostEnvironment);

            return Results.File(pdfBytes, "application/pdf", $"invoice-{invoiceData.InvoiceNumber}.pdf");
        })
        .WithName("puppeteerPdf-get-invoice-pdf-qrcode")
        .WithOpenApi(operation => {
            operation.Summary = "Generate invoice PDF with QR code";
            operation.Description = "Creates a professional invoice PDF with company details, line items, QR code, and barcode";
            operation.Parameters[0].Description = "Number of line items to include in the invoice";
            return operation;
        });

        return routes;
    }

    private static async Task<byte[]> GeneratePdfFromHtml(string htmlContent, IWebHostEnvironment webHostEnvironment)
    {
        // Ensure Puppeteer is downloaded
        await new BrowserFetcher().DownloadAsync();

        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = ["--no-sandbox", "--disable-setuid-sandbox"]
        });
        await using var page = await browser.NewPageAsync();

        // Load HTML content
        await page.SetContentAsync(htmlContent);

        // Get current date and format it
        string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        // Get the absolute path to the logo
        string logoFilePath = Path.Combine(webHostEnvironment.WebRootPath, "easypos_logo.jpg");

        // Check if the file exists
        if (!File.Exists(logoFilePath))
        {
            Console.WriteLine($"Logo file not found at path: {logoFilePath}");
        }

        // Convert logo to base64 data URL
        string logoBase64 = Convert.ToBase64String(File.ReadAllBytes(logoFilePath));
        string logoDataUrl = $"data:image/jpeg;base64,{logoBase64}";

        // Create a proper file URL
        string logoUrl = $"file://{logoFilePath.Replace("\\", "/")}";
        Console.WriteLine($"Using logo URL: {logoUrl}");

        // Generate PDF with header and footer
        return await page.PdfDataAsync(new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            DisplayHeaderFooter = true,
            HeaderTemplate = $@"
            <div style='width:100%; display:flex; justify-content:center;'>
                <img src='{logoDataUrl}' style='height:80px; max-width:100%;' alt='EasyPOS Logo' onerror='console.error(""Logo failed to load"");'>
            </div>",
            FooterTemplate = $@"
            <div style='width:100%; font-size:10px; padding:10px 20px; display:flex; justify-content:space-between;'>
                <span style='text-align:left;'>Printed on {currentDateTime}</span>
                <span style='text-align:right;'>Page <span class='pageNumber'></span> of <span class='totalPages'></span></span>
            </div>",
            MarginOptions = new MarginOptions
            {
                Top = "40px",  // Ensure enough space for the header
                Bottom = "40px", // Ensure enough space for the footer
                Left = "20px",
                Right = "20px"
            }
        });
    }

    

    
}
