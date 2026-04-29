using FluentValidation;

namespace TaskFlow.Application.Authentication.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty().WithMessage("User name is required")
            .MinimumLength(4).WithMessage("User name must be at least 4 characters long")
            .MaximumLength(10).WithMessage("User name must be at most 20 characters long");
        
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters long")
            .MaximumLength(15).WithMessage("Password must be at most 15 characters long");
    }
}