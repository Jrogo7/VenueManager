using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace ClubManager.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin plugin;
    private Configuration configuration;

    public MainWindow(Plugin plugin) : base(
        "Club Manager", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(250, 300),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
        this.configuration = plugin.Configuration;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        ImGui.BeginTabBar("Tabs");
        // Render Guests tab if selected 
        if (ImGui.BeginTabItem("Guests")) {

          // Render high level information 
          ImGui.Text(this.configuration.userInHouse ? 
            "You are in a " + TerritoryUtils.getHouseType(this.configuration.territory) + " in " + TerritoryUtils.getHouseLocation(this.configuration.territory) : 
            "You are not in a house.");

          // Clear guest list button 
          if (ImGui.Button("Clear Guest List")) {
            this.configuration.guests = new();
            this.configuration.Save();
          }

          // Draw Guests 
          ImGui.Text("Guests:");
          ImGui.BeginChild(1);
          ImGui.Indent(10);
          foreach (var guest in this.configuration.guests) {
            ImGui.Text(guest.Value.Name);
          }
          ImGui.Unindent(10);
          ImGui.EndChild();

          ImGui.EndTabItem();
        }
        // Render Settings Tab if selected 
        if (ImGui.BeginTabItem("Settings")) {
          // can't ref a property, so use a local copy
          var showChatAlerts = this.configuration.showChatAlerts;
          if (ImGui.Checkbox("Chat alerts", ref showChatAlerts))
          {
              this.configuration.showChatAlerts = showChatAlerts;
              this.configuration.Save();
          }

          var soundAlerts = this.configuration.soundAlerts;
          if (ImGui.Checkbox("Sound alerts", ref soundAlerts))
          {
              this.configuration.soundAlerts = soundAlerts;
              this.configuration.Save();
          }

          ImGui.Indent(20);
          if (ImGuiComponents.IconButton(FontAwesomeIcon.Music)) {
            plugin.playDoorbell();
          }
          if (ImGui.IsItemHovered()) {
              ImGui.SetTooltip("Test Sound");
          }
          var volume = this.configuration.soundVolume;
          if (ImGui.SliderFloat("Volume", ref volume, 0, 5)) {
            this.configuration.soundVolume = volume;
            plugin.reloadDoorbell();
          }
          ImGui.Unindent();

          ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
    }
}
