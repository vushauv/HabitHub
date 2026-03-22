using backend.Dtos;
using backend.Models;
using backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserRepository users) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var user = await users.GetByIdAsync(id);
        if (user is null) return NotFound();
        return Ok(new UserResponse(user.Id, user.Email, user.CreatedAt));
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email,
            PasswordHash = request.Password, // TODO: hash before storing
            CreatedAt = DateTime.UtcNow
        };

        var created = await users.CreateAsync(user);
        return CreatedAtAction(nameof(Get), new { id = created.Id },
            new UserResponse(created.Id, created.Email, created.CreatedAt));
    }
}
