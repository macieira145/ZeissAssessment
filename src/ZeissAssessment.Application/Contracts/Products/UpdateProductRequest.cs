using System.ComponentModel.DataAnnotations;

namespace ZeissAssessment.Application.Contracts.Products;

public class UpdateProductRequest
{
    [Required]
    [StringLength(200, MinimumLength = 5)]
    public required string Name { get; init; }

    [Required]
    [StringLength(2000, MinimumLength = 1)]
    public required string Description { get; init; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Price must be greater than 0.")]
    public required decimal Price { get; init; }
}