# PDF Report Generation in .NET - A Comprehensive Guide

This repository demonstrates three different approaches for generating PDF reports in .NET applications: QuestPDF, PuppeteerSharp, and Playwright. Each method offers its own advantages and trade-offs for creating professional invoices with headers, footers, tables, images, QR codes, and barcodes.

## Table of Contents
- [Overview](#overview)
- [Common Setup](#common-setup)
- [QuestPDF Implementation](#questpdf-implementation)
- [PuppeteerSharp Implementation](#puppeteersharp-implementation)
- [Playwright Implementation](#playwright-implementation)
- [Comparison](#comparison)

## Overview

This project showcases three different PDF generation approaches:

1. **QuestPDF**: A .NET library for programmatic PDF creation with fluent API
2. **PuppeteerSharp**: A headless Chrome automation library that can generate PDFs from HTML
3. **Playwright**: A modern browser automation library with PDF generation capabilities

Each approach includes endpoints for generating basic invoices and enhanced invoices with QR codes and barcodes.

## Common Setup

### Shared Dependencies

```bash
# Common data generation library
dotnet add package Bogus --version 34.0.2
```

If you're planning to use sample data for testing, the Bogus library helps create realistic fake data:

```csharp
// Example of fake data generation
var invoiceData = FakeData.GenerateInvoiceData(lineItemCount ?? 10);
```

## QuestPDF Implementation

QuestPDF is a .NET library that provides a fluent API for creating PDF documents programmatically.

### Step 1: Install Required Packages

```bash
dotnet add package QuestPDF --version 2023.6.0
dotnet add package ZXing.Net --version 0.16.9
```

### Step 2: Basic Configuration

```csharp
// Set license type in your application startup
QuestPDF.Settings.License = LicenseType.Community;
```

### Step 3: Define Document Structure (Header, Content, Footer)

```csharp
var document = Document.Create(container =>
{
    container.Page(page =>
    {
        // Page settings
        page.Size(PageSizes.A4);
        page.Margin(30);
        page.PageColor(Colors.White);
        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Arial"));

        // Header
        page.Header().Column(headerColumn =>
        {
            headerColumn.Item().Row(row =>
            {
                // Company info (left side)
                row.RelativeItem().Column(column =>
                {
                    column.Item().Text("EasyPOS").FontSize(16).Bold().FontColor(Colors.Blue.Medium);
                    column.Item().Text("Company Address Line 1");
                    column.Item().Text("Company Address Line 2");
                    column.Item().Text("Company Address Line 3");
                });

                // Invoice details (right side)
                row.RelativeItem().AlignRight().Column(column =>
                {
                    column.Item().Text("INVOICE").FontSize(16).Bold().FontColor(Colors.Blue.Medium);
                    column.Item().Text($"Invoice Number: {invoiceData.InvoiceNumber}");
                    column.Item().Text($"Date of Issue: {invoiceData.InvoiceDate:yyyy-MM-dd}");
                    column.Item().Text($"Due Date: {invoiceData.DueDate:yyyy-MM-dd}");
                });
            });
        });

        // Content
        page.Content().Column(column =>
        {
            // Client info
            column.Item().PaddingVertical(15).Column(innerCol =>
            {
                innerCol.Item().Text("INVOICE TO:").Bold().FontColor(Colors.Blue.Medium);
                innerCol.Item().Text(invoiceData.ClientName);
                innerCol.Item().Text(invoiceData.ClientAddress);
                innerCol.Item().Text($"{invoiceData.ClientCity}, {invoiceData.ClientState}, {invoiceData.ClientPostal}");
                innerCol.Item().Text(invoiceData.ClientCountry);
            });

            // Line items table
            column.Item().PaddingTop(15).Table(table =>
            {
                // Define columns
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(4); // Description
                    columns.RelativeColumn(1); // Qty
                    columns.RelativeColumn(2); // Unit Price
                    columns.RelativeColumn(2); // Total
                });

                // Add table header
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

                // Add table rows
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

            // Summary table
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

            // Thank you message
            column.Item().PaddingTop(15).Column(msgCol =>
            {
                msgCol.Item().Text("Thank you for your business!")
                    .SemiBold().FontColor(Colors.Blue.Medium);
                msgCol.Item().Text("Payment is due within 30 days. Late payments may be subject to additional fees.")
                    .FontSize(9).Italic();
            });
        });

        // Footer
        page.Footer()
            .AlignRight()
            .Text($"Printed on {DateTime.Now:yyyy-MM-dd HH:mm:ss} Page 1 of 1")
            .FontColor(Colors.Grey.Medium);
    });
});

// Generate PDF
var pdfStream = new MemoryStream();
document.GeneratePdf(pdfStream);
pdfStream.Position = 0;
```

### Step 4: Add Company Logo to Header

```csharp
// In the header section or row
string logoPath = "wwwroot/easypos_logo.jpg";
if (File.Exists(logoPath))
{
    column.Item().Height(40).Image(logoPath);
}
```

### Step 5: Add QR Code and Barcode

```csharp
// In the content section, after the thank you message
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
```

### QuestPDF Pros and Cons

**Pros:**
- Pure .NET solution without browser dependencies
- Programmatic control over every aspect of the PDF
- No HTML/CSS knowledge required
- Fast and efficient for simple to moderately complex documents
- Works well in containerized environments

**Cons:**
- Steeper learning curve for complex layouts
- Limited styling options compared to HTML/CSS
- Community version has limitations (watermark for commercial use)

### QuestPDF Dependencies

- **QuestPDF**: Core library for PDF generation with fluent API
- **ZXing.Net**: Used for barcode and QR code generation

## PuppeteerSharp Implementation

PuppeteerSharp is a .NET library that provides a way to control headless Chrome/Chromium browsers for tasks including PDF generation from HTML.

### Step 1: Install Required Packages

```bash
dotnet add package PuppeteerSharp --version 10.1.0
dotnet add package Razor.Templating.Core --version 1.8.0
dotnet add package ZXing.Net --version 0.16.9
dotnet add package SkiaSharp --version 2.88.3
```

### Step 2: Basic Configuration

```csharp
// Download Chromium browser during application startup
await new BrowserFetcher().DownloadAsync();
```

### Step 3: Create HTML Template and Generate PDF with Header, Content, and Footer

First, create an HTML template for your invoice (e.g., `Views/invoice_with_qr.cshtml`):

```html
<!DOCTYPE html>
<html>
<head>
    <title>Invoice</title>
    <style>
        /* Your CSS styling here */
        body { font-family: Arial, sans-serif; }
        .header { display: flex; justify-content: space-between; }
        .invoice-title { font-size: 24px; color: #2962ff; }
        /* Additional styles... */
    </style>
</head>
<body>
    <div class="content">
        <!-- Invoice content with client info, line items, etc. -->
        <!-- QR code and barcode placeholders -->
        <div class="codes">
            <img src="@Model.QRCodeDataUrl" alt="QR Code" />
            <img src="@Model.BarcodeDataUrl" alt="Barcode" />
        </div>
    </div>
</body>
</html>
```

Then, generate the PDF:

```csharp
private static async Task<byte[]> GeneratePdfFromHtml(string htmlContent, IWebHostEnvironment webHostEnvironment)
{
    // Initialize Puppeteer
    await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
    {
        Headless = true,
        Args = ["--no-sandbox", "--disable-setuid-sandbox"]
    });
    await using var page = await browser.NewPageAsync();

    // Set HTML content
    await page.SetContentAsync(htmlContent);

    // Get current date and format it
    string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    // Generate PDF with header and footer
    return await page.PdfDataAsync(new PdfOptions
    {
        Format = PaperFormat.A4,
        PrintBackground = true,
        DisplayHeaderFooter = true,
        
        // Define header template
        HeaderTemplate = $@"
        <div style='width:100%; display:flex; justify-content:center;'>
            <!-- Logo will be placed here -->
        </div>",
        
        // Define footer template
        FooterTemplate = $@"
        <div style='width:100%; font-size:10px; padding:10px 20px; display:flex; justify-content:space-between;'>
            <span style='text-align:left;'>Printed on {currentDateTime}</span>
            <span style='text-align:right;'>Page <span class='pageNumber'></span> of <span class='totalPages'></span></span>
        </div>",
        
        // Set margins
        MarginOptions = new MarginOptions
        {
            Top = "40px",
            Bottom = "40px",
            Left = "20px",
            Right = "20px"
        }
    });
}
```

### Step 4: Add Logo to Header

```csharp
// Get the absolute path to the logo
string logoFilePath = Path.Combine(webHostEnvironment.WebRootPath, "easypos_logo.jpg");

// Convert logo to base64 data URL
string logoBase64 = Convert.ToBase64String(File.ReadAllBytes(logoFilePath));
string logoDataUrl = $"data:image/jpeg;base64,{logoBase64}";

// Use in header template
HeaderTemplate = $@"
<div style='width:100%; display:flex; justify-content:center;'>
    <img src='{logoDataUrl}' style='height:80px; max-width:100%;' alt='EasyPOS Logo'>
</div>",
```

### Step 5: Add QR Code and Barcode

For PuppeteerSharp, we'll generate data URLs for QR code and barcode and pass them to the template:

```csharp
// Generate QR code and barcode
string qrCodeDataUrl = GenerateQRCodeDataUrl(invoiceData.InvoiceNumber);
string barcodeDataUrl = GenerateBarcodeDataUrl(invoiceData.InvoiceNumber);

// Add to invoice data
invoiceData.QRCodeDataUrl = qrCodeDataUrl;
invoiceData.BarcodeDataUrl = barcodeDataUrl;

// Helper methods for QR code and barcode generation
public static string GenerateQRCodeDataUrl(string content)
{
    var writer = new BarcodeWriterPixelData
    {
        Format = BarcodeFormat.QR_CODE,
        Options = new QrCodeEncodingOptions
        {
            ErrorCorrection = ErrorCorrectionLevel.H,
            Height = 200,
            Width = 200,
            Margin = 1
        }
    };
    var pixelData = writer.Write(content);
    
    // Convert to SkiaSharp bitmap and then to data URL
    var info = new SKImageInfo(pixelData.Width, pixelData.Height);
    using var surface = SKSurface.Create(info);
    unsafe
    {
        fixed (byte* p = pixelData.Pixels)
        {
            using var skData = SKData.Create((IntPtr)p, pixelData.Pixels.Length);
            var skImage = SKImage.FromPixels(info, skData, pixelData.Width * 4);
            surface.Canvas.DrawImage(skImage, 0, 0);
        }
    }
    using var image = surface.Snapshot();
    using var data = image.Encode(SKEncodedImageFormat.Png, 100);
    
    // Convert to base64
    var base64String = Convert.ToBase64String(data.ToArray());
    return $"data:image/png;base64,{base64String}";
}

public static string GenerateBarcodeDataUrl(string content)
{
    var writer = new BarcodeWriterPixelData
    {
        Format = BarcodeFormat.CODE_128,
        Options = new ZXing.Common.EncodingOptions
        {
            Height = 80,
            Width = 220,
            Margin = 1
        }
    };
    
    // Similar conversion as QR code
    // ... (implementation similar to QR code)
    
    return $"data:image/png;base64,{base64String}";
}

// Helper method to render Razor templates
public static async Task<string> GenerateHtmlContent<T>(T model, string viewName)
{
    return await RazorTemplateEngine.RenderAsync($"Views/{viewName}.cshtml", model);
}
```

### PuppeteerSharp Pros and Cons

**Pros:**
- Leverage HTML/CSS skills for document layout
- Full web rendering capabilities
- Excellent for complex layouts and styles
- Better support for modern CSS features

**Cons:**
- Requires Chromium installation
- Higher resource consumption
- Slower than direct PDF generation
- May have issues in containerized environments

### PuppeteerSharp Dependencies

- **PuppeteerSharp**: Core library for controlling headless Chrome/Chromium
- **Razor.Templating.Core**: Used for rendering Razor views as HTML templates
- **ZXing.Net**: Used for generating barcode and QR code data
- **SkiaSharp**: Used for image processing to create barcode/QR code images

## Playwright Implementation

Playwright is a modern browser automation library with capabilities for PDF generation.

### Step 1: Install Required Packages

```bash
dotnet add package Microsoft.Playwright --version 1.34.0
dotnet add package Razor.Templating.Core --version 1.8.0
dotnet add package ZXing.Net --version 0.16.9
dotnet add package SkiaSharp --version 2.88.3
```

### Step 2: Basic Configuration

```csharp
// Install browser binaries using CLI before running the application
// dotnet tool install --global Microsoft.Playwright.CLI
// playwright install chromium
```

### Step 3: Generate PDF with Header, Content, and Footer

```csharp
private static async Task<byte[]> GeneratePdfFromHtml(string htmlContent)
{
    // Initialize Playwright
    using var playwright = await Playwright.CreateAsync();
    await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
    {
        Headless = true
    });
    var page = await browser.NewPageAsync();
    
    // Set HTML content
    await page.SetContentAsync(htmlContent);
    
    // Get current date
    string currentDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    
    // Generate PDF with header and footer
    return await page.PdfAsync(new PagePdfOptions
    {
        Format = PaperFormat.A4,
        PrintBackground = true,
        DisplayHeaderFooter = true,
        
        // Header template
        HeaderTemplate = @"
            <div style='width:100%; font-size:12px; padding:10px 20px; font-weight:bold; text-align:center;'>
                Invoice Details
            </div>",
            
        // Footer template
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
```

### Step 4: Add Logo to Header

```csharp
// Get logo path
string logoFilePath = Path.Combine(webHostEnvironment.WebRootPath, "easypos_logo.jpg");

// Convert logo to base64 data URL
string logoBase64 = Convert.ToBase64String(File.ReadAllBytes(logoFilePath));
string logoDataUrl = $"data:image/jpeg;base64,{logoBase64}";

// Use in header template
HeaderTemplate = $@"
    <div style='width:100%; display:flex; justify-content:center;'>
        <img src='{logoDataUrl}' style='height:80px; max-width:100%;' alt='EasyPOS Logo'>
    </div>",
```

### Step 5: Add QR Code and Barcode

Playwright uses the same approach as PuppeteerSharp for QR code and barcode generation:

```csharp
// Generate QR code and barcode data URLs
string qrCodeDataUrl = UtilitiesExtension.GenerateQRCodeDataUrl(invoiceData.InvoiceNumber);
string barcodeDataUrl = UtilitiesExtension.GenerateBarcodeDataUrl(invoiceData.InvoiceNumber);

// Add to invoice data model
invoiceData.QRCodeDataUrl = qrCodeDataUrl;
invoiceData.BarcodeDataUrl = barcodeDataUrl;

// The QRCodeDataUrl and BarcodeDataUrl properties will be used in the HTML template
```

The QR code and barcode generation helpers are the same as in the PuppeteerSharp implementation:
- `GenerateQRCodeDataUrl` and `GenerateBarcodeDataUrl` using ZXing.Net and SkiaSharp
- `GenerateHtmlContent` using Razor.Templating.Core

### Playwright Pros and Cons

**Pros:**
- Modern browser automation with better API
- Cross-browser support
- Better performance than PuppeteerSharp
- Great developer experience

**Cons:**
- Similar drawbacks to PuppeteerSharp (resource usage, dependencies)
- Newer library with potentially fewer examples

### Playwright Dependencies

- **Microsoft.Playwright**: Core library for browser automation
- **Razor.Templating.Core**: Used for rendering Razor views as HTML templates
- **ZXing.Net**: Used for generating barcode and QR code data
- **SkiaSharp**: Used for image processing to create barcode/QR code images

## Comparison

| Feature | QuestPDF | PuppeteerSharp | Playwright |
|---------|----------|----------------|------------|
| **Type** | Native PDF | HTML-to-PDF | HTML-to-PDF |
| **Dependencies** | Minimal | Browser + Libraries | Browser + Libraries |
| **Performance** | Fast | Slower | Moderate |
| **Complexity** | Medium | Low (if familiar with HTML) | Low (if familiar with HTML) |
| **Styling** | Limited | Full CSS | Full CSS |
| **Resource Usage** | Low | High | High |
| **Container Support** | Excellent | Requires Setup | Requires Setup |
| **Learning Curve** | Steeper for complex layouts | Lower with HTML knowledge | Lower with HTML knowledge |
| **Maintenance** | Active Community | Active Community | Active Community |

## Choose the Right Approach

- **QuestPDF**: Best for applications with simple to moderate PDF needs where performance and resource usage are concerns.
- **PuppeteerSharp**: Ideal when you want to leverage HTML/CSS skills or need complex layouts.
- **Playwright**: Good alternative to PuppeteerSharp with better API and performance.

Each approach has its strengths, so pick the one that best suits your specific requirements and team's skills.
