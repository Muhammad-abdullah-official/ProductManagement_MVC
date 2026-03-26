namespace ProductManagement.DTOs.PRODUCT_DTOs
{
    public record UpdateProductRequest
        (
         string? Name,
         string? Description,
         decimal? Price,
         int? Stock
         );
}
