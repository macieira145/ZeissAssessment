using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace ZeissAssessment.Application.Contracts.Products;

public class StockLevelProductsRequest : IValidatableObject
{
    [Range(0, int.MaxValue, ErrorMessage = "MinStock must be 0 or greater.")]
    public int? MinStock { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "MaxStock must be 0 or greater.")]
    public int? MaxStock { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinStock.HasValue && MaxStock.HasValue && MinStock >= MaxStock)
        {
            yield return new ValidationResult(
                "MinStock must be lower than MaxStock.",
                new[] { nameof(MinStock) });
        }
        
        if (MinStock.HasValue && MaxStock.HasValue && MinStock >= MaxStock)
        {
            yield return new ValidationResult(
                "MaxStock must be higher than MinStock.",
                new[] { nameof(MaxStock) });
        }
    }
}