namespace ProductManagement.DTOs.PRODUCT_DTOs
{
    public record ProductDto(
        Guid Id,
        string Name,
        string Description,
        decimal Price,
        int Stock,
        Guid UserId,
        string CreatedByName,
        DateTime CreatedAt
    );
}
