using Dalamud.Bindings.ImGui;
using VenueManager.Widgets;

namespace VenueManager.Tabs;

public class GuestLogTab
{
  private Plugin plugin;
  private GuestListWidget guestListWidget;

  // Current venue selected for logs 
  private long selectVenue = 0;

  public GuestLogTab(Plugin plugin)
  {
    this.plugin = plugin;
    this.guestListWidget = new GuestListWidget(plugin);
    this.guestListWidget.showDownloadButtons = true;
  }

  public unsafe void draw()
  {
    string displayName = "Outdoor Event";
    if (plugin.venueList.venues.ContainsKey(selectVenue))
    {
      displayName = plugin.venueList.venues[selectVenue].name;
    }

    ImGui.Text("Select venue to display guest log for:");
    // Combo box of all venues 
    if (ImGui.BeginCombo("##VenueForLogs", displayName))
    {
      // Outside Venue 
      bool is_selected = 1 == selectVenue;
      if (ImGui.Selectable("Outdoor Event", is_selected))
      {
        selectVenue = 1;
      }
      if (is_selected)
        ImGui.SetItemDefaultFocus();
        
      // All Saved Venues 
      foreach (var venue in plugin.venueList.venues)
      {
        is_selected = venue.Key == selectVenue;
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

    if (selectVenue != 0)
      this.guestListWidget.draw(selectVenue);
    else 
      ImGui.Text("Please select a venue");
  }
}