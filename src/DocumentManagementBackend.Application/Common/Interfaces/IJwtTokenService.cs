using DocumentManagementBackend.Domain.Entities;

namespace DocumentManagementBackend.Application.Common.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}