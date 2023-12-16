using System;
using Dalamud.Game.ClientState.Objects.SubKinds;

namespace ClubManager
{
    [Serializable]
    public class Player
    {
        public string Name { get; set; } = "";
        public uint HomeWorld { get; set; } = 0;
        public bool inHouse { get; set; } = false;
        public uint ObjectId { get; set; } = 0;
        public DateTime firstSeen;
        public int entryCount { get; set; } = 0;

        public Player(string name, uint homeWorld, bool inHouse, uint objectId, DateTime time, int entryCount)
        {
            this.Name = name;
            this.HomeWorld = homeWorld;
            this.inHouse = inHouse;
            this.ObjectId = objectId;
            this.firstSeen = time;
            this.entryCount = entryCount;
        }

        public static Player fromCharacter(PlayerCharacter character) {
          return new Player(character.Name.TextValue, character.HomeWorld.Id, true, character.ObjectId, DateTime.Now, 1);
        }
   }
}
