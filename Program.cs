using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Identity.Client; // MSAL library

namespace TenantStorageUsage
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            var tenantId = "your-tenant-id";              // Replace with your Azure AD tenant ID
            var clientId = "your-client-id";              // Replace with your Azure AD app's client ID
            var clientSecret = "your-client-secret";      // Replace with your Azure AD app's client secret
            var scope = "https://admin.services.crm.dynamics.com/.default";
            var authority = $"https://login.microsoftonline.com/{tenantId}";

            // Authenticate and get token
            var token = await AcquireAccessTokenAsync(authority, clientId, clientSecret, scope);

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("Failed to obtain access token.");
                return;
            }

            Console.WriteLine("Access token acquired.");
            
            // Fetch tenant-level storage usage
            await FetchStorageUsageAsync(token);
        }

        private static async Task<string> AcquireAccessTokenAsync(string authority, string clientId, string clientSecret, string scope)
        {
            try
            {
                var clientApp = ConfidentialClientApplicationBuilder
                    .Create(clientId)
                    .WithClientSecret(clientSecret)
                    .WithAuthority(new Uri(authority))
                    .Build();

                var result = await clientApp.AcquireTokenForClient(new[] { scope }).ExecuteAsync();
                return result.AccessToken;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error acquiring token: " + ex.Message);
                return null;
            }
        }

        private static async Task FetchStorageUsageAsync(string accessToken)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                var requestUrl = "https://admin.services.crm.dynamics.com/api/v1.0/Storage/UsageDetails"; // Replace with the actual endpoint
                try
                {
                    var response = await client.GetAsync(requestUrl);

                    if (response.IsSuccessStatusCode)
                    {
                        var data = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Storage Usage Data:");
                        Console.WriteLine(data);
                    }
                    else
                    {
                        Console.WriteLine("Failed to fetch storage usage. Status Code: " + response.StatusCode);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error fetching storage usage: " + ex.Message);
                }
            }
        }
    }
}