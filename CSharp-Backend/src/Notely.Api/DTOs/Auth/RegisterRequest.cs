using System.ComponentModel.DataAnnotations;

namespace Notely.Api.DTOs.Auth;

public record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password
);
