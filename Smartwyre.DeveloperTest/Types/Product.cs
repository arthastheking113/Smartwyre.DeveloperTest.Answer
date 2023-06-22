namespace Smartwyre.DeveloperTest.Types;

public class Product
{
    public int Id { get; set; }
    public string Identifier { get; set; }
    public decimal Price { get; set; }
    public string Uom { get; set; }
    public SupportedIncentiveType SupportedIncentives { get; set; }
    public bool IsValidIncentive(SupportedIncentiveType incentiveType)
    {
        return SupportedIncentives.HasFlag(incentiveType);
    }
}
