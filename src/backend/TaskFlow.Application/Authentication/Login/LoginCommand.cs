using ErrorOr;
using MediatR;

namespace TaskFlow.Application.Authentication.Login;

public record LoginCommand(string UserName, string Password) : IRequest<ErrorOr<LoginCommandResult>>;