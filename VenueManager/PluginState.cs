namespace VenueManager
{
    public class PluginState 
    {
        // If the player is in a house, these values will be filled with the current house information
        public Venue currentHouse = new();
        // Is the current user in the house
        public bool userInHouse { get; set; } = false;
        // Count of players in a the house 
        public int playersInHouse { get; set; } = 0;

        public PluginState()
        {
        }
    }
}
