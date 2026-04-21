using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskFlow.Api.Contracts.Authentication;
using TaskFlow.Application.Authentication.Login;
using TaskFlow.Application.Authentication.Register;

namespace TaskFlow.Api.Controllers;

public class AuthenticationController : ApiController
{
    private readonly IMapper _mapper;
    private readonly IMediator _mediator;

    public AuthenticationController(IMapper mapper, IMediator mediator)
    {
        _mapper = mapper;
        _mediator = mediator;
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("api/v1/register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var command = _mapper.Map<RegisterCommand>(request);
        var result = await _mediator.Send(command);
        return result.Match(
            success => Created("api/v1/register", _mapper.Map<AuthenticationResponse>(success)),
            Problem);
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("api/v1/login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = _mapper.Map<LoginCommand>(request);
        var result = await _mediator.Send(command);
        return result.Match(
            success => Ok(_mapper.Map<AuthenticationResponse>(success)), Problem);
    }

    [HttpPost]
    [Route("api/v1/refresh")]
    public Task<IActionResult> Refresh()
    {
        throw new NotImplementedException();
        //Todo: implement token refresh logic
    }
}