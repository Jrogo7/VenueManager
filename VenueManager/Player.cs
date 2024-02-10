using System;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace VenueManager
{
    [Serializable]
    public class Player
    {
        public string Name { get; set; } = "";
        public uint homeWorld { get; set; } = 0;
        public bool inHouse { get; set; } = false;
        public bool isFriend { get; set; } = false; 
        public uint ObjectId { get; set; } = 0;
        public DateTime firstSeen;
        public DateTime lastSeen;
        public DateTime latestEntry;
        public DateTime timeCursor;
        public double milisecondsInVenue { get; set; } = 0;
        public int entryCount { get; set; } = 0;
        public string WorldName => Plugin.DataManager.GetExcelSheet<Lumina.Excel.GeneratedSheets.World>()?.GetRow(homeWorld)?.Name?.RawString ?? $"World_{homeWorld}";

        public Player() {}

        public static Player fromCharacter(PlayerCharacter character) {
          Player player = new Player();
          player.Name = character.Name.TextValue;
          player.homeWorld = character.HomeWorld.Id;
          player.inHouse = true;
          player.isFriend = character.StatusFlags.HasFlag(StatusFlags.Friend);
          player.ObjectId = character.ObjectId;
          player.entryCount = 1;
          player.firstSeen = DateTime.Now;
          player.lastSeen = DateTime.Now;
          player.latestEntry = DateTime.Now;
          player.timeCursor = DateTime.Now;
          return player;
        }

        public string getCSVString(bool isCurrentHouse) {
          return Name + "," + WorldName + "," + inHouse + 
            "," + latestEntry.ToString("MM/dd/yyyy hh:mm tt") + 
            "," + firstSeen.ToString("MM/dd/yyyy hh:mm tt") + 
            "," + lastSeen.ToString("MM/dd/yyyy hh:mm tt") + 
            "," + (milisecondsInVenue / 1000) +
            "," + entryCount;
        }

        public void onLeaveVenue() {
          inHouse = false;
        }

        public void onAccumulateTime() {
          DateTime now = DateTime.Now;
          TimeSpan timeDiff = DateTime.Now  - timeCursor;
          milisecondsInVenue += timeDiff.TotalMilliseconds;
          timeCursor = now;

        }

        public string getTimeInVenue(bool isCurrentHouse) {
          double secondsInVenue = milisecondsInVenue / 1000;

          // Convert to semi human readable format 
          int minutes = (int)(secondsInVenue) / 60;
          int second = (int)(secondsInVenue) % 60;
          if (second < 10) 
            return minutes + ":0" + second;
          return minutes + ":" + second;
        }
   }
}
