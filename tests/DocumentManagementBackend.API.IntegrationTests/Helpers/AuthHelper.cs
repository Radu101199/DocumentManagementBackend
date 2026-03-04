using System.Net.Http.Json;

namespace DocumentManagementBackend.API.IntegrationTests.Helpers;

public static class AuthHelper
{
    public static async Task<string> GetTokenAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = email,
            Password = password
        });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result!.Token;
    }

    public static void AddAuthHeader(this HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    private record LoginResponse(string Token, string Email, string FullName, IEnumerable<string> Roles);
}