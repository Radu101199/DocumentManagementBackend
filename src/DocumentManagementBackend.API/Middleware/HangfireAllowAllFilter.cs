using Hangfire.Dashboard;

public class HangfireAllowAllFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}