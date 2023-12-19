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
    private GuestsTab guestsTab;

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
        this.guestsTab = new GuestsTab(plugin);
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
            this.guestsTab.draw();

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
