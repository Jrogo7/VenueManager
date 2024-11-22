using System;

namespace VenueManager
{
    [Serializable]
    public class Venue
    {
        public long houseId {get; set;} = 0;
        public int plot {get; set;} = 0;
        public int ward {get; set;} = 0;
        public int room {get; set;} = 0;
        public string name {get; set;} = "";
        public string district {get; set;} = "";
        public uint worldId {get; set;} = 0;
        public ushort type {get; set;} = 0;
        public string notes {get; set;} = "";
        public string WorldName => Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.World>()?.GetRow(worldId).Name.ToString() ?? $"World_{worldId}";
        public string DataCenter => Plugin.DataManager.GetExcelSheet<Lumina.Excel.Sheets.World>()?.GetRow(worldId).DataCenter.Value.Name.ToString() ?? "";

        public Venue()
        {
        }

        public Venue(Venue club) {
          houseId = club.houseId;
          plot = club.plot;
          ward = club.ward;
          room = club.room;
          name = club.name;
          district = club.district;
          worldId = club.worldId;
          type = club.type;
          notes = club.notes;
        }

        public string getVenueAddress() {
          string address = DataCenter + " | " + WorldName + " | " + district + " | W" + ward;
          if (TerritoryUtils.isPlotType(type)) {
            address += " | P" + plot;
            if (TerritoryUtils.isChamber(type)) {
              address += " | Chamber" + room;
            }
          } else {
            address += " | Room" + room;
          }
          
          return address;
        }
   }
}
