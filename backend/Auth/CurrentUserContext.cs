using backend.Models;

namespace backend.Auth;
using backend.Enums;

public record CurrentUserContext
(
    Guid UserId,
    UserType UserType,
    User User,
    string SessionId
);