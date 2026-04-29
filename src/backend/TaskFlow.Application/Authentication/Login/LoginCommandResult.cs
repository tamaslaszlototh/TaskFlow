namespace TaskFlow.Application.Authentication.Login;

public record LoginCommandResult(string Token, string RefreshToken, int ExpiresInMinutes);