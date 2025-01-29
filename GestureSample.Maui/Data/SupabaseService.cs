using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestureSample.Maui.Data
{
    using System.Net.Http;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Text.Json;
    using System.Text;

    public static class SupabaseService
    {
        private static readonly string supabaseUrl = "https://your-supabase-url.supabase.co";
        private static readonly string apiKey = "your-supabase-api-key";

        public static async Task SyncUserAsync(User user)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("apikey", apiKey);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

                var getResponse = await client.GetAsync($"{supabaseUrl}/rest/v1/users?id=eq.{user.Id}");
                var existingUsers = JsonSerializer.Deserialize<List<User>>(await getResponse.Content.ReadAsStringAsync());

                if (existingUsers.Count == 0)
                {
                    var newUserContent = new StringContent(JsonSerializer.Serialize(user), Encoding.UTF8, "application/json");
                    await client.PostAsync($"{supabaseUrl}/rest/v1/users", newUserContent);
                }
                else
                {
                    var existingUser = existingUsers[0];
                    if (existingUser.LastLoginTime != user.LastLoginTime)
                    {
                        var updateContent = new StringContent(JsonSerializer.Serialize(new { last_login_time = user.LastLoginTime }), Encoding.UTF8, "application/json");
                        await client.PatchAsync($"{supabaseUrl}/rest/v1/users?id=eq.{user.Id}", updateContent);
                    }
                }

                await SyncUserGames(user);
            }
        }

        private static async Task SyncUserGames(User user)
        {
            Console.WriteLine($"Syncing games for user {user.Name}");
        }
    }
}
