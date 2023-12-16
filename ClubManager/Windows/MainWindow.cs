using System;
using System.Linq;
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
          ImGui.Text(plugin.pluginState.userInHouse ? 
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

          // Generate sorted guest list 
          var sortedGuestList = this.configuration.guests.ToList();
          sortedGuestList.Sort((pair1,pair2) => pair2.Value.firstSeen.CompareTo(pair1.Value.firstSeen));
          foreach (var guest in sortedGuestList) {
            ImGui.Text(guest.Value.firstSeen.ToString("hh:mm") + " - " + guest.Value.Name);
          }
          ImGui.Unindent(10);
          ImGui.EndChild();

          ImGui.EndTabItem();
        }
        // Render Clubs Tab 
        if (ImGui.BeginTabItem("Clubs")) {
          drawClubMenu();
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

    // Club name inside input box 
    private string clubName = string.Empty;

    // Draw club list menu 
    private void drawClubMenu() {

      ImGui.InputTextWithHint("", "Enter club name", ref clubName, 256);
      ImGui.SameLine();
      // Only allow saving club if name is entered, user is in a house, and current house id is not in list 
      bool canAdd = clubName.Length > 0 && 
        plugin.pluginState.userInHouse && 
        !this.configuration.knownClubs.ContainsKey(plugin.pluginState.currentHouse.houseId);
      if (!canAdd) ImGui.BeginDisabled();
      if (ImGui.Button("Save Club")) {
        Club club = new Club(plugin.pluginState.currentHouse);
        club.name = clubName;
        this.configuration.knownClubs.Add(club.houseId, club);
        this.configuration.Save();
      }
      if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) {
        if (!plugin.pluginState.userInHouse)
          ImGui.SetTooltip("You are not in a house");
        else if (clubName.Length == 0) 
          ImGui.SetTooltip("You must enter a name");
        else if (this.configuration.knownClubs.ContainsKey(plugin.pluginState.currentHouse.houseId))
          ImGui.SetTooltip("Club already saved");
      }
      if (!canAdd) ImGui.EndDisabled();

      ImGui.Spacing();
      ImGui.Text("Clubs:");
      if (ImGui.BeginTable("Clubs", 6)) {
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("District");
        ImGui.TableSetupColumn("Ward");
        ImGui.TableSetupColumn("Plot");
        ImGui.TableSetupColumn("World");
        ImGui.TableSetupColumn("Delete");
        ImGui.TableHeadersRow();

        foreach (var club in configuration.knownClubs) {
          var fontColor = plugin.pluginState.userInHouse && plugin.pluginState.currentHouse.houseId == club.Value.houseId ?
            new Vector4(0,0.69f,0,1) : new Vector4(1,1,1,1);
            
          ImGui.TableNextColumn();
          ImGui.TextColored(fontColor, club.Value.name);
          ImGui.TableNextColumn();
          ImGui.TextColored(fontColor, club.Value.district);
          ImGui.TableNextColumn();
          ImGui.TextColored(fontColor, "" + club.Value.ward);
          ImGui.TableNextColumn();
          ImGui.TextColored(fontColor, "" + club.Value.plot);
          ImGui.TableNextColumn();
          ImGui.TextColored(fontColor, club.Value.WorldName);
          ImGui.TableNextColumn();
          // Allow the user to delete the saved club
          if (ImGuiComponents.IconButton("##" + club.Value.houseId, FontAwesomeIcon.Trash)) {
            this.configuration.knownClubs.Remove(club.Value.houseId);
            this.configuration.Save();
          }
        }

        ImGui.EndTable();
      }
    }
}
