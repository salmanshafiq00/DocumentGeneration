namespace DocumentGeneration.Models;

public class InvoiceData
{
    // Company information
    public string CompanyName { get; set; }
    public string CompanyAddress { get; set; }
    public string CompanyCity { get; set; }
    public string CompanyState { get; set; }
    public string CompanyPostal { get; set; }
    public string CompanyCountry { get; set; }

    // Invoice information
    public string InvoiceNumber { get; set; }
    public string InvoiceDate { get; set; }
    public string DueDate { get; set; }

    // Client information
    public string ClientName { get; set; }
    public string ClientAddress { get; set; }
    public string ClientCity { get; set; }
    public string ClientState { get; set; }
    public string ClientPostal { get; set; }
    public string ClientCountry { get; set; }

    // Line items and calculations
    public List<LineItem> LineItems { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Discount { get; set; }
    public decimal SubtotalLessDiscount { get; set; }
    public decimal TaxRate { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal BalanceDue { get; set; }

    // Notes and terms
    public string Notes { get; set; }
    public string Terms { get; set; }

    // New properties for QR code, barcode, and logo
    public string QRCodeDataUrl { get; set; }
    public string BarcodeDataUrl { get; set; }
    public string LogoUrl { get; set; }
}
