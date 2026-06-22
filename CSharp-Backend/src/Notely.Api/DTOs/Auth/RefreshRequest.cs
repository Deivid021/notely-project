using System.ComponentModel.DataAnnotations;

namespace Notely.Api.DTOs.Auth;

public record RefreshRequest([Required] string RefreshToken);
