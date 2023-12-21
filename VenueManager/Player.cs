using System;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace VenueManager
{
    [Serializable]
    public class Player
    {
        public string Name { get; set; } = "";
        public uint homeWorld { get; set; } = 0;
        public bool inHouse { get; set; } = false;
        public uint ObjectId { get; set; } = 0;
        public DateTime firstSeen;
        public DateTime lastSeen;
        public DateTime latestEntry;
        public int entryCount { get; set; } = 0;
        public string WorldName => Plugin.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.World>()?.GetRow(homeWorld)?.Name?.RawString ?? $"World_{homeWorld}";

        public Player(string name, uint homeWorld, bool inHouse, uint objectId, DateTime firstSeen, DateTime lastSeen, DateTime latestEntry, int entryCount)
        {
            this.Name = name;
            this.homeWorld = homeWorld;
            this.inHouse = inHouse;
            this.ObjectId = objectId;
            this.firstSeen = firstSeen;
            this.lastSeen = firstSeen;
            this.latestEntry = latestEntry;
            this.entryCount = entryCount;
        }

        public static Player fromCharacter(PlayerCharacter character) {
          return new Player(character.Name.TextValue, character.HomeWorld.Id, true, character.ObjectId, DateTime.Now, DateTime.Now, DateTime.Now, 1);
        }

        public string getCSVString() {
          return Name + "," + WorldName + "," + inHouse + 
            "," + latestEntry.ToString("MM/dd/yyyy hh:mm tt") + 
            "," + firstSeen.ToString("MM/dd/yyyy hh:mm tt") + 
            "," + lastSeen.ToString("MM/dd/yyyy hh:mm tt") + 
            "," + entryCount;
        }
   }
}
