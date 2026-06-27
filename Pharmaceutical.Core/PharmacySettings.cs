namespace Pharmaceutical.Core;

public class PharmacySettings
{
    public const string SectionName = "PharmacySettings";

    public int LowStockThreshold { get; set; } = 200;
}
