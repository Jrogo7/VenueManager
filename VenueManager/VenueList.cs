using System;
using System.Collections.Generic;
using System.IO;

namespace VenueManager
{
  [Serializable]
  public class VenueList
  {
    private static readonly string OutputFile = "venues.json";
  
    public Dictionary<long, Venue> venues { get; set; } = new();

    public VenueList()
    {
    }

    public void save() {
      FileStore.SaveClassToFileInPluginDir(OutputFile, this.GetType(), this);
    }

    public void load() {
      VenueList oldData = new();
      FileInfo oldDataFile = FileStore.GetFileInfo("venus.json");
      if (oldDataFile.Exists) {
        oldData = FileStore.LoadFile<VenueList>("venus.json", this); // Load any old venues if there were any 
        File.Delete(oldDataFile.FullName);
      }

      VenueList loadedData = FileStore.LoadFile<VenueList>(OutputFile, this);

      // Migrate old data 
      foreach (var venue in oldData.venues) {
        if (!loadedData.venues.ContainsKey(venue.Key)) {
          loadedData.venues.Add(venue.Key, venue.Value);
        }
      }

      // Load data into current instance 
      this.venues = loadedData.venues;

      // Save new file to ensure they are not lost 
      if (oldData.venues.Count > 0) {
        this.save();
      }
    }
  }
}
