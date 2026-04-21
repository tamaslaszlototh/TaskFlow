using ErrorOr;
using MediatR;

namespace TaskFlow.Application.Authentication.Register;

public record RegisterCommand(string Email, string Password, string UserName)
    : IRequest<ErrorOr<RegisterCommandResult>>;