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

namespace ClubManager
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Club Manager";
        private const string CommandName = "/club";
        [PluginService] public static IClientState ClientState { get; private set; } = null!;
        [PluginService] public static IFramework Framework { get; private set; } = null!;
        // Game Objects 
        [PluginService] public static IObjectTable Objects { get; private set; } = null!;
        [PluginService] public static IPluginLog Log { get; private set; } = null!;
        [PluginService] public static IChatGui Chat { get; private set; } = null!;

        private DalamudPluginInterface PluginInterface { get; init; }
        private ICommandManager CommandManager { get; init; }
        public Configuration Configuration { get; init; }

        // Windows 
        public WindowSystem WindowSystem = new("ClubManager");
        private MainWindow MainWindow { get; init; }
        private Stopwatch stopwatch = new();

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager)
        {
            Log.Debug("Club Manager Plugin started");

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

            // Run territory change one time on boot to register current location 
            OnTerritoryChanged(ClientState.TerritoryType);
        }

        public void Dispose()
        {
            // Remove framework listener on close 
            Framework.Update -= OnFrameworkUpdate;
            // Remove territory change listener 
            ClientState.TerritoryChanged -= OnTerritoryChanged;

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
                this.Configuration.userInHouse = true;
                stopwatch.Start();
            }
            else if (this.Configuration.userInHouse)
            {
                this.Configuration.userInHouse = false;
                stopwatch.Stop();
            }

            this.Configuration.Save();
        }

        private void OnFrameworkUpdate(IFramework framework)
        {
          // Every second we are in a house. Process players and see what has changed 
          if (Configuration.userInHouse && stopwatch.ElapsedMilliseconds > 1000) {
            Log.Info("Process Players");
            bool configUpdated = false;
            foreach (var o in Objects)
            {
              if (o is not PlayerCharacter pc) continue;
              var player = Player.fromCharacter(pc);

              if (!this.Configuration.guests.ContainsKey(o.ObjectId)) {
                this.Configuration.guests.Add(o.ObjectId, player);
                configUpdated = true;
                
                if (Configuration.showChatAlerts) {
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

            if (configUpdated) this.Configuration.Save();

            stopwatch.Restart();
          }
            
        }
    }
}
