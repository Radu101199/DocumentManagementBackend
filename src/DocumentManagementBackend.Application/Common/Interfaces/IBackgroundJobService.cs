namespace DocumentManagementBackend.Application.Common.Interfaces;

public interface IBackgroundJobService
{
    // Fire-and-Forget — rulează imediat în background
    string Enqueue<T>(System.Linq.Expressions.Expression<Action<T>> methodCall);
    string Enqueue<T>(System.Linq.Expressions.Expression<Func<T, Task>> methodCall);

    // Delayed — rulează după un delay
    string Schedule<T>(System.Linq.Expressions.Expression<Action<T>> methodCall, TimeSpan delay);
    string Schedule<T>(System.Linq.Expressions.Expression<Func<T, Task>> methodCall, TimeSpan delay);

    // Recurring — rulează la intervale fixe (cron)
    void AddOrUpdateRecurring<T>(
        string jobId,
        System.Linq.Expressions.Expression<Action<T>> methodCall,
        string cronExpression);
}