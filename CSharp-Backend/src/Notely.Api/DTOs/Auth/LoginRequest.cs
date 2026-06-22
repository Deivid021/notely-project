using System.ComponentModel.DataAnnotations;

namespace Notely.Api.DTOs.Auth;

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);
