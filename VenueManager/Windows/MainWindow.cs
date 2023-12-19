using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using VenueManager.Tabs;
using VenueManager.Widgets;

namespace VenueManager.Windows;

public class MainWindow : Window, IDisposable
{
  private readonly Vector4 colorGreen = new(0,0.69f,0,1);

    private Plugin plugin;
    private Configuration configuration;
    private VenuesTab venuesTab;
    private SettingsTab settingsTab;
    private GuestListWidget guestListWidget;

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
        this.venuesTab = new VenuesTab(plugin);
        this.settingsTab = new SettingsTab(plugin);
        this.guestListWidget = new GuestListWidget(plugin);
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
            venuesTab.draw();
            ImGui.EndTabItem();
          }
        }
        // Render Settings Tab if selected 
        if (ImGui.BeginTabItem("Settings")) {
          this.settingsTab.draw();
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
          ImGui.Text("This venue is not saved. Guest list will be shared will all unsaved venues");
        }

        // We are in a saved house, draw guest list for that house
        if (plugin.venueList.venues.ContainsKey(plugin.pluginState.currentHouse.houseId)) 
          this.guestListWidget.draw(plugin.pluginState.currentHouse.houseId);
        // Otherwise draw public list 
        else 
          this.guestListWidget.draw(0);
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

      this.guestListWidget.draw(selectVenue);
    }
}
