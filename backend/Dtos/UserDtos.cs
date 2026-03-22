namespace backend.Dtos;

public record CreateUserRequest(string Email, string Password);

public record UserResponse(Guid Id, string Email, DateTime CreatedAt);
