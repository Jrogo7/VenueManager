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
using System;
using VenueManager.UI;
using Dalamud.Game.ClientState.Objects.Enums;

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
    [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
    // Game Objects 
    [PluginService] public static IObjectTable Objects { get; private set; } = null!;
    [PluginService] public static IPluginLog Log { get; private set; } = null!;
    [PluginService] public static IChatGui Chat { get; private set; } = null!;

    private ICommandManager CommandManager { get; init; }
    public Configuration Configuration { get; init; }
    public PluginState pluginState { get; init; }
    public VenueList venueList { get; init; }
    public Dictionary<long, GuestList> guestLists = new();

    // Windows 
    public WindowSystem WindowSystem = new("VenueManager");
    private MainWindow MainWindow { get; init; }
    private NotesWindow NotesWindow { get; init; }

    private Stopwatch stopwatch = new();
    private Stopwatch webserviceStopwatch = new();
    private DoorbellSound doorbell;

    // True for the first loop that a player enters a house 
    private bool justEnteredHouse = false;

    private bool running = false;

    public Plugin(
        [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
        [RequiredVersion("1.0")] ICommandManager commandManager)
    {
      this.pluginState = new PluginState();
      this.venueList = new VenueList();
      this.venueList.load();

      // Default guest list 
      this.guestLists.Add(0, new GuestList());
      this.guestLists[0].load();
      // Create default fake outside event 
      this.guestLists.Add(1, GuestList.getOutdoorList());

      PluginInterface = pluginInterface;
      this.CommandManager = commandManager;

      this.Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
      this.Configuration.Initialize(PluginInterface);

      MainWindow = new MainWindow(this);
      NotesWindow = new NotesWindow(this);

      WindowSystem.AddWindow(MainWindow);
      WindowSystem.AddWindow(NotesWindow);

      this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand) { ShowInHelp = true, HelpMessage = "Open venue manager interface to see guests list and manage venues" });
      this.CommandManager.AddHandler(CommandNameAlias, new CommandInfo(OnCommand) { ShowInHelp = true, HelpMessage = "Alias for /venue" });
      this.CommandManager.AddHandler(CommandNameAlias2, new CommandInfo(OnCommand) { ShowInHelp = true, HelpMessage = "Alias for /venue" });
      var SnoozeHandler = new CommandInfo(OnCommand) { ShowInHelp = true, HelpMessage = "Pause alerts until leaving the house." };
      var SnoozeHandlerAlias = new CommandInfo(OnCommand) { ShowInHelp = true, HelpMessage = "Alias for /venue snooze" };
      CommandManager.AddHandler(CommandName + " snooze", SnoozeHandler);
      CommandManager.AddHandler(CommandNameAlias + " snooze", SnoozeHandlerAlias);
      CommandManager.AddHandler(CommandNameAlias2 + " snooze", SnoozeHandlerAlias);

      PluginInterface.UiBuilder.Draw += DrawUI;

      // Bind territory changed listener to client 
      ClientState.TerritoryChanged += OnTerritoryChanged;
      Framework.Update += OnFrameworkUpdate;
      ClientState.Logout += OnLogout;

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
      NotesWindow.Dispose();

      this.CommandManager.RemoveHandler(CommandName);
      this.CommandManager.RemoveHandler(CommandNameAlias);
      this.CommandManager.RemoveHandler(CommandNameAlias2);
      CommandManager.RemoveHandler(CommandName + " snooze");
      CommandManager.RemoveHandler(CommandNameAlias + " snooze");
      CommandManager.RemoveHandler(CommandNameAlias2 + " snooze");
    }

    private void OnSnooze()
    {
      if (pluginState.snoozed)
      {
        pluginState.snoozed = false;
        Chat.Print((this.Configuration.showPluginNameInChat ? $"[{Name}] " : "") + "Alerts unpaused");
      }
      else if (!pluginState.userInHouse)
      {
        Chat.Print((this.Configuration.showPluginNameInChat ? $"[{Name}] " : "") + "You must be in a house to pause alerts");
      }
      else
      {
        pluginState.snoozed = true;
        Chat.Print((this.Configuration.showPluginNameInChat ? $"[{Name}] " : "") + "Alerts paused until leaving the current house");
      }
    }

    private void OnCommand(string command, string args)
    {
      if (args == "snooze")
      {
        OnSnooze();
        return;
      }
      // in response to the slash command, just display our main ui
      MainWindow.IsOpen = true;
    }

    private void DrawUI()
    {
      this.WindowSystem.Draw();
    }

    public void ShowNotesWindow(Venue venue)
    {
      NotesWindow.venue = venue;
      NotesWindow.IsOpen = true;
    }

    private void OnLogout()
    {
      // Erase territory state 
      pluginState.territory = 0;

      leftHouse();
    }

    private void OnTerritoryChanged(ushort territory)
    {
      // Save current user territory 
      pluginState.territory = territory;

      // Reset tracking outside 
      pluginState.isTrackingOutside = false;
      // Clear outdoor events list 
      guestLists[1].guests = new();

      // Player has entered a house 
      if (TerritoryUtils.isHouse(territory))
      {
        justEnteredHouse = true;
        pluginState.userInHouse = true;
        startTimers();
      }
      // Player has left a house 
      else if (pluginState.userInHouse)
      {
        leftHouse();
      }

      this.Configuration.Save();
    }

    public void startTimers()
    {
      stopwatch.Start();
      webserviceStopwatch.Start();
    }

    public void stopTimers()
    {
      stopwatch.Stop();
      webserviceStopwatch.Stop();
    }

    private void leftHouse()
    {
      pluginState.userInHouse = false;
      pluginState.currentHouse = new Venue(); // Erase venue when leaving 
      stopwatch.Stop();
      webserviceStopwatch.Stop();
      // Unsnooze if leaving a house when snoozed 
      if (pluginState.snoozed) OnSnooze();
    }

    private unsafe void OnFrameworkUpdate(IFramework framework)
    {
      if (running) {
        Log.Warning("Skipping processing while already running.");
        return;
      }
      running = true;
      try
      {
        // Every second we are in a house or tracking outside event. Process players and see what has changed 
        if ((pluginState.userInHouse || pluginState.isTrackingOutside) && stopwatch.ElapsedMilliseconds > 1000)
        {
          // Fetch updated house information 
          if (pluginState.userInHouse)
          {
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
                pluginState.currentHouse.type = pluginState.territory;
                pluginState.currentHouse.worldId = worldId ?? 0;
                pluginState.currentHouse.district = TerritoryUtils.getHouseLocation(pluginState.territory);

                // Load current guest list from disk if player has entered a saved venue 
                if (venueList.venues.ContainsKey(pluginState.currentHouse.houseId))
                {
                  var venue = venueList.venues[pluginState.currentHouse.houseId];
                  GuestList venueGuestList = new GuestList(venue.houseId, venue);
                  venueGuestList.load();
                  guestLists.Add(venue.houseId, venueGuestList);
                }
              }
            }
            catch
            {
              // Typically fails first time after entering a house 
              running = false;
              return;
            }
          }

          if (!Configuration.showGuestsTab) {
            running = false;
            return;
          }

          bool guestListUpdated = false;
          bool playerArrived = false;
          int playerCount = 0;

          // Object to track seen players 
          Dictionary<string, bool> seenPlayers = new();
          foreach (var o in Objects)
          {
            // Reject non player objects 
            if (o is not PlayerCharacter pc) continue;
            var player = Player.fromCharacter(pc);

            // Skip player characters that do not have a name. 
            // Portrait and Adventure plates show up with this. 
            if (pc.Name.TextValue.Length == 0) continue;
            // Im not sure what this means, but it seems that 4 is for players
            if (o.SubKind != 4) continue;
            playerCount++;

            // Add player to seen map 
            if (seenPlayers.ContainsKey(player.Name))
              seenPlayers[player.Name] = true;
            else
              seenPlayers.Add(player.Name, true);

            // Is the new player the current user 
            var isSelf = ClientState.LocalPlayer?.Name.TextValue == player.Name;

            // Store Player name 
            if (ClientState.LocalPlayer?.Name.TextValue.Length > 0) pluginState.playerName = ClientState.LocalPlayer?.Name.TextValue ?? "";

            // New Player has entered the house 
            if (!getCurrentGuestList().guests.ContainsKey(player.Name))
            {
              guestListUpdated = true;
              getCurrentGuestList().guests.Add(player.Name, player);
              if (!isSelf) playerArrived = true;
              showGuestEnterChatAlert(getCurrentGuestList().guests[player.Name], isSelf);
            }
            // Mark the player as re-entering the venue 
            else if (!getCurrentGuestList().guests[player.Name].inHouse)
            {
              guestListUpdated = true;
              getCurrentGuestList().guests[player.Name].inHouse = true;
              getCurrentGuestList().guests[player.Name].latestEntry = DateTime.Now;
              getCurrentGuestList().guests[player.Name].timeCursor = DateTime.Now;
              getCurrentGuestList().guests[player.Name].entryCount++;
              showGuestEnterChatAlert(getCurrentGuestList().guests[player.Name], isSelf);
            }
            // Current user just entered house
            else if (justEnteredHouse)
            {
              getCurrentGuestList().guests[player.Name].timeCursor = DateTime.Now;
              // setting is enabled to notify them on existing users. 
              if (this.Configuration.showChatAlertAlreadyHere) 
                showGuestEnterChatAlert(getCurrentGuestList().guests[player.Name], isSelf);
            }
            
            // Re-mark as friend incase status changed 
            getCurrentGuestList().guests[player.Name].isFriend = pc.StatusFlags.HasFlag(StatusFlags.Friend);

            // Mark last seen 
            getCurrentGuestList().guests[player.Name].lastSeen = DateTime.Now;

            // Mark last time current player enter house 
            if (justEnteredHouse && isSelf)
            {
              getCurrentGuestList().guests[player.Name].latestEntry = DateTime.Now;
            }
          }

          // Check for guests that have left the house 
          foreach (var guest in getCurrentGuestList().guests)
          {
            // Guest is marked as in the house 
            if (guest.Value.inHouse) 
            {
              // Guest was not seen this loop 
              if (!seenPlayers.ContainsKey(guest.Value.Name))
              {
                guest.Value.onLeaveVenue();
                guestListUpdated = true;
                showGuestLeaveChatAlert(guest.Value);
              }
              // Guest was seen this loop 
              else 
              {
                guest.Value.onAccumulateTime();
              }
            }
            
          }

          // Only play doorbell sound once if there were one or more new people 
          if (Configuration.soundAlerts && playerArrived && !pluginState.snoozed)
          {
            doorbell.play();
          }

          // Save number of players seen this update 
          pluginState.playersInHouse = playerCount;

          // Save config if we saw new players
          if (guestListUpdated) getCurrentGuestList().save();

          justEnteredHouse = false;
          stopwatch.Restart();

          // Send data to server 
          if (Configuration.webserverConfig.sendDataOnInterval &&
            webserviceStopwatch.ElapsedMilliseconds > Configuration.webserverConfig.IntervalMiliseconds &&
            RestUtils.failedRequests <= RestUtils.maxFailedRequests)
          {
            webserviceStopwatch.Restart();
            getCurrentGuestList().sentToWebserver(this);
          }
        }
      }
      catch (Exception e)
      {
        Log.Error("Venue Manager Failed during framework update");
        Log.Error(e.ToString());
      }
      running = false;
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
      var messageBuilder = new SeStringBuilder();
      var knownVenue = venueList.venues.ContainsKey(pluginState.currentHouse.houseId);

      // Show text alert for self if the venue is known
      if (isSelf)
      {
        if (knownVenue)
        {
          var venue = venueList.venues[pluginState.currentHouse.houseId];
          if (this.Configuration.showPluginNameInChat) messageBuilder.AddText($"[{Name}] ");
          messageBuilder.AddText("You have entered " + venue.name);
          Chat.Print(new XivChatEntry() { Message = messageBuilder.Build() });
        }
        return;
      }

      // Don't show alerts if snoozed 
      if (pluginState.snoozed) return;
      // Don't show if chat alerts disabled 
      if (!Configuration.showChatAlerts) return;

      // Alert type is already here 
      bool isAlreadyHere = justEnteredHouse && this.Configuration.showChatAlertAlreadyHere;

      // Return if not showing already here alerts
      if (justEnteredHouse && !this.Configuration.showChatAlertAlreadyHere) return;

      // Return if reentry alerts are disabled. (We need to ignore this check for already here alerts)
      if (player.entryCount > 1 && !Configuration.showChatAlertReentry && !isAlreadyHere) return;
      // Return if entry alerts are disabled . (We need to ignore this check for already here alerts)
      if (player.entryCount == 1 && !Configuration.showChatAlertEntry && !isAlreadyHere) return;

      // Show text alert for guests
      if (this.Configuration.showPluginNameInChat) messageBuilder.AddText($"[{Name}] ");

      // Player Color 
      messageBuilder.AddUiForeground(Colors.getChatColor(player, true));

      // Add player message 
      messageBuilder.Add(new PlayerPayload(player.Name, player.homeWorld));
      messageBuilder.AddUiForegroundOff();

      // Message Color 
      messageBuilder.AddUiForeground(Colors.getChatColor(player, false));

      // Current player has re-entered the house 
      if (justEnteredHouse)
      {
        if (pluginState.isTrackingOutside)
          messageBuilder.AddText(" is already at the event");
        else
          messageBuilder.AddText(" is already inside");
      }
      // Player enters house while you are already inside
      else
      {
        if (pluginState.isTrackingOutside)
          messageBuilder.AddText(" has arrived");
        else
          messageBuilder.AddText(" has entered");
        if (player.entryCount > 1)
          messageBuilder.AddText(" (" + player.entryCount + ")");
      }

      // Venue Name 
      if (knownVenue)
      {
        var venue = venueList.venues[pluginState.currentHouse.houseId];
        messageBuilder.AddText(" " + venue.name);
      }
      else if (pluginState.isTrackingOutside)
      {
        messageBuilder.AddText(" at the event");
      }
      else
      {
        messageBuilder.AddText(" the " + TerritoryUtils.getHouseType(pluginState.territory));
      }

      messageBuilder.AddUiForegroundOff();
      Chat.Print(new XivChatEntry() { Message = messageBuilder.Build() });
    }

    private void showGuestLeaveChatAlert(Player player)
    {
      if (!Configuration.showChatAlerts) return;
      if (!Configuration.showChatAlertLeave) return;
      // Don't show alerts if snoozed 
      if (pluginState.snoozed) return;

      var isSelf = ClientState.LocalPlayer?.Name.TextValue == player.Name;
      if (isSelf) return;
      // Don't show leave alerts if user just entered the building
      if (justEnteredHouse) return;

      var messageBuilder = new SeStringBuilder();
      var knownVenue = venueList.venues.ContainsKey(pluginState.currentHouse.houseId);

      // Add plugin name 
      if (this.Configuration.showPluginNameInChat) messageBuilder.AddText($"[{Name}] ");

      // Add Player name 
      messageBuilder.Add(new PlayerPayload(player.Name, player.homeWorld));
      messageBuilder.AddText(" has left");

      // Add Venue info 
      if (knownVenue)
      {
        var venue = venueList.venues[pluginState.currentHouse.houseId];
        messageBuilder.AddText(" " + venue.name);
      }
      else if (pluginState.isTrackingOutside)
      {
        messageBuilder.AddText(" the event");
      }
      else
      {
        messageBuilder.AddText(" the " + TerritoryUtils.getHouseType(pluginState.territory));
      }

      var entry = new XivChatEntry() { Message = messageBuilder.Build() };
      Chat.Print(entry);
    }

    public GuestList getCurrentGuestList()
    {
      if (pluginState.userInHouse)
      {
        if (guestLists.ContainsKey(pluginState.currentHouse.houseId))
        {
          return guestLists[pluginState.currentHouse.houseId];
        }
      }
      else if (pluginState.isTrackingOutside)
      {
        return guestLists[1];
      }
      return guestLists[0];
    }

    // Post a clickable player link in chat
    public void chatPlayerLink(Player player)
    {

      var messageBuilder = new SeStringBuilder();
      messageBuilder.Add(new PlayerPayload(player.Name, player.homeWorld));
      var entry = new XivChatEntry() { Message = messageBuilder.Build() };
      Chat.Print(entry);
    }

  } // Plugin
}
