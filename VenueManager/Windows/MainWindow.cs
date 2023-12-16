using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace VenueManager.Windows;

public class MainWindow : Window, IDisposable
{
  private readonly Vector4 colorGreen = new(0,0.69f,0,1);

    private Plugin plugin;
    private Configuration configuration;

    public MainWindow(Plugin plugin) : base(
        "Venue Manager", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
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
          if (plugin.pluginState.userInHouse) {
            if (plugin.venueStore.venues.ContainsKey(plugin.pluginState.currentHouse.houseId)) {
              var venue = plugin.venueStore.venues[plugin.pluginState.currentHouse.houseId];
              ImGui.Text("You are at " + venue.name);
            } else {
              var typeText = TerritoryUtils.isPlotType(plugin.pluginState.currentHouse.type) ? 
                "P" + plugin.pluginState.currentHouse.plot : 
                "Room" + plugin.pluginState.currentHouse.room;
              ImGui.Text("You are in a " + TerritoryUtils.getHouseType(plugin.pluginState.currentHouse.type) + " in " + 
                plugin.pluginState.currentHouse.district + " W" + plugin.pluginState.currentHouse.ward + " " + typeText);
            }

            // List the number of players in the house 
            ImGui.TextWrapped($"There are currently {plugin.pluginState.playersInHouse} guests inside (out of {this.configuration.guests.Count} total visitors)");
          } else {
            ImGui.Text("You are not in a house.");
          }
          ImGui.Spacing();
          ImGui.Separator();
          ImGui.Spacing();

          // Clear guest list button 
          if (ImGui.Button("Clear Guest List")) {
            this.configuration.guests = new();
            this.configuration.Save();
          }

          // Draw Guests 
          ImGui.Text($"Guests ({this.configuration.guests.Count})");
          ImGui.BeginChild(1);
          ImGui.Indent(10);

          // Generate sorted guest list 
          var sortedGuestList = this.configuration.guests.ToList();
          sortedGuestList.Sort((pair1,pair2) => pair2.Value.firstSeen.CompareTo(pair1.Value.firstSeen));
          foreach (var guest in sortedGuestList) {
            var color = guest.Value.inHouse ? new Vector4(1,1,1,1) : new Vector4(.5f,.5f,.5f,1);
            ImGui.TextColored(color, guest.Value.firstSeen.ToString("hh:mm") + " - " + guest.Value.Name);
          }
          ImGui.Unindent(10);
          ImGui.EndChild();

          ImGui.EndTabItem();
        }
        // Render Venues Tab 
        if (ImGui.BeginTabItem("Venues")) {
          drawVenueMenu();
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

          // can't ref a property, so use a local copy
          var showPluginNameInChat = this.configuration.showPluginNameInChat;
          ImGui.Indent(20);
          if (ImGui.Checkbox("Include Plugin Name", ref showPluginNameInChat))
          {
              this.configuration.showPluginNameInChat = showPluginNameInChat;
              this.configuration.Save();
          }
          ImGui.Unindent();

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

    // Venue name inside input box 
    private string venueName = string.Empty;

    // Draw venue list menu 
    private void drawVenueMenu() {

      ImGui.InputTextWithHint("", "Enter venue name", ref venueName, 256);
      ImGui.SameLine();
      // Only allow saving venue if name is entered, user is in a house, and current house id is not in list 
      bool canAdd = venueName.Length > 0 && 
        plugin.pluginState.userInHouse && 
        !plugin.venueStore.venues.ContainsKey(plugin.pluginState.currentHouse.houseId);
      if (!canAdd) ImGui.BeginDisabled();
      if (ImGui.Button("Save Venue")) {
        Venue venue = new Venue(plugin.pluginState.currentHouse);
        venue.name = venueName;
        plugin.venueStore.venues.Add(venue.houseId, venue);
        plugin.venueStore.save();
      }
      if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) {
        if (!plugin.pluginState.userInHouse)
          ImGui.SetTooltip("You are not in a house");
        else if (venueName.Length == 0) 
          ImGui.SetTooltip("You must enter a name");
        else if (plugin.venueStore.venues.ContainsKey(plugin.pluginState.currentHouse.houseId))
          ImGui.SetTooltip("Venue already saved");
      }
      if (!canAdd) ImGui.EndDisabled();

      ImGui.Spacing();
      if (ImGui.BeginTable("Venues", 7)) {
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("District");
        ImGui.TableSetupColumn("Ward");
        ImGui.TableSetupColumn("Plot");
        ImGui.TableSetupColumn("Room");
        ImGui.TableSetupColumn("World");
        ImGui.TableSetupColumn("Delete");
        ImGui.TableHeadersRow();

        foreach (var venue in plugin.venueStore.venues) {
          var fontColor = plugin.pluginState.userInHouse && plugin.pluginState.currentHouse.houseId == venue.Value.houseId ?
            colorGreen : new Vector4(1,1,1,1);
            
          ImGui.TableNextColumn();
          ImGui.TextColored(fontColor, venue.Value.name);
          ImGui.TableNextColumn();
          ImGui.TextColored(fontColor, venue.Value.district);
          ImGui.TableNextColumn();
          ImGui.TextColored(fontColor, "" + venue.Value.ward);
          ImGui.TableNextColumn();
          ImGui.TextColored(fontColor, TerritoryUtils.isPlotType(venue.Value.type) ? "" + venue.Value.plot : "");
          ImGui.TableNextColumn();
          ImGui.TextColored(fontColor, !TerritoryUtils.isPlotType(venue.Value.type) ? "" + venue.Value.room : "");
          ImGui.TableNextColumn();
          ImGui.TextColored(fontColor, venue.Value.WorldName);
          ImGui.TableNextColumn();
          
          bool disabled = false;
          if (!ImGui.IsKeyDown(ImGuiKey.LeftCtrl) && !ImGui.IsKeyDown(ImGuiKey.RightCtrl)) {
            ImGui.BeginDisabled();
            disabled = true;
          }

          // Allow the user to delete the saved venue
          if (ImGuiComponents.IconButton("##" + venue.Value.houseId, FontAwesomeIcon.Trash)) {
            plugin.venueStore.venues.Remove(venue.Value.houseId);
            plugin.venueStore.save();
          }
          if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) && disabled) {
            ImGui.SetTooltip("You must hold control to delete");
          }
          if (disabled) ImGui.EndDisabled();
        }

        ImGui.EndTable();
      }
    }
}
