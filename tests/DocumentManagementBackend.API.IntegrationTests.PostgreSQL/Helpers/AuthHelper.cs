using System.Net.Http.Json;

namespace DocumentManagementBackend.API.IntegrationTests.PostgreSQL.Helpers;

public static class AuthHelper
{
    public static async Task<string> GetTokenAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        return result!.Token;
    }

    private record LoginResult(string Token, string Email, string FullName, List<string> Roles);
}
