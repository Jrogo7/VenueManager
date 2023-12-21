using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VenueManager
{
  public class RestUtils
  {
    private static string CurrentUrl = "";
    private static HttpClient Client = new();

    public static int successfulRequests = 0;
    public static int failedRequests = 0;
    public static readonly int maxFailedRequests = 5;

    public static async Task PostAsync(string url, string content, Plugin plugin)
    {
      if (url != CurrentUrl) {
        Client = new HttpClient()
        {
          BaseAddress = new Uri(url)
        };

        Client.DefaultRequestHeaders.Add("FF-UserName", plugin.pluginState.playerName);

        CurrentUrl = url;
      }

      // Build content payload 
      StringContent stringContent = new(content, Encoding.UTF8, "application/json");

      try
      {
        // Post message 
        using HttpResponseMessage response = await Client.PostAsync("", stringContent);
        response.EnsureSuccessStatusCode();
        successfulRequests++;
      }
      catch
      {
        failedRequests++;
        Plugin.Log.Warning("Failed to post to " + url);
      }
    }
  }
}
