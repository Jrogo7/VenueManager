namespace ClubManager
{
    public class PluginState 
    {
        // If the player is in a house, these values will be filled with the current house information
        public Club currentHouse = new();
        // Is the current user in the house
        public bool userInHouse { get; set; } = false;

        public PluginState()
        {
        }
    }
}
