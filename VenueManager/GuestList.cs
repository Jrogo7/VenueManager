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
    public Venue venue { get; set; } = new();
    public DateTime startTime { get; set; } = DateTime.Now;
    public bool outsideEvent { get; set; } = false;

    public GuestList()
    {
    }

    public GuestList(long id, Venue venue)
    {
      this.houseId = id;
      this.venue = new Venue(venue);
    }

    private string getFileName()
    {
      return houseId + "-" + OutputFile;
    }

    public void save()
    {
      // Save not supported for default guest list
      if (this.houseId == 0 || this.houseId == 1) return;

      FileStore.SaveClassToFileInPluginDir(getFileName(), this.GetType(), this);
    }

    public void load()
    {
      // Load not supported for default guest list
      if (this.houseId == 0 || this.houseId == 1) return;

      // Don't attempt to load if there is no file 
      var fileInfo = FileStore.GetFileInfo(getFileName());
      if (!fileInfo.Exists) return;

      GuestList loadedData = FileStore.LoadFile<GuestList>(getFileName(), this);
      this.guests = loadedData.guests;
      this.houseId = loadedData.houseId;
      this.startTime = loadedData.startTime;
      // Don't replace venue if the incoming one is blank
      if (loadedData.venue.name.Length != 0)
        this.venue = loadedData.venue;
    }

    public void saveToFile(string path)
    {
      FileStore.SaveClassToFile(path, this.GetType(), this);
    }

    public void saveToFileCSV(string path)
    {
      string csv = "Name,World,Is Inside,Latest Entry,First Seen,Last Seen,Entry Count\n";
      foreach (var guest in guests) {
        csv += guest.Value.getCSVString() + "\n";
      }
      FileStore.SaveStringToFile(path, csv);
    }

    public void sentToWebserver(Plugin plugin) {
      // Cant send payload if we do not have a url 
      if (plugin.Configuration.webserverConfig.endpoint.Length == 0) return;
      // Don't send data for default house 
      if (this.houseId == 0) return;

      // Ensure no player created notes are sent to the server
      this.venue.notes = "";

      // Convert class to string
      string output = JsonConvert.SerializeObject(this, this.GetType(), new JsonSerializerSettings { Formatting = Formatting.Indented });

      // Post data to the webserver
      _ = RestUtils.PostAsync(plugin.Configuration.webserverConfig.endpoint, output, plugin);
    }
  }
}
