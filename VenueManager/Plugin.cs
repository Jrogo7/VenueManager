using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using VenueManager.Windows;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text;
using System.Diagnostics;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;
using System.Collections.Generic;

namespace VenueManager
{
  public sealed class Plugin : IDalamudPlugin
  {
    public string Name => "Venue Manager";
    private const string CommandName = "/venue";
    private const string CommandNameAlias = "/vm";
    private const string CommandNameAlias2 = "/club";
    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static IFramework Framework { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    // Game Objects 
    [PluginService] public static IObjectTable Objects { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;
    [PluginService] public static IChatGui Chat { get; private set; } = null!;

    private ICommandManager CommandManager { get; init; }
    public Configuration Configuration { get; init; }
    public PluginState pluginState { get; init; }
    public VenueList venueList { get; init; }
    public GuestList guestList { get; init; }

    // Windows 
    public WindowSystem WindowSystem = new("VenueManager");
    private MainWindow MainWindow { get; init; }
    private Stopwatch stopwatch = new();
    private DoorbellSound doorbell;

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] ICommandManager commandManager)
    {
      this.pluginState = new PluginState();
      this.venueList = new VenueList();
      this.venueList.load();
      this.guestList = new GuestList();
      this.guestList.load();

      PluginInterface = pluginInterface;
      this.CommandManager = commandManager;

      this.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
      this.Configuration.Initialize(PluginInterface);

      MainWindow = new MainWindow(this);

      WindowSystem.AddWindow(MainWindow);

      this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand) {
        HelpMessage = "Open venue manager interface to see guests list and manage venues"
      });
      this.CommandManager.AddHandler(CommandNameAlias, new CommandInfo(OnCommand) {
        HelpMessage = "Alias for /venue"
      });
      this.CommandManager.AddHandler(CommandNameAlias2, new CommandInfo(OnCommand) {
        HelpMessage = "Alias for /venue"
      });

      PluginInterface.UiBuilder.Draw += DrawUI;

      // Bind territory changed listener to client 
      ClientState.TerritoryChanged += OnTerritoryChanged;
      Framework.Update += OnFrameworkUpdate;

      // Load Sound 
      doorbell = new DoorbellSound(this, Configuration.doorbellType);
      doorbell.load();

      // Run territory change one time on boot to register current location 
      OnTerritoryChanged(ClientState.TerritoryType);
    }

    public void Dispose()
    {
      // Remove framework listener on close 
      Framework.Update -= OnFrameworkUpdate;
      // Remove territory change listener 
      ClientState.TerritoryChanged -= OnTerritoryChanged;

      // Dispose our sound file 
      doorbell.disposeFile();

      this.WindowSystem.RemoveAllWindows();

      MainWindow.Dispose();

      this.CommandManager.RemoveHandler(CommandName);
      this.CommandManager.RemoveHandler(CommandNameAlias);
      this.CommandManager.RemoveHandler(CommandNameAlias2);
    }

    private void OnCommand(string command, string args)
    {
      // in response to the slash command, just display our main ui
      MainWindow.IsOpen = true;
    }

    private void DrawUI()
    {
      this.WindowSystem.Draw();
    }

    private void OnTerritoryChanged(ushort territory)
    {
      // Save current user territory 
      this.Configuration.territory = territory;

      if (TerritoryUtils.isHouse(territory))
      {
        // Log.Debug("User has entered a house");
        pluginState.userInHouse = true;
        stopwatch.Start();
      }
      else if (pluginState.userInHouse)
      {
        pluginState.userInHouse = false;
        pluginState.currentHouse = new Venue(); // Erase venue when leaving 
        stopwatch.Stop();
      }

      this.Configuration.Save();
    }


    private unsafe void OnFrameworkUpdate(IFramework framework)
    {
      // Every second we are in a house. Process players and see what has changed 
      if (pluginState.userInHouse && stopwatch.ElapsedMilliseconds > 1000)
      {
        // Fetch updated house information 
        try
        {
          var housingManager = HousingManager.Instance();
          var worldId = ClientState.LocalPlayer?.CurrentWorld.Id;
          // If the user has transitioned into a new house. Store that house information. Ensure we have a world to set it to 
          if (pluginState.currentHouse.houseId != housingManager->GetCurrentHouseId() && worldId != null)
          {
            pluginState.currentHouse.houseId = housingManager->GetCurrentHouseId();
            pluginState.currentHouse.plot = housingManager->GetCurrentPlot() + 1; // Game stores plot as -1 
            pluginState.currentHouse.ward = housingManager->GetCurrentWard() + 1; // Game stores ward as -1 
            pluginState.currentHouse.room = housingManager->GetCurrentRoom();
            pluginState.currentHouse.type = this.Configuration.territory;
            pluginState.currentHouse.worldId = worldId ?? 0;
            pluginState.currentHouse.district = TerritoryUtils.getHouseLocation(Configuration.territory);
          }
        }
        catch
        {
          Log.Error("Failed to load housing information");
        }

        if (!Configuration.showGuestsTab) return;

        bool configUpdated = false;
        bool playerArrived = false;
        int playerCount = 0;

        // Object to track seen players 
        Dictionary<string, bool> seenPlayers = new();
        foreach (var o in Objects)
        {
          // Reject non player objects 
          if (o is not PlayerCharacter pc) continue;
          playerCount++;
          var player = Player.fromCharacter(pc);

          // Add player to seen map 
          seenPlayers.Add(o.Name.TextValue, true);

          // Is the new player the current user 
          var isSelf = ClientState.LocalPlayer?.Name.TextValue == o.Name.TextValue;

          // New Player has entered the house 
          if (!guestList.guests.ContainsKey(o.Name.TextValue))
          {
            configUpdated = true;
            guestList.guests.Add(player.Name, player);
            if (!isSelf) playerArrived = true;
            showGuestEnterChatAlert(guestList.guests[player.Name], isSelf);
          }
          // Mark the player as re-entering the venue 
          else if (!guestList.guests[player.Name].inHouse)
          {
            configUpdated = true;
            guestList.guests[player.Name].inHouse = true;
            guestList.guests[player.Name].entryCount++;
            showGuestEnterChatAlert(guestList.guests[player.Name], isSelf);
          }
        }

        // Check for guests that have left the house 
        foreach (var guest in guestList.guests)
        {
          if (!seenPlayers.ContainsKey(guest.Value.Name) && guest.Value.inHouse)
          {
            guest.Value.inHouse = false;
            configUpdated = true;
            showGuestLeaveChatAlert(guest.Value);
          }
        }

        // Only play doorbell sound once if there were multiple new people 
        if (Configuration.soundAlerts && playerArrived)
        {
          doorbell.play();
        }

        // Save number of players seen this update 
        pluginState.playersInHouse = playerCount;

        // Save config if we saw new players
        if (configUpdated) guestList.save();

        stopwatch.Restart();
      }

    }

    public void playDoorbell()
    {
      doorbell.play();
    }

    public void reloadDoorbell()
    {
      doorbell.setType(Configuration.doorbellType);
      doorbell.load();
    }

    private void showGuestEnterChatAlert(Player player, bool isSelf)
    {
      // Message Chat for player arriving 
      if (Configuration.showChatAlerts)
      {
        var messageBuilder = new SeStringBuilder();
        var knownVenue = venueList.venues.ContainsKey(pluginState.currentHouse.houseId);

        // Show text alert for self if the venue is known
        if (isSelf && knownVenue)
        {
          var venue = venueList.venues[pluginState.currentHouse.houseId];
          if (this.Configuration.showPluginNameInChat) messageBuilder.AddText($"[{Name}] ");
          messageBuilder.AddText("You have entered " + venue.name);
          var entry = new XivChatEntry() { Message = messageBuilder.Build() };
          Chat.Print(entry);
        }
        // Show text alert for guests
        else if (!isSelf)
        {
          if (this.Configuration.showPluginNameInChat) messageBuilder.AddText($"[{Name}] ");
          if (player.entryCount == 1)
            messageBuilder.AddUiForeground(060); // Green. `/xldata` -> UIColor in chat in game 
          else if (player.entryCount == 2)
            messageBuilder.AddUiForeground(063);
          else if (player.entryCount == 3)
            messageBuilder.AddUiForeground(500);
          else if (player.entryCount >= 4)
            messageBuilder.AddUiForeground(518);
          messageBuilder.Add(new PlayerPayload(player.Name, player.HomeWorld));
          messageBuilder.AddText(" has entered");
          if (player.entryCount > 1)
            messageBuilder.AddText(" (" + player.entryCount + ")");
          if (knownVenue)
          {
            var venue = venueList.venues[pluginState.currentHouse.houseId];
            messageBuilder.AddText(" " + venue.name);
          }
          else
          {
            messageBuilder.AddText(" the " + TerritoryUtils.getHouseType(this.Configuration.territory));
          }

          messageBuilder.AddUiForegroundOff();
          var entry = new XivChatEntry() { Message = messageBuilder.Build() };
          Chat.Print(entry);
        }
      }
    }

    private void showGuestLeaveChatAlert(Player player) {
      if (!Configuration.showChatAlerts) return;
      
      var isSelf = ClientState.LocalPlayer?.Name.TextValue == player.Name;
      if (isSelf) return;

      var messageBuilder = new SeStringBuilder();
      var knownVenue = venueList.venues.ContainsKey(pluginState.currentHouse.houseId);

      // Add plugin name 
      if (this.Configuration.showPluginNameInChat) messageBuilder.AddText($"[{Name}] ");

      // Add Player name 
      messageBuilder.Add(new PlayerPayload(player.Name, player.HomeWorld));
      messageBuilder.AddText(" has left");
      
      // Add Venue info 
      if (knownVenue)
      {
        var venue = venueList.venues[pluginState.currentHouse.houseId];
        messageBuilder.AddText(" " + venue.name);
      }
      else
      {
        messageBuilder.AddText(" the " + TerritoryUtils.getHouseType(this.Configuration.territory));
      }

      var entry = new XivChatEntry() { Message = messageBuilder.Build() };
      Chat.Print(entry);
    }
  }
}
