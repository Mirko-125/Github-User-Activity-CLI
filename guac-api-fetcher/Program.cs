using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace GUAC
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length < 1)
            {
                ShowUsage();
                return;
            }

            string username = args[0];
            string url = $"https://api.github.com/users/{username}/events";

            using HttpClient client = new HttpClient();

            client.DefaultRequestHeaders.UserAgent.ParseAdd("User agent header");

            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                if (!(response.IsSuccessStatusCode))
                {
                    Console.WriteLine($"Failed to fetch activity for user '{username}'. HTTP Status: {response.StatusCode}");
                    return;
                }

                string jsonResponse = await response.Content.ReadAsStringAsync();

                using JsonDocument doc = JsonDocument.Parse(jsonResponse);
                JsonElement root = doc.RootElement;

                foreach (JsonElement eventElement in root.EnumerateArray())
                {
                    string? eventType = eventElement.GetProperty("type").GetString();

                    switch (eventType)
                    {
                        case "PushEvent":
                            {
                                string? repoName = eventElement.GetProperty("repo").GetProperty("name").GetString();
                                int commitCount = eventElement.GetProperty("payload").GetProperty("size").GetInt32();
                                Console.WriteLine($"Pushed {commitCount} commit{(commitCount != 1 ? "s" : "")} to {repoName}");
                                break;
                            }
                        case "IssuesEvent":
                            {
                                string? action = eventElement.GetProperty("payload").GetProperty("action").GetString();
                                if (action == "opened")
                                {
                                    string? repoName = eventElement.GetProperty("repo").GetProperty("name").GetString();
                                    Console.WriteLine($"Opened a new issue in {repoName}");
                                }
                                break;
                            }
                        case "WatchEvent":
                            {
                                string? repoName = eventElement.GetProperty("repo").GetProperty("name").GetString();
                                Console.WriteLine($"Starred {repoName}");
                                break;
                            }
                        default:
                            {
                                string? repoName = eventElement.GetProperty("repo").GetProperty("name").GetString();
                                Console.WriteLine($"{eventType} event in {repoName}");
                                break;
                            }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An exception has occured: {ex}");
                return;
            }

            return;
        }

        static void ShowUsage()
        {
            Console.WriteLine("Usage: dotnet run <username>");
        }
    }
}