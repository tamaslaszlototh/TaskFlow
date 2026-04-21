namespace TaskFlow.Api.Contracts.Authentication;

public record RegisterRequest(string Email, string Password, string UserName);