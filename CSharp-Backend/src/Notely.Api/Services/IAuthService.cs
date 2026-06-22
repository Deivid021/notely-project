using Notely.Api.DTOs.Auth;

namespace Notely.Api.Services;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest req);
    Task<AuthResponse?> LoginAsync(LoginRequest req);
    Task<AuthResponse?> RefreshAsync(string token);
    Task RevokeAsync(string token);
}
