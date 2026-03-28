using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.DTOs.USER_DTOs;
using ProductManagement.GENERIC_REPOSITORY;
using ProductManagement.Models;
using ProductManagement.RESPONSES;
using System.Security.Claims;

namespace ProductManagement.CONTROLLER
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepo;

        public UsersController(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        private Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        private string CurrentUserRole =>
            User.FindFirstValue(ClaimTypes.Role) ?? "User";

        // GET /api/users/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            // Users can only see themselves; Admins can see anyone
            if (id != CurrentUserId && CurrentUserRole != "Admin")
                return StatusCode(403, ApiResponse<object>.Forbidden());

            var user = await _userRepo.GetByIdAsync(id);
            if (user is null) return NotFound(ApiResponse<object>.NotFound());

            var dto = new UserDto(user.Id, user.Email, user.FirstName,
                                   user.LastName, user.Role, user.CreatedAt);
            return Ok(ApiResponse<UserDto>.Ok(dto));
        }

        // PUT /api/users/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserRequest request)
        {
            if (id != CurrentUserId && CurrentUserRole != "Admin")
                return StatusCode(403, ApiResponse<object>.Forbidden());

            var user = await _userRepo.GetByIdAsync(id);
            if (user is null) return NotFound(ApiResponse<object>.NotFound());

            if (request.FirstName is not null) user.FirstName = request.FirstName;
            if (request.LastName is not null) user.LastName = request.LastName;
            
            await _userRepo.UpdateAsync(user);

            var dto = new UserDto(user.Id, user.Email, user.FirstName,
                                   user.LastName, user.Role, user.CreatedAt);
            return Ok(ApiResponse<UserDto>.Ok(dto, "Profile updated."));
        }

        // GET /api/users  — Admin only
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var paged = await _userRepo.GetPagedAsync(page, pageSize);
            var dtos = paged.Items.Select(u =>
                new UserDto(u.Id, u.Email, u.FirstName, u.LastName, u.Role, u.CreatedAt));

            var result = new PagedResult<UserDto>
            {
                Items = dtos.ToList(),
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            };

            return Ok(ApiResponse<PagedResult<UserDto>>.Ok(result));
        }

        // DELETE /api/users/{id}  — Admin only (soft delete)
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await _userRepo.GetByIdAsync(id);
            if (user is null) return NotFound(ApiResponse<object>.NotFound());

            await _userRepo.DeleteAsync(id);
            return Ok(ApiResponse<bool>.Ok(true, "User deleted."));
        }
    }
}
