using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;
using ImGuiNET;
using VenueManager.Widgets;

namespace VenueManager.Tabs;

public class GuestsTab
{
  private Plugin plugin;
  private GuestListWidget guestListWidget;

  public GuestsTab(Plugin plugin)
  {
    this.plugin = plugin;
    this.guestListWidget = new GuestListWidget(plugin);
  }

  public unsafe void draw()
  {
    // Render high level information 
    if (plugin.pluginState.userInHouse)
    {
      if (plugin.venueList.venues.ContainsKey(plugin.pluginState.currentHouse.houseId))
      {
        var venue = plugin.venueList.venues[plugin.pluginState.currentHouse.houseId];
        ImGui.Text("You are at " + venue.name);
      }
      else
      {
        var typeText = TerritoryUtils.isPlotType(plugin.pluginState.currentHouse.type) ?
          "P" + plugin.pluginState.currentHouse.plot :
          "Room" + plugin.pluginState.currentHouse.room;
        ImGui.Text("You are in a " + TerritoryUtils.getHouseType(plugin.pluginState.currentHouse.type) + " in " +
          plugin.pluginState.currentHouse.district + " W" + plugin.pluginState.currentHouse.ward + " " + typeText);
      }

      // List the number of players in the house 
      ImGui.TextWrapped($"There are currently {plugin.pluginState.playersInHouse} guests inside (out of {plugin.getCurrentGuestList().guests.Count} total visitors)");
    }
    else if (plugin.pluginState.isTrackingOutside) {
      ImGui.Text("Currently tracking outdoor event");
      // List the number of players at the event
      ImGui.TextWrapped($"There are currently {plugin.pluginState.playersInHouse} guests at this event (out of {plugin.getCurrentGuestList().guests.Count} total visitors)");
      if (ImGui.Button("Stop Tracking")) {
        plugin.pluginState.isTrackingOutside = false;
        plugin.stopTimers();
      }
    } 
    else
    {
      ImGui.Text("You are not in a house.");
      if (ImGui.Button("Track outdoor event?")) {
        plugin.pluginState.isTrackingOutside = true;
        plugin.startTimers();
      }
    }
    if (plugin.pluginState.snoozed) ImGui.TextColored(new Vector4(.82f, .5f, .04f, 1f), "Alarms snoozed");
    ImGui.Spacing();
    ImGui.Separator();
    ImGui.Spacing();

    if (plugin.pluginState.userInHouse)
    {
      if (plugin.venueList.venues.ContainsKey(plugin.pluginState.currentHouse.houseId))
      {
        var venue = plugin.venueList.venues[plugin.pluginState.currentHouse.houseId];
        ImGui.Text("Guest list for " + venue.name);
      }
      else
      {
        ImGui.Text("This venue is not saved. Not all features will be supported.");
      }

      // We are in a saved house, draw guest list for that house
      if (plugin.venueList.venues.ContainsKey(plugin.pluginState.currentHouse.houseId))
        this.guestListWidget.draw(plugin.pluginState.currentHouse.houseId);
      // Otherwise draw public list 
      else
        this.guestListWidget.draw(0);
    }
    else if (plugin.pluginState.isTrackingOutside) {
      ImGui.Text("Guests for outdoor event");
      this.guestListWidget.draw(1);
    }
    else
    {
      ImGui.Text("Guest list will be shown when you enter a house");
    }
  }
}