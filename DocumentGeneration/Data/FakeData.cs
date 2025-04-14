using Bogus;
using System.Globalization;

namespace DocumentGeneration.Data;

public class FakeData
{

    public static InvoiceData GenerateInvoiceData(int lineItemCount = 10)
    {
        var faker = new Faker();

        var invoiceData = new InvoiceData
        {
            // Company information
            CompanyName = faker.Company.CompanyName(),
            CompanyAddress = faker.Address.StreetAddress(),
            CompanyCity = faker.Address.City(),
            CompanyState = faker.Address.StateAbbr(),
            CompanyPostal = faker.Address.ZipCode(),
            CompanyCountry = faker.Address.Country(),

            // Invoice information
            InvoiceNumber = faker.Random.Int(1, 9999).ToString("0000"),
            InvoiceDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),
            DueDate = DateTime.UtcNow.ToString("yyyy-MM-dd"),

            // Client information
            ClientName = faker.Company.CompanyName(),
            ClientAddress = faker.Address.StreetAddress(),
            ClientCity = faker.Address.City(),
            ClientState = faker.Address.StateAbbr(),
            ClientPostal = faker.Address.ZipCode(),
            ClientCountry = faker.Address.Country(),

            // Line items
            LineItems = GetLineItems(lineItemCount),

            // Notes and terms
            Notes = "Thank you for your business!",
            Terms = "Payment is due within 30 days. Late payments may be subject to additional fees."
        };

        // Calculate totals
        invoiceData.Subtotal = invoiceData.LineItems.Sum(x =>
            decimal.Parse(x.Price, CultureInfo.InvariantCulture) * x.Quantity);
        invoiceData.Discount = invoiceData.Subtotal * 0.05m; // 5% Discount
        invoiceData.SubtotalLessDiscount = invoiceData.Subtotal - invoiceData.Discount;
        invoiceData.TaxRate = 0.1m; // 10% Tax Rate
        invoiceData.TaxTotal = invoiceData.SubtotalLessDiscount * invoiceData.TaxRate;
        invoiceData.BalanceDue = invoiceData.SubtotalLessDiscount + invoiceData.TaxTotal;

        return invoiceData;
    }

    public static List<LineItem> GetLineItems(int count = 10)
    {
        var faker = new Faker();
        return Enumerable.Range(1, count).Select(i => new LineItem
        {
            Index = i,
            Name = faker.Commerce.ProductName(),
            Quantity = faker.Random.Int(1, 5),
            Price = faker.Commerce.Price(10, 1000)
        }).ToList();
    }
}
