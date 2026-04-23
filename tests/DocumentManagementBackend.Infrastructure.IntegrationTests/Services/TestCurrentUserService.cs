using DocumentManagementBackend.Application.Common.Interfaces;

public class TestCurrentUserService : ICurrentUserService
{
    private readonly string? _userId;

    public TestCurrentUserService(string? userId = null)
    {
        _userId = userId;
    }

    public string? UserId => _userId;
}
