namespace TaskFlow.Api.Contracts.Authentication;

public record AuthenticationResponse(
    string Token,
    string RefreshToken,
    int ExpiresInMinutes);