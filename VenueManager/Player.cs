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
        public double secondsInVenue { get; set; } = 0;
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
          // If the current user is creating this CSV from inside the current house and 
          // this player is also in the house. Add any extra seconds that may still not 
          // be calculated on the total. 
          TimeSpan timeDiff = DateTime.Now - timeCursor;
          double extraSeconds = isCurrentHouse && inHouse ?  timeDiff.TotalSeconds : 0;

          return Name + "," + WorldName + "," + inHouse + 
            "," + latestEntry.ToString("MM/dd/yyyy hh:mm tt") + 
            "," + firstSeen.ToString("MM/dd/yyyy hh:mm tt") + 
            "," + lastSeen.ToString("MM/dd/yyyy hh:mm tt") + 
            "," + (secondsInVenue + extraSeconds) +
            "," + entryCount;
        }

        public void onLeaveVenue() {
          // Only add up time if current user is in the house
          if (inHouse) {
            TimeSpan timeDiff = DateTime.Now  - timeCursor;
            secondsInVenue += timeDiff.TotalSeconds;
          }
          timeCursor = DateTime.Now;
          inHouse = false;
        }

        public string getTimeInVenue(bool isCurrentHouse) {
          // If the current user is creating this CSV from inside the current house and 
          // this player is also in the house. Add any extra seconds that may still not 
          // be calculated on the total. 
          TimeSpan timeDiff = DateTime.Now - timeCursor;
          double extraSeconds = isCurrentHouse && inHouse ?  timeDiff.TotalSeconds : 0;

          // Convert to semi human readable format 
          int minutes = (int)(secondsInVenue + extraSeconds) / 60;
          int second = (int)(secondsInVenue + extraSeconds) % 60;
          if (second < 10) 
            return minutes + ":0" + second;
          return minutes + ":" + second;
        }
   }
}
