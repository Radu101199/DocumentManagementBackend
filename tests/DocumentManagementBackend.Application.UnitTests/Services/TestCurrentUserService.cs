using DocumentManagementBackend.Application.Common.Interfaces;

namespace DocumentManagementBackend.API.IntegrationTests;

public class TestCurrentUserService : ICurrentUserService
{
    private readonly string? _userId;

    public TestCurrentUserService(string? userId = null)
    {
        _userId = userId;
    }

    public string? UserId => _userId;
}