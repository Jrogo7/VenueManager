using Dalamud.Configuration;
using Dalamud.Plugin;
using System;

namespace VenueManager
{
  [Serializable]
  public class Configuration : IPluginConfiguration
  {
    public int Version { get; set; } = 0;

    // Should chat message alerts be printed to the chat
    public bool showChatAlerts { get; set; } = false;
    public bool showChatAlertEntry { get; set; } = true;
    public bool showChatAlertReentry { get; set; } = true;
    public bool showChatAlertLeave { get; set; } = true;
    public bool showChatAlertAlreadyHere { get; set; } = false;
    public bool showPluginNameInChat { get; set; } = false;

    // Should sound alerts be played when new players join the house 
    public bool soundAlerts { get; set; } = false;
    public float soundVolume { get; set; } = 1;
    // User selection for doorbell type
    public DOORBELL_TYPE doorbellType { get; set; } = DOORBELL_TYPE.DOORBELL;

    // Tab visibiliy options 
    public bool showGuestsTab { get; set; } = true;
    public bool showWebserviceLogging { get; set; } = false;
    public bool showStatsTab { get; set; } = false;
    public bool showVenueTab { get; set; } = true;

    public bool sortFriendsToTop { get; set; } = true;
    public bool sortCurrentVisitorsTop { get; set; } = true;

    // Advanced setting webserver config
    public WebserverConfig webserverConfig { get; set; } = new();

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private IDalamudPluginInterface? pluginInterface;

    public void Initialize(IDalamudPluginInterface pluginInterface)
    {
      this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
      this.pluginInterface!.SavePluginConfig(this);
    }
  }
}
