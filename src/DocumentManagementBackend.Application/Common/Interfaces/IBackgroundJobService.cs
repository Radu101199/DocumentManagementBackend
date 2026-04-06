using System.Linq.Expressions;

namespace DocumentManagementBackend.Application.Common.Interfaces;

public interface IBackgroundJobService
{
    string Enqueue<T>(Expression<Func<T, Task>> methodCall);
    string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay);
    void AddOrUpdateRecurring<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression);
}
