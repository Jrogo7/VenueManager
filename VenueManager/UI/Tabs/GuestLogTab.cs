using ImGuiNET;
using VenueManager.Widgets;

namespace VenueManager.Tabs;

public class GuestLogTab
{
  private Plugin plugin;
  private GuestListWidget guestListWidget;

  // Current venue selected for logs 
  private long selectVenue = 0;
  private readonly string defaultVenueName = "Default (shared with all non-saved venues)";

  public GuestLogTab(Plugin plugin)
  {
    this.plugin = plugin;
    this.guestListWidget = new GuestListWidget(plugin);
  }

  public unsafe void draw()
  {
    string displayName = "";
    if (selectVenue == 0) displayName = defaultVenueName;
    else if (plugin.venueList.venues.ContainsKey(selectVenue))
    {
      displayName = plugin.venueList.venues[selectVenue].name;
    }

    ImGui.Text("Select venue to display guest log for:");
    // Combo box of all venues 
    if (ImGui.BeginCombo("##VenueForLogs", displayName))
    {
      // Default 0 Venue 
      if (ImGui.Selectable(defaultVenueName, selectVenue == 0))
      {
        selectVenue = 0;
      }
      if (selectVenue == 0)
        ImGui.SetItemDefaultFocus();

      // All Saved Venues 
      foreach (var venue in plugin.venueList.venues)
      {
        bool is_selected = venue.Key == selectVenue;
        if (ImGui.Selectable(venue.Value.name, is_selected))
        {
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