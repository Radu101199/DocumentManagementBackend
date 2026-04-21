using DocumentManagementBackend.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace DocumentManagementBackend.Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    public string? UserId { get; }

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        UserId = httpContextAccessor.HttpContext?.User
            .FindFirstValue(ClaimTypes.NameIdentifier);
    }
}