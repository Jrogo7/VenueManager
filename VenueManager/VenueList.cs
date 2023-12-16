using System;
using System.Collections.Generic;

namespace VenueManager
{
  [Serializable]
  public class VenueList
  {
    private static readonly string OutputFile = "venus.json";
  
    public Dictionary<long, Venue> venues { get; set; } = new();

    public VenueList()
    {
    }

    public void save() {
      FileStore.SaveFile(OutputFile, this.GetType(), this);
    }

    public void load() {
      VenueList loadedData = FileStore.LoadFile<VenueList>(OutputFile, this);
      this.venues = loadedData.venues;
    }
  }
}
