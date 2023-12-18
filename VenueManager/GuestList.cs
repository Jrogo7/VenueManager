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
    public long houseId {get; set;} = 0;
    public string venueName {get; set;} = "";

    public GuestList()
    {
    }

    public GuestList(long id, string name)
    {
      this.houseId = id;
      this.venueName = name;
    }

    private string getFileName() {
      return houseId + "-" + OutputFile;
    }

    public void save() {
      FileStore.SaveFile(getFileName(), this.GetType(), this);
    }

    public void load() {
      // Don't attempt to load if there is no file 
      var fileInfo = FileStore.GetFileInfo(getFileName());
      if (!fileInfo.Exists) return;
      
      GuestList loadedData = FileStore.LoadFile<GuestList>(getFileName(), this);
      this.guests = loadedData.guests;
      this.houseId = loadedData.houseId;
      this.venueName = loadedData.venueName;
    }
  }
}
