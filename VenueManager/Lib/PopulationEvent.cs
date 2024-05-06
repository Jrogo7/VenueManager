using System;

namespace VenueManager
{
    [Serializable]
    public class PopulationEvent
    {
      public long houseId {get; set;} = 0;
      public int playerCount {get; set;} = 0;
      public DateTime time {get; set;} = DateTime.Now;
      

      public PopulationEvent()
      {
      }

      public PopulationEvent(long houseId, int playerCount)
      {
        this.houseId = houseId;
        this.playerCount = playerCount;
      }
   }
}
