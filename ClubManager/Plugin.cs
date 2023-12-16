using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ClubManager.Windows;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Game.Text;
using System.Diagnostics;
using FFXIVClientStructs.FFXIV.Client.Game.Housing;

namespace ClubManager
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Club Manager";
        private const string CommandName = "/club";
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static IFramework Framework { get; private set; } = null!;
        [PluginService] public static IDataManager DataManager { get; private set; } = null!;
        // Game Objects 
        [PluginService] public static IObjectTable Objects { get; private set; } = null!;
        [PluginService] public static IPluginLog Log { get; private set; } = null!;
        [PluginService] public static IChatGui Chat { get; private set; } = null!;

        public DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }
        public PluginState pluginState {get; init;}

        // Windows 
        public WindowSystem WindowSystem = new("ClubManager");
        private MainWindow MainWindow { get; init; }
        private Stopwatch stopwatch = new();
        private Sound doorbell;

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            this.pluginState = new PluginState();

            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);

            MainWindow = new MainWindow(this);

            WindowSystem.AddWindow(MainWindow);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "Open club manager interface to see guests who come in your home"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;

            // Bind territory changed listener to client 
            ClientState.TerritoryChanged += OnTerritoryChanged;
            Framework.Update += OnFrameworkUpdate;

            // Load Sound 
            doorbell = new Sound(this, Sound.DOORBELL_TYPE.DOORBELL);
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
                pluginState.currentHouse = new Club(); // Erase club when leaving 
                stopwatch.Stop();
            }

            this.Configuration.Save();
        }
        

        private unsafe void OnFrameworkUpdate(IFramework framework)
        {
          // Every second we are in a house. Process players and see what has changed 
          if (pluginState.userInHouse && stopwatch.ElapsedMilliseconds > 1000) {
            // Fetch updated house information 
            try {
              var housingManager = HousingManager.Instance();
              var worldId = ClientState.LocalPlayer?.CurrentWorld.Id;
              // If the user has transitioned into a new house. Store that house information. Ensure we have a world to set it to 
              if (pluginState.currentHouse.houseId != housingManager->GetCurrentHouseId() && worldId != null) {
                pluginState.currentHouse.houseId = housingManager->GetCurrentHouseId(); 
                pluginState.currentHouse.plot = housingManager->GetCurrentPlot() + 1; // Game stores plot as -1 
                pluginState.currentHouse.ward = housingManager->GetCurrentWard() + 1; // Game stores ward as -1 
                pluginState.currentHouse.worldId = worldId ?? 0;
                pluginState.currentHouse.district = TerritoryUtils.getHouseLocation(Configuration.territory);
              }
            } catch {
              Log.Error("Failed to load housing information");
            }

            bool configUpdated = false;
            bool playerArrived = false;
            foreach (var o in Objects)
            {
              if (o is not PlayerCharacter pc) continue;
              var player = Player.fromCharacter(pc);

              // New Player has entered the house 
              if (!this.Configuration.guests.ContainsKey(o.Name.TextValue)) {
                this.Configuration.guests.Add(player.Name, player);
                configUpdated = true;

                // Is the new player the current user 
                var isSelf = ClientState.LocalPlayer?.Name.TextValue == o.Name.TextValue;

                if (!isSelf) playerArrived = true;
                
                if (Configuration.showChatAlerts && !isSelf) {
                  // Message Chat for player arriving 
                  var messageBuilder = new SeStringBuilder();
                  messageBuilder.AddUiForeground(060); // Green. `/xldata` -> UIColor in chat in game 
                  messageBuilder.AddText($"[{Name}] ");
                  messageBuilder.Add(new PlayerPayload(player.Name, player.HomeWorld));
                  messageBuilder.AddText(" has entered the " + TerritoryUtils.getHouseType(this.Configuration.territory));
                  var entry = new XivChatEntry() {
                    Message = messageBuilder.Build()
                  };
                  Chat.Print(entry);
                }
              }
            }

            // Only play doorbell sound once if there were multiple new people 
            if (Configuration.soundAlerts && playerArrived) {
              doorbell.play();
            }

            if (configUpdated) this.Configuration.Save();

            stopwatch.Restart();
          }
            
        }

      public void playDoorbell() {
        doorbell.play();
      }

      public void reloadDoorbell() {
        doorbell.load();
      }
    }
}
