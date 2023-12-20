using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VenueManager
{
  public class RestUtils
  {
    public static async Task PostAsync(string url, string content)
    {
      // Main Http Client
      HttpClient client = new HttpClient()
      {
        BaseAddress = new Uri(url)
      };

      // Build content payload 
      StringContent stringContent = new(content, Encoding.UTF8, "application/json");

      try
      {
        // Post message 
        using HttpResponseMessage response = await client.PostAsync("", stringContent);

        response.EnsureSuccessStatusCode();

        var jsonResponse = await response.Content.ReadAsStringAsync();
        Plugin.Log.Info($"{jsonResponse}\n");
      }
      catch
      {
        Plugin.Log.Warning("Failed to post to " + url);
      }
    }
  }
}
