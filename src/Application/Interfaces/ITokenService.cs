using ProductApi.Application.DTOs;

namespace ProductApi.Application.Interfaces;

public interface ITokenService
{
    AuthResponseDto GenerateTokens(string username);
    AuthResponseDto RefreshTokens(string expiredAccessToken, string refreshToken);
}
