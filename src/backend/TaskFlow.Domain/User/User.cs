using Microsoft.AspNetCore.Identity;

namespace TaskFlow.Domain.User;

public sealed class User : IdentityUser
{
    public DateTime CreatedAt { get; set; }
}