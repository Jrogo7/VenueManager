using System;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using ImGuiNET;

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

    if (!this.configuration.showGuestsTab && !this.configuration.showVenueTab)
    {
      ImGui.TextColored(new Vector4(0.9f, 0, 1f, 1f), "So Empty :(");
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
    if (ImGui.Checkbox("Reentry Alerts", ref showChatAlertReentry))
    {
      this.configuration.showChatAlertReentry = showChatAlertReentry;
      this.configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
      ImGui.SetTooltip("Display chat message when a guest reenters a venue after leaving");
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
      plugin.reloadDoorbell();
    }

    if (!this.configuration.soundAlerts) ImGui.EndDisabled();
    if (!this.configuration.showGuestsTab) ImGui.EndDisabled();
    ImGui.Unindent();
    ImGui.EndChild();
  }
}