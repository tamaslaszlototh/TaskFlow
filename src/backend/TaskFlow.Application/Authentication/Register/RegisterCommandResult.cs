namespace TaskFlow.Application.Authentication.Register;

public record RegisterCommandResult(string Token, string RefreshToken, int ExpiresInMinutes);