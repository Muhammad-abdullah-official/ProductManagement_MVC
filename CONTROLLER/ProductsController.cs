using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.DTOs.PAGINATION;
using ProductManagement.DTOs.PRODUCT_DTOs;
using ProductManagement.Models;
using ProductManagement.Services;
using System.Security.Claims;

namespace ProductManagement.CONTROLLER
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]                         // all endpoints need JWT by default
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        // ── Helper: pull userId from JWT claims ──
        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private string CurrentUserRole =>
            User.FindFirstValue(ClaimTypes.Role) ?? "User";

        // GET /api/products?page=1&pageSize=10&search=phone
        [HttpGet]
        [AllowAnonymous]                // public endpoint — no login needed
        public async Task<IActionResult> GetAll([FromQuery] PaginationParams pagination)
        {
            var result = await _productService.GetAllAsync(pagination);
            return StatusCode(result.StatusCode, result);
        }

        // GET /api/products/{id}
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _productService.GetByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        // POST /api/products
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            var result = await _productService.CreateAsync(request, CurrentUserId);
            return StatusCode(result.StatusCode, result);
        }

        // PUT /api/products/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
        {
            var result = await _productService.UpdateAsync(id, request, CurrentUserId, CurrentUserRole);
            return StatusCode(result.StatusCode, result);
        }

        // DELETE /api/products/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _productService.DeleteAsync(id, CurrentUserId, CurrentUserRole);
            return StatusCode(result.StatusCode, result);
        }

        // GET /api/products/admin/all  — Admin only
        [HttpGet("admin/all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminGetAll([FromQuery] PaginationParams pagination)
        {
            var result = await _productService.GetAllAsync(pagination);
            return StatusCode(result.StatusCode, result);
        }
    }
}
