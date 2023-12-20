using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Interface.Internal;
using Dalamud.Plugin.Services;
using ImGuiNET;

namespace VenueManager.Tabs;

public class VenuesTab
{
  private readonly Vector4 colorGreen = new(0, 0.69f, 0, 1);
  private Plugin plugin;

  // Venue name inside input box 
  private string venueName = string.Empty;

  public VenuesTab(Plugin plugin)
  {
    this.plugin = plugin;
  }

  public bool TryLoadIcon(uint iconId, [NotNullWhen(true)] out IDalamudTextureWrap? wrap, bool keepAlive = false)
  {
    wrap = Plugin.TextureProvider.GetIcon(iconId, ITextureProvider.IconFlags.HiRes, null, keepAlive);
    return wrap != null;
  }

  private List<KeyValuePair<long, Venue>> getSortedVenues(ImGuiTableSortSpecsPtr sortSpecs)
  {
    ImGuiTableColumnSortSpecsPtr currentSpecs = sortSpecs.Specs;

    var venues = plugin.venueList.venues.ToList();
    switch (currentSpecs.ColumnIndex)
    {
      case 2: // Name
        if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) venues.Sort((pair1, pair2) => pair2.Value.name.CompareTo(pair1.Value.name));
        else if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) venues.Sort((pair1, pair2) => pair1.Value.name.CompareTo(pair2.Value.name));
        break;
      case 3: // District 
        if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) venues.Sort((pair1, pair2) => pair2.Value.district.CompareTo(pair1.Value.district));
        else if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) venues.Sort((pair1, pair2) => pair1.Value.district.CompareTo(pair2.Value.district));
        break;
      case 7: // World
        if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) venues.Sort((pair1, pair2) => pair2.Value.WorldName.CompareTo(pair1.Value.WorldName));
        else if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) venues.Sort((pair1, pair2) => pair1.Value.WorldName.CompareTo(pair2.Value.WorldName));
        break;
      case 8: // Datacenter 
        if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) venues.Sort((pair1, pair2) => pair2.Value.DataCenter.CompareTo(pair1.Value.DataCenter));
        else if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) venues.Sort((pair1, pair2) => pair1.Value.DataCenter.CompareTo(pair2.Value.DataCenter));
        break;
      default:
        break;
    }

    return venues;
  }

  // Draw venue list menu 
  public unsafe void draw()
  {
    if (!plugin.pluginState.userInHouse) ImGui.BeginDisabled();
    // Copy current location to clipboard 
    if (ImGui.Button("Copy Current Address"))
    {
      ImGui.SetClipboardText(plugin.pluginState.currentHouse.getVenueAddress());
    }
    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) && !plugin.pluginState.userInHouse)
    {
      ImGui.SetTooltip("You must be in a house");
    }
    if (!plugin.pluginState.userInHouse) ImGui.EndDisabled();
    ImGui.Separator();
    ImGui.Spacing();

    ImGui.Text("Save the current venue you are in to the list of venues");
    ImGui.InputTextWithHint("", "Enter venue name", ref venueName, 256);
    ImGui.SameLine();
    // Only allow saving venue if name is entered, user is in a house, and current house id is not in list 
    bool canAdd = venueName.Length > 0 &&
      plugin.pluginState.userInHouse &&
      !plugin.venueList.venues.ContainsKey(plugin.pluginState.currentHouse.houseId);
    if (!canAdd) ImGui.BeginDisabled();
    if (ImGui.Button("Save Venue"))
    {
      // Save venue to saved venue list 
      Venue venue = new Venue(plugin.pluginState.currentHouse);
      venue.name = venueName;
      plugin.venueList.venues.Add(venue.houseId, venue);
      plugin.venueList.save();
      // Add a new guest list to the main registry for this venue 
      GuestList guestList = new GuestList(venue.houseId, venue);
      plugin.guestLists.Add(venue.houseId, guestList);
    }
    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
    {
      if (!plugin.pluginState.userInHouse)
        ImGui.SetTooltip("You are not in a house");
      else if (plugin.venueList.venues.ContainsKey(plugin.pluginState.currentHouse.houseId))
        ImGui.SetTooltip("Current venue already saved as " + plugin.venueList.venues[plugin.pluginState.currentHouse.houseId].name);
      else if (venueName.Length == 0)
        ImGui.SetTooltip("You must enter a name");
    }
    if (!canAdd) ImGui.EndDisabled();

    ImGui.Spacing();
    if (ImGui.BeginTable("Venues", 10, ImGuiTableFlags.Sortable))
    {
      ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 20);
      ImGui.TableSetupColumn("", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 20);
      ImGui.TableSetupColumn("Name");
      ImGui.TableSetupColumn("District");
      ImGui.TableSetupColumn("Ward", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 40);
      ImGui.TableSetupColumn("Plot", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 40);
      ImGui.TableSetupColumn("Room", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoSort, 40);
      ImGui.TableSetupColumn("World");
      ImGui.TableSetupColumn("DataCenter");
      ImGui.TableSetupColumn("Delete", ImGuiTableColumnFlags.NoSort);
      ImGui.TableHeadersRow();

      ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();
      var venues = getSortedVenues(sortSpecs);

      foreach (var venue in venues)
      {
        var fontColor = plugin.pluginState.userInHouse && plugin.pluginState.currentHouse.houseId == venue.Value.houseId ?
          colorGreen : new Vector4(1, 1, 1, 1);

        ImGui.TableNextColumn();
        if (ImGuiComponents.IconButton("##Copy" + venue.Value.houseId, FontAwesomeIcon.Copy))
        {
          ImGui.SetClipboardText(venue.Value.getVenueAddress());
        }
        if (ImGui.IsItemHovered())
        {
          ImGui.SetTooltip("Copy Address");
        }
        ImGui.TableNextColumn();
        if (TryLoadIcon(TerritoryUtils.getHouseIcon(venue.Value.type), out var iconHandle))
          ImGui.Image(iconHandle.ImGuiHandle, new Vector2(ImGui.GetFrameHeight()));
        else
          ImGui.Dummy(new Vector2(ImGui.GetFrameHeight()));
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
        ImGui.TextColored(fontColor, venue.Value.DataCenter);

        ImGui.TableNextColumn();
        bool disabled = false;
        if (!ImGui.IsKeyDown(ImGuiKey.LeftCtrl) && !ImGui.IsKeyDown(ImGuiKey.RightCtrl))
        {
          ImGui.BeginDisabled();
          disabled = true;
        }

        // Allow the user to delete the saved venue
        if (ImGuiComponents.IconButton("##" + venue.Value.houseId, FontAwesomeIcon.Trash))
        {
          plugin.venueList.venues.Remove(venue.Value.houseId);
          plugin.venueList.save();
        }
        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) && disabled)
        {
          ImGui.SetTooltip("Hold control to delete");
        }
        if (disabled) ImGui.EndDisabled();
      }

      ImGui.EndTable();
    }
  }
}