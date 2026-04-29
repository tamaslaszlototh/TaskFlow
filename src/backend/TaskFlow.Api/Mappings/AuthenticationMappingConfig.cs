using Mapster;
using TaskFlow.Api.Contracts.Authentication;
using TaskFlow.Application.Authentication.Login;
using TaskFlow.Application.Authentication.RefreshToken;
using TaskFlow.Application.Authentication.Register;

namespace TaskFlow.Api.Mappings;

public class AuthenticationMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RegisterRequest, RegisterCommand>();
        config.NewConfig<RegisterCommandResult, AuthenticationResponse>();
        config.NewConfig<LoginRequest, LoginCommand>();
        config.NewConfig<LoginCommandResult, AuthenticationResponse>();
        config.NewConfig<RefreshTokenRequest, RefreshTokenCommand>();
    }
}