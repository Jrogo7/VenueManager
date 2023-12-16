using System;

namespace ClubManager
{
    [Serializable]
    public class Club
    {
        public long houseId {get; set;} = 0;
        public int plot {get; set;} = 0;
        public int ward {get; set;} = 0;
        public string name {get; set;} = "";
        public string district {get; set;} = "";
        public uint worldId {get; set;} = 0;
        public string WorldName => Plugin.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.World>()?.GetRow(worldId)?.Name?.RawString ?? $"World_{worldId}";

        public Club()
        {
        }

        public Club(Club club) {
          houseId = club.houseId;
          plot = club.plot;
          ward = club.ward;
          name = club.name;
          district = club.district;
          worldId = club.worldId;
        }
   }
}
