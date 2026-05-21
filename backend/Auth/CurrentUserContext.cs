namespace backend.Auth;
using backend.Enums;

public record CurrentUserContext
(
    Guid UserId,
    UserType UserType,
    string SessionId
);