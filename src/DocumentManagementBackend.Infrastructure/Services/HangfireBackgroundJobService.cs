using System.Linq.Expressions;
using DocumentManagementBackend.Application.Common.Interfaces;
using Hangfire;

namespace DocumentManagementBackend.Infrastructure.Services;

public class HangfireBackgroundJobService : IBackgroundJobService
{
    private readonly IBackgroundJobClient _client;
    private readonly IRecurringJobManager _recurringJobManager;

    public HangfireBackgroundJobService(
        IBackgroundJobClient client,
        IRecurringJobManager recurringJobManager)
    {
        _client = client;
        _recurringJobManager = recurringJobManager;
    }

    public string Enqueue<T>(Expression<Func<T, Task>> methodCall)
        => _client.Enqueue(methodCall);

    public string Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan delay)
        => _client.Schedule(methodCall, delay);

    public void AddOrUpdateRecurring<T>(string jobId, Expression<Func<T, Task>> methodCall, string cronExpression)
        => _recurringJobManager.AddOrUpdate(jobId, methodCall, cronExpression);
}
