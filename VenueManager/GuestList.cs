using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace VenueManager
{
  [Serializable]
  public class GuestList
  {
    private static readonly string OutputFile = "guests.json";

    // List of guests in the venue
    public Dictionary<string, Player> guests { get; set; } = new();
    public long houseId { get; set; } = 0;
    public string venueName { get; set; } = "";

    public GuestList()
    {
    }

    public GuestList(long id, string name)
    {
      this.houseId = id;
      this.venueName = name;
    }

    private string getFileName()
    {
      return houseId + "-" + OutputFile;
    }

    public void save()
    {
      FileStore.SaveClassToFileInPluginDir(getFileName(), this.GetType(), this);
    }

    public void load()
    {
      // Don't attempt to load if there is no file 
      var fileInfo = FileStore.GetFileInfo(getFileName());
      if (!fileInfo.Exists) return;

      GuestList loadedData = FileStore.LoadFile<GuestList>(getFileName(), this);
      this.guests = loadedData.guests;
      this.houseId = loadedData.houseId;
      this.venueName = loadedData.venueName;
    }

    public void saveToFile(string path)
    {
      FileStore.SaveClassToFile(path, this.GetType(), this);
    }

    public void sentToWebserver(Plugin plugin) {
      // Cant send payload if we do not have a url 
      if (plugin.Configuration.webserverConfig.endpoint.Length == 0) return;

      // Convert class to string
      string output = JsonConvert.SerializeObject(this, this.GetType(), new JsonSerializerSettings { Formatting = Formatting.Indented });

      // Post data to the webserver
      _ = RestUtils.PostAsync(plugin.Configuration.webserverConfig.endpoint, output);
    }
  }
}
