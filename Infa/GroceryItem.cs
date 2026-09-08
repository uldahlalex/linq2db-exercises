using System.ComponentModel.DataAnnotations;
using LinqToDB.Mapping;
using DataType = LinqToDB.DataType;

namespace Infa;

public enum StorageType
{
    [MapValue("ambient")] Ambient,
    [MapValue("chilled")] Chilled,
    [MapValue("frozen")] Frozen
}

public enum Supplier
{
    [MapValue("local")] LocalFarm,
    [MapValue("wholesale")] Wholesale,
    [MapValue("import")] Import
}

[Table("GroceryItems")]
public class GroceryItem
{
    [PrimaryKey] public Guid Id { get; set; }

    [Column, NotNull] public string Name { get; set; } = "";
    [Column] public string? Brand { get; set; }
    [Column, NotNull] public string Category { get; set; } = "";
    [Column] public string? Tags { get; set; }
    [Column] public string? Barcode { get; set; }

    [Column] public decimal PriceDkk { get; set; }
    [Column] public decimal? DiscountPercent { get; set; }
    [Column] [Range(1, Int32.MaxValue)] public int StockCount { get; set; }
    [Column] public int TimesPurchased { get; set; }
    [Column] public double WeightKg { get; set; }
    [Column] public double? RatingAvg { get; set; }

    [Column] public bool IsOrganic { get; set; }
    [Column] public bool IsDiscontinued { get; set; }

    [Column] public StorageType Storage { get; set; }
    [Column] public Supplier SuppliedBy { get; set; }

    [Column] public DateTime CreatedAtUtc { get; set; }
    [Column] public DateTime? LastPurchasedAtUtc { get; set; }
    [Column] public DateOnly? BestBefore { get; set; }
    [Column(DataType = DataType.Int64)] public TimeSpan? PreparationTime { get; set; }
}
