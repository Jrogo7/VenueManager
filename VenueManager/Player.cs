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

        public Player() {}

        public static Player fromCharacter(PlayerCharacter character) {
          Player player = new Player();
          player.Name = character.Name.TextValue;
          player.homeWorld = character.HomeWorld.Id;
          player.inHouse = true;
          player.ObjectId = character.ObjectId;
          player.entryCount = 1;
          player.firstSeen = DateTime.Now;
          player.lastSeen = DateTime.Now;
          player.latestEntry = DateTime.Now;
          return player;
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
