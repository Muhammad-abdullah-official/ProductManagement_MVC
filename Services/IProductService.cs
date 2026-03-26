using ProductManagement.DTOs.PAGINATION;
using ProductManagement.DTOs.PRODUCT_DTOs;
using ProductManagement.RESPONSES;

namespace ProductManagement.Services
{
    public interface IProductService
    {
        Task<ApiResponse<PagedResult<ProductDto>>> GetAllAsync(PaginationParams p);
        Task<ApiResponse<ProductDto>> GetByIdAsync(Guid id);
        Task<ApiResponse<ProductDto>> CreateAsync(CreateProductRequest req, Guid userId);
        Task<ApiResponse<ProductDto>> UpdateAsync(Guid id, UpdateProductRequest req, Guid userId, string role);
        Task<ApiResponse<bool>> DeleteAsync(Guid id, Guid userId, string role);
    }
}
