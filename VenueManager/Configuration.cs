using Dalamud.Configuration;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;

namespace VenueManager
{

  [Serializable]
  public class Configuration : IPluginConfiguration
  {
    public int Version { get; set; } = 0;
    // Should chat message alerts be printed to the chat
    public bool showChatAlerts { get; set; } = false;
    public bool showPluginNameInChat { get; set; } = false;
    // Should sound alerts be played when new players join the house 
    public bool soundAlerts { get; set; } = false;
    public float soundVolume { get; set; } = 1;
    // Territory that the current user is in
    public ushort territory { get; set; } = 0;

    // the below exist just to make saving less cumbersome
    [NonSerialized]
    private DalamudPluginInterface? pluginInterface;

    public void Initialize(DalamudPluginInterface pluginInterface)
    {
      this.pluginInterface = pluginInterface;
    }

    public void Save()
    {
      this.pluginInterface!.SavePluginConfig(this);
    }
  }
}
