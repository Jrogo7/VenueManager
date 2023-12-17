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
        if (this.configuration.showGuestsTab) {
          if (ImGui.BeginTabItem("Guests")) {
            drawGuestsMenu();

            ImGui.EndTabItem();
          }
          if (ImGui.BeginTabItem("Guest Logs")) {
            drawGuestLogMenu();

            ImGui.EndTabItem();
          }
        }
        // Render Venues Tab 
        if (this.configuration.showVenueTab) {
          if (ImGui.BeginTabItem("Venues")) {
            drawVenueMenu();
            ImGui.EndTabItem();
          }
        }
        // Render Settings Tab if selected 
        if (ImGui.BeginTabItem("Settings")) {
          drawSettings();
          ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
    }

    private void drawGuestsMenu() {
      // Render high level information 
      if (plugin.pluginState.userInHouse) {
        if (plugin.venueList.venues.ContainsKey(plugin.pluginState.currentHouse.houseId)) {
          var venue = plugin.venueList.venues[plugin.pluginState.currentHouse.houseId];
          ImGui.Text("You are at " + venue.name);
        } else {
          var typeText = TerritoryUtils.isPlotType(plugin.pluginState.currentHouse.type) ? 
            "P" + plugin.pluginState.currentHouse.plot : 
            "Room" + plugin.pluginState.currentHouse.room;
          ImGui.Text("You are in a " + TerritoryUtils.getHouseType(plugin.pluginState.currentHouse.type) + " in " + 
            plugin.pluginState.currentHouse.district + " W" + plugin.pluginState.currentHouse.ward + " " + typeText);
        }

        // List the number of players in the house 
        ImGui.TextWrapped($"There are currently {plugin.pluginState.playersInHouse} guests inside (out of {plugin.getCurrentGuestList().guests.Count} total visitors)");
      } else {
        ImGui.Text("You are not in a house.");
      }
      if (plugin.pluginState.snoozed) ImGui.TextColored(new Vector4(.82f, .5f, .04f, 1f), "Alarms snoozed");
      ImGui.Spacing();
      ImGui.Separator();
      ImGui.Spacing();

      if (plugin.pluginState.userInHouse) {
        if (plugin.venueList.venues.ContainsKey(plugin.pluginState.currentHouse.houseId)) {
          var venue = plugin.venueList.venues[plugin.pluginState.currentHouse.houseId];
          ImGui.Text("Guest list for " + venue.name);
        } else {
          ImGui.Text("This venue is not saved. Guest list will be shared will all unsaved houses");
        }

        drawGuestList(plugin.pluginState.currentHouse.houseId);
      }
      else {
          ImGui.Text("Guest list will be shown when you enter a house");
      }
    }

    // Current venue selected for logs 
    private long selectVenue = 0;
    private readonly string defaultVenueName = "Default (shared with all non-saved venues)";

    private void drawGuestLogMenu() {
      string displayName = "";
      if (selectVenue == 0) displayName = defaultVenueName;
      else if (plugin.venueList.venues.ContainsKey(selectVenue)) {
        displayName = plugin.venueList.venues[selectVenue].name;
      }

      ImGui.Text("Select venue to display guest log for:");
      // Combo box of all venues 
      if (ImGui.BeginCombo("##VenueForLogs", displayName)) {
        // Default 0 Venue 
        if (ImGui.Selectable(defaultVenueName, selectVenue == 0)) {
          selectVenue = 0;
        }
        if (selectVenue == 0)
            ImGui.SetItemDefaultFocus();
        
        // All Saved Venues 
        foreach (var venue in plugin.venueList.venues)
        {
            bool is_selected = venue.Key == selectVenue;
            if (ImGui.Selectable(venue.Value.name, is_selected)) {
              selectVenue = venue.Key;
            }
            if (is_selected)
                ImGui.SetItemDefaultFocus();
        }
        ImGui.EndCombo();
      }

      ImGui.Separator();
      ImGui.Spacing();

      drawGuestList(selectVenue);
    }

    private void drawGuestList(long houseId) {
      // Ensure we have this guest list 
      if (!plugin.guestLists.ContainsKey(houseId)) {
        GuestList guestList = new GuestList(houseId, "");
        guestList.load();
        plugin.guestLists.Add(houseId, guestList);
      }


      bool disabled = false;
      if (!ImGui.IsKeyDown(ImGuiKey.LeftCtrl) && !ImGui.IsKeyDown(ImGuiKey.RightCtrl)) {
        ImGui.BeginDisabled();
        disabled = true;
      }
      // Clear guest list button 
      if (ImGui.Button("Clear Guest List")) {
        plugin.guestLists[houseId].guests = new();
        plugin.guestLists[houseId].save();
      }
      if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) && disabled) {
        ImGui.SetTooltip("Hold control to clear guest list");
      }
      if (disabled) ImGui.EndDisabled();

      // Draw Guests 
      ImGui.Text($"Guests ({plugin.guestLists[houseId].guests.Count})");
      ImGui.BeginChild(1);
      ImGui.Indent(10);

      // Generate sorted guest list 
      var sortedGuestList = plugin.guestLists[houseId].guests.ToList();
      sortedGuestList.Sort((pair1,pair2) => pair2.Value.firstSeen.CompareTo(pair1.Value.firstSeen));
      foreach (var guest in sortedGuestList) {
        var color = guest.Value.inHouse ? new Vector4(1,1,1,1) : new Vector4(.5f,.5f,.5f,1);
        ImGui.TextColored(color, guest.Value.firstSeen.ToString("hh:mm") + " - " + guest.Value.Name);
      }
      ImGui.Unindent(10);
      ImGui.EndChild();
    }

    private void drawSettings() {
      ImGui.BeginChild(1);
      ImGui.Text("Tab Visibility");
      var showGuestsTab = this.configuration.showGuestsTab;
      if (ImGui.Checkbox("Guests Tab", ref showGuestsTab))
      {
          this.configuration.showGuestsTab = showGuestsTab;
          this.configuration.Save();
      }
      ImGui.Indent(20);
      ImGui.TextWrapped("Hiding the Guests Tab will also disable all notifications around guests entering or leaving.");
      ImGui.Unindent();
      var showVenueTab = this.configuration.showVenueTab;
      if (ImGui.Checkbox("Venues Tab", ref showVenueTab))
      {
          this.configuration.showVenueTab = showVenueTab;
          this.configuration.Save();
      }

      if (!this.configuration.showGuestsTab && !this.configuration.showVenueTab) {
        ImGui.TextColored(new Vector4(0.9f,0,1f,1f), "So Empty :(");
      }
      
      ImGui.Separator();
      ImGui.Spacing();

      if (!this.configuration.showGuestsTab) ImGui.BeginDisabled();

      ImGui.Text("Guest Chat Alerts");
      var showChatAlerts = this.configuration.showChatAlerts;
      if (ImGui.Checkbox("Enabled##showChatAlerts", ref showChatAlerts))
      {
          this.configuration.showChatAlerts = showChatAlerts;
          this.configuration.Save();
      }

      if (!this.configuration.showChatAlerts) ImGui.BeginDisabled();
      // Reentry Alerts 
      var showChatAlertReentry = this.configuration.showChatAlertReentry;
      if (ImGui.Checkbox("Reentry Alerts", ref showChatAlertReentry))
      {
        this.configuration.showChatAlertReentry = showChatAlertReentry;
        this.configuration.Save();
      }
      if (ImGui.IsItemHovered()) {
        ImGui.SetTooltip("Display chat message for when a user reenters a club after leaving");
      }

      // Include plugin name in alerts 
      var showPluginNameInChat = this.configuration.showPluginNameInChat;
      if (ImGui.Checkbox("Include Plugin Name", ref showPluginNameInChat))
      {
          this.configuration.showPluginNameInChat = showPluginNameInChat;
          this.configuration.Save();
      }
      if (!this.configuration.showChatAlerts) ImGui.EndDisabled();

      ImGui.Separator();
      ImGui.Spacing();

      ImGui.Text("Guest Sound Alerts");
      // Enable / Disable sound allerts 
      var soundAlerts = this.configuration.soundAlerts;
      if (ImGui.Checkbox("Enabled##soundAlerts", ref soundAlerts))
      {
          this.configuration.soundAlerts = soundAlerts;
          this.configuration.Save();
      }
      if (!this.configuration.soundAlerts) ImGui.BeginDisabled();
      // Allow the user to select which doorbell sound they would like 
      if (ImGui.BeginCombo("Doorbell sound", DoorbellSound.DoorbellSoundTypes[(int)configuration.doorbellType])) {
        var doorbells = (DOORBELL_TYPE[])Enum.GetValues(typeof(DOORBELL_TYPE));
        for (int i = 0; i < doorbells.Length; i++)
        {
            bool is_selected = configuration.doorbellType == doorbells[i];
            if (ImGui.Selectable(DoorbellSound.DoorbellSoundTypes[i], is_selected)) {
              configuration.doorbellType = doorbells[i];
              configuration.Save();
              plugin.reloadDoorbell();
            }
            if (is_selected)
                ImGui.SetItemDefaultFocus();
        }
        ImGui.EndCombo();
      }
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
      
      if (!this.configuration.soundAlerts) ImGui.EndDisabled();
      if (!this.configuration.showGuestsTab) ImGui.EndDisabled();
      ImGui.EndChild();
    }

    // Venue name inside input box 
    private string venueName = string.Empty;

    // Draw venue list menu 
    private void drawVenueMenu() {

      ImGui.Text("Save the current venue you are in to the list of venues");
      ImGui.InputTextWithHint("", "Enter venue name", ref venueName, 256);
      ImGui.SameLine();
      // Only allow saving venue if name is entered, user is in a house, and current house id is not in list 
      bool canAdd = venueName.Length > 0 && 
        plugin.pluginState.userInHouse && 
        !plugin.venueList.venues.ContainsKey(plugin.pluginState.currentHouse.houseId);
      if (!canAdd) ImGui.BeginDisabled();
      if (ImGui.Button("Save Venue")) {
        // Save venue to saved venue list 
        Venue venue = new Venue(plugin.pluginState.currentHouse);
        venue.name = venueName;
        plugin.venueList.venues.Add(venue.houseId, venue);
        plugin.venueList.save();
        // Add a new guest list to the main registry for this venue 
        GuestList guestList = new GuestList(venue.houseId, venueName);
        plugin.guestLists.Add(venue.houseId, guestList);
      }
      if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled)) {
        if (!plugin.pluginState.userInHouse)
          ImGui.SetTooltip("You are not in a house");
        else if (plugin.venueList.venues.ContainsKey(plugin.pluginState.currentHouse.houseId))
          ImGui.SetTooltip("Current venue already saved as " + plugin.venueList.venues[plugin.pluginState.currentHouse.houseId].name);
        else if (venueName.Length == 0) 
          ImGui.SetTooltip("You must enter a name");
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

        foreach (var venue in plugin.venueList.venues) {
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
            plugin.venueList.venues.Remove(venue.Value.houseId);
            plugin.venueList.save();
          }
          if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) && disabled) {
            ImGui.SetTooltip("Hold control to delete");
          }
          if (disabled) ImGui.EndDisabled();
        }

        ImGui.EndTable();
      }
    }
}
