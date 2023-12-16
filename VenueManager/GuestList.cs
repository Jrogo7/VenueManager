using System;
using System.Collections.Generic;

namespace VenueManager
{
  [Serializable]
  public class GuestList
  {
    private static readonly string OutputFile = "guests.json";
  
    // List of guests in the venue
    public Dictionary<string, Player> guests { get; set; } = new();

    public GuestList()
    {
    }

    public void save() {
      FileStore.SaveFile(OutputFile, this.GetType(), this);
    }

    public void load() {
      GuestList loadedData = FileStore.LoadFile<GuestList>(OutputFile, this);
      this.guests = loadedData.guests;
    }
  }
}
