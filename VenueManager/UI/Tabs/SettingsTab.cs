using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using FFXIVClientStructs.FFXIV.Client.Game;
using ImGuiNET;

using System.Drawing;
using System.Linq;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Memory;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using Map = Lumina.Excel.Sheets.Map;

namespace VenueManager.Tabs;

public class SettingsTab
{
  private Plugin plugin;
  private Configuration configuration;

  public SettingsTab(Plugin plugin)
  {
    this.plugin = plugin;
    this.configuration = plugin.Configuration;
  }

  // Draw settings menu 
  public unsafe void draw()
  {
    ImGui.BeginChild(1);
    ImGui.Indent(5);

    ImGui.Text("Tab Visibility");
    var showGuestsTab = this.configuration.showGuestsTab;
    if (ImGui.Checkbox("Guest Tabs", ref showGuestsTab))
    {
      this.configuration.showGuestsTab = showGuestsTab;
      this.configuration.Save();
    }
    ImGui.Indent(20);

    // Guest tab sub settings 
    if (!this.configuration.showGuestsTab) ImGui.BeginDisabled();
    ImGui.TextWrapped("Hiding the Guests Tab will also disable all notifications around guests entering or leaving.");
    // Webserver configuration 
    var showWebserviceLogging = this.configuration.showWebserviceLogging;
    if (ImGui.Checkbox("Webservice Logging", ref showWebserviceLogging))
    {
      this.configuration.showWebserviceLogging = showWebserviceLogging;
      this.configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
      ImGui.SetTooltip("This is an advanced feature that is for developers");
    }
    // Stats tab
    var showStatsTab = this.configuration.showStatsTab;
    if (ImGui.Checkbox("Stats Tab (work in progress)", ref showStatsTab))
    {
      this.configuration.showStatsTab = showStatsTab;
      this.configuration.Save();
    }
    if (!this.configuration.showGuestsTab) ImGui.EndDisabled();

    ImGui.Unindent();
    var showVenueTab = this.configuration.showVenueTab;
    if (ImGui.Checkbox("Venues Tab", ref showVenueTab))
    {
      this.configuration.showVenueTab = showVenueTab;
      this.configuration.Save();
    }

    if (!this.configuration.showGuestsTab && !this.configuration.showVenueTab)
    {
      ImGui.TextColored(new Vector4(0.9f, 0, 1f, 1f), "So Empty :(");
    }

    // =============================================================================
    ImGui.Separator();
    ImGui.Spacing();
    ImGui.Text("Guest List");
    var sortCurrentVisitorsTop = this.configuration.sortCurrentVisitorsTop;
    if (ImGui.Checkbox("Pin current visitors to top", ref sortCurrentVisitorsTop))
    {
      this.configuration.sortCurrentVisitorsTop = sortCurrentVisitorsTop;
      this.configuration.Save();
    }
    if (ImGui.IsItemHovered()) {
      ImGui.SetTooltip("Pin current visitors to the top of the guest list");
    }

    var sortFriendsToTop = this.configuration.sortFriendsToTop;
    if (ImGui.Checkbox("Pin frients to top", ref sortFriendsToTop))
    {
      this.configuration.sortFriendsToTop = sortFriendsToTop;
      this.configuration.Save();
    }
    if (ImGui.IsItemHovered()) {
      ImGui.SetTooltip("Pin friends to the top of the guest list");
    }

    // =============================================================================
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
    ImGui.Indent(20);
    // Entry Alerts 
    var showChatAlertEntry = this.configuration.showChatAlertEntry;
    if (ImGui.Checkbox("Entry Alerts", ref showChatAlertEntry))
    {
      this.configuration.showChatAlertEntry = showChatAlertEntry;
      this.configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
      ImGui.SetTooltip("Display chat message when a guest enters a venue");
    }

    // Reentry Alerts 
    var showChatAlertReentry = this.configuration.showChatAlertReentry;
    if (ImGui.Checkbox("Re-entry Alerts", ref showChatAlertReentry))
    {
      this.configuration.showChatAlertReentry = showChatAlertReentry;
      this.configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
      ImGui.SetTooltip("Display chat message when a guest re-enters a venue after leaving");
    }

    // Current Visitor
    var showChatAlertAlreadyHere = this.configuration.showChatAlertAlreadyHere;
    if (ImGui.Checkbox("Current Visitors on Entry", ref showChatAlertAlreadyHere))
    {
      this.configuration.showChatAlertAlreadyHere = showChatAlertAlreadyHere;
      this.configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
      ImGui.SetTooltip("Display chat message for all current guests when re-entering a house");
    }

    // Leave Alerts 
    var showChatAlertLeave = this.configuration.showChatAlertLeave;
    if (ImGui.Checkbox("Leave Alerts", ref showChatAlertLeave))
    {
      this.configuration.showChatAlertLeave = showChatAlertLeave;
      this.configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
      ImGui.SetTooltip("Display chat message when a guest leaves");
    }

    // Include plugin name in alerts 
    var showPluginNameInChat = this.configuration.showPluginNameInChat;
    if (ImGui.Checkbox("Include Plugin Name", ref showPluginNameInChat))
    {
      this.configuration.showPluginNameInChat = showPluginNameInChat;
      this.configuration.Save();
    }
    if (!this.configuration.showChatAlerts) ImGui.EndDisabled();
    ImGui.Unindent();

    // =============================================================================
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
    if (ImGui.BeginCombo("Doorbell sound", DoorbellSound.DoorbellSoundTypes[(int)configuration.doorbellType]))
    {
      var doorbells = (DOORBELL_TYPE[])Enum.GetValues(typeof(DOORBELL_TYPE));
      for (int i = 0; i < doorbells.Length; i++)
      {
        bool is_selected = configuration.doorbellType == doorbells[i];
        if (ImGui.Selectable(DoorbellSound.DoorbellSoundTypes[i], is_selected))
        {
          configuration.doorbellType = doorbells[i];
          configuration.Save();
          plugin.reloadDoorbell();
        }
        if (is_selected)
          ImGui.SetItemDefaultFocus();
      }
      ImGui.EndCombo();
    }
    if (ImGuiComponents.IconButton(FontAwesomeIcon.Music))
    {
      plugin.playDoorbell();
    }
    if (ImGui.IsItemHovered())
    {
      ImGui.SetTooltip("Test Sound");
    }
    var volume = this.configuration.soundVolume;
    if (ImGui.SliderFloat("Volume", ref volume, 0, 5))
    {
      this.configuration.soundVolume = volume;
      configuration.Save();
      plugin.reloadDoorbell();
    }

    if (!this.configuration.soundAlerts) ImGui.EndDisabled();
    if (!this.configuration.showGuestsTab) ImGui.EndDisabled();

    // =============================================================================
    ImGui.Separator();
    ImGui.Spacing();
  
    try {
      var housingManager = HousingManager.Instance();

      var mapData = Plugin.DataManager.GetExcelSheet<Map>().GetRow(AgentMap.Instance()->SelectedMapId);
      string[] parts = mapData.PlaceName.Value.Name.ExtractText().Split(" - ");
      string district = parts.Length == 2 ? parts[1] : "";

    ImGui.Text($@"Debug Info

Territory Id: {plugin.pluginState.territory}
In House: {plugin.pluginState.userInHouse}

HouseID: {housingManager->GetCurrentIndoorHouseId()}
Plot: {housingManager->GetCurrentPlot() + 1}
Ward: {housingManager->GetCurrentWard() + 1}
Room: {housingManager->GetCurrentRoom()}
Division: {housingManager->GetCurrentDivision()}
District: {district}
PlaceName: {mapData.PlaceName.Value.Name.ExtractText()}
");
    } catch {

    }

    ImGui.Unindent();
    ImGui.EndChild();
  }
}