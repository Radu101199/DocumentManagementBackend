namespace DocumentManagementBackend.Application.Common.Interfaces;

public interface ICacheService
{
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken cancellationToken = default);
}
