namespace DocumentGeneration.Models;

public class LineItem
{
    public int Index { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Price { get; set; } = string.Empty;
}
