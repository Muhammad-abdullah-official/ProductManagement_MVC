using System.ComponentModel.DataAnnotations;

namespace ProductManagement.DTOs.PRODUCT_DTOs
{
    public record CreateProductRequest
        (
         [Required, MinLength(2)] string Name,
         string Description,
         [Range(0.01, double.MaxValue)] decimal Price,
         [Range(0, int.MaxValue)] int Stock
        );
}
