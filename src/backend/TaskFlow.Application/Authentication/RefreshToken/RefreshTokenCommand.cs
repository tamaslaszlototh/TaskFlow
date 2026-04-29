using ErrorOr;
using MediatR;
using TaskFlow.Application.Authentication.Login;

namespace TaskFlow.Application.Authentication.RefreshToken;

public record RefreshTokenCommand(string RefreshToken) : IRequest<ErrorOr<LoginCommandResult>>;
