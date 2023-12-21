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
    public static bool headersChanged = true;
    public static DateTime lastTimeSent = new();
    public static bool lastTimeSentSet = false;

    public static async Task PostAsync(string url, string content, Plugin plugin)
    {
      if (url != CurrentUrl || headersChanged) {
        Client = new HttpClient()
        {
          BaseAddress = new Uri(url)
        };
        // Add default player name header
        Client.DefaultRequestHeaders.Add("FF-UserName", plugin.pluginState.playerName);

        // Add all client defined headers 
        foreach (var header in plugin.Configuration.webserverConfig.headers) {
          if (header.key.Length > 0)
            Client.DefaultRequestHeaders.Add(header.key, header.value);
        }
        headersChanged = false;
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
        lastTimeSent = DateTime.Now;
        lastTimeSentSet = true;
      }
      catch
      {
        failedRequests++;
        Plugin.Log.Warning("Failed to post to " + url);
      }
    }
  }
}
