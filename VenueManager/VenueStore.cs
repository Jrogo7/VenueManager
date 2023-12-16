using System;
using System.Collections.Generic;
using System.IO;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Utility;
using Newtonsoft.Json;

namespace VenueManager
{
  [Serializable]
  public class VenueStore
  {
    private static readonly string OutputFile = "venus.json";
  
    public Dictionary<long, Venue> venues { get; set; } = new();

    public VenueStore()
    {
    }

    public void save() {
      FileStore.SaveFile(OutputFile, this.GetType(), this);
    }

    public void load() {
      VenueStore loadedData = FileStore.LoadFile<VenueStore>(OutputFile, this);
      this.venues = loadedData.venues;
    }
  }
}
