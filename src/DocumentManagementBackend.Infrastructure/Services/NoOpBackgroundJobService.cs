using System.Linq.Expressions;
using DocumentManagementBackend.Application.Common.Interfaces;

namespace DocumentManagementBackend.Infrastructure.Services;

public class NoOpBackgroundJobService : IBackgroundJobService
{
    public string Enqueue<T>(Expression<Func<T, Task>> methodCall) => Guid.NewGuid().ToString();
    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay) => Guid.NewGuid().ToString();
    public void AddOrUpdateRecurring<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression) { }
}
