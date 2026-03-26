using ProductManagement.DTOs.PAGINATION;
using ProductManagement.DTOs.PRODUCT_DTOs;
using ProductManagement.GENERIC_REPOSITORY;
using ProductManagement.Models;
using ProductManagement.RESPONSES;

namespace ProductManagement.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repo;
        private readonly ILogger<ProductService> _logger;

        public ProductService(IProductRepository repo, ILogger<ProductService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<ApiResponse<PagedResult<ProductDto>>> GetAllAsync(PaginationParams p)
        {
            // Build optional search filter
            var paged = await _repo.GetPagedAsync(
                p.Page, p.PageSize,
                filter: string.IsNullOrEmpty(p.Search)
                    ? null
                    : prod => prod.Name.Contains(p.Search) || prod.Description.Contains(p.Search)
            );

            var dtoResult = new PagedResult<ProductDto>
            {
                Items = paged.Items.Select(MapToDto).ToList(),
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            };

            return ApiResponse<PagedResult<ProductDto>>.Ok(dtoResult);
        }

        public async Task<ApiResponse<ProductDto>> GetByIdAsync(Guid id)
        {
            var product = await _repo.GetByIdAsync(id);
            return product is null
                ? ApiResponse<ProductDto>.NotFound()
                : ApiResponse<ProductDto>.Ok(MapToDto(product));
        }

        public async Task<ApiResponse<ProductDto>> CreateAsync(CreateProductRequest req, Guid userId)
        {
            var product = new Product
            {
                Name = req.Name,
                Description = req.Description,
                Price = req.Price,
                Stock = req.Stock,
                UserId = userId
            };

            await _repo.AddAsync(product);
            _logger.LogInformation("Product created: {Name} by user {UserId}", req.Name, userId);

            // Reload to get navigation property
            var created = await _repo.GetByIdAsync(product.Id);
            return ApiResponse<ProductDto>.Created(MapToDto(created!));
        }

        public async Task<ApiResponse<ProductDto>> UpdateAsync(
            Guid id, UpdateProductRequest req, Guid userId, string role)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product is null) return ApiResponse<ProductDto>.NotFound();

            // Business rule: only owner or admin can update
            if (product.UserId != userId && role != "Admin")
                return ApiResponse<ProductDto>.Forbidden("You don't own this product.");

            if (req.Name is not null) product.Name = req.Name;
            if (req.Description is not null) product.Description = req.Description;
            if (req.Price is not null) product.Price = req.Price.Value;
            if (req.Stock is not null) product.Stock = req.Stock.Value;

            await _repo.UpdateAsync(product);
            return ApiResponse<ProductDto>.Ok(MapToDto(product));
        }

        public async Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId, string role)
        {
            var product = await _repo.GetByIdAsync(id);
            if (product is null) return ApiResponse<bool>.NotFound();

            if (product.UserId != userId && role != "Admin")
                return ApiResponse<bool>.Forbidden();

            await _repo.DeleteAsync(id);
            return ApiResponse<bool>.Ok(true, "Product deleted.");
        }

        // ── Map Entity → DTO ─────────────────────
        private static ProductDto MapToDto(Product p) =>
            new(p.Id, p.Name, p.Description, p.Price, p.Stock,
                p.UserId, $"{p.User?.FirstName} ".Trim(),
                p.CreatedAt);
    }
}
