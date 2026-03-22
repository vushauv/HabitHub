using backend.Controllers;
using backend.Dtos;
using backend.Models;
using backend.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace backend.Tests;

public class UserControllerTests
{
    private readonly FakeUserRepository _repo = new();
    private readonly UserController _controller;

    public UserControllerTests()
    {
        _controller = new UserController(_repo);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenUserDoesNotExist()
    {
        var result = await _controller.Get(Guid.NewGuid());

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Get_ReturnsUser_WhenUserExists()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@example.com",
            PasswordHash = "hash",
            CreatedAt = DateTime.UtcNow
        };
        _repo.Seed(user);

        var result = await _controller.Get(user.Id);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<UserResponse>(ok.Value);
        Assert.Equal(user.Email, response.Email);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction()
    {
        var request = new CreateUserRequest("new@example.com", "password123");

        var result = await _controller.Create(request);

        var created = Assert.IsType<CreatedAtActionResult>(result);
        var response = Assert.IsType<UserResponse>(created.Value);
        Assert.Equal("new@example.com", response.Email);
    }

    private class FakeUserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, User> _users = new();

        public void Seed(User user) => _users[user.Id] = user;

        public Task<User?> GetByIdAsync(Guid id) =>
            Task.FromResult(_users.GetValueOrDefault(id));

        public Task<User> CreateAsync(User user)
        {
            _users[user.Id] = user;
            return Task.FromResult(user);
        }
    }
}
