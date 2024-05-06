using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.ImGuiFileDialog;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using Lumina.Data.Files;
using VenueManager.UI;

namespace VenueManager.Widgets;

public class GuestListWidget
{
  private Plugin plugin;
  private readonly FileDialogManager fileDialog = new();
  private bool drawSaveDialog = false;
  private static unsafe string GetUserPath() => Framework.Instance()->UserPath;
  private bool simpleFormat = true;
  public bool showDownloadButtons {get; set;} = false;
  private string filter = "";

  public GuestListWidget(Plugin plugin)
  {
    this.plugin = plugin;
  }

  // Draw venue list menu 
  public unsafe void draw(long houseId)
  {
    if (drawSaveDialog) fileDialog.Draw();

    // Ensure we have this guest list 
    if (!plugin.guestLists.ContainsKey(houseId)) {
      if (plugin.venueList.venues.ContainsKey(houseId)) {
        GuestList guestList = new GuestList(houseId, plugin.venueList.venues[houseId]);
        guestList.load();
        plugin.guestLists.Add(houseId, guestList);
      }
      // A house Id has been sent to be rendered that is not saved. This is invalid. 
      else {
        Plugin.Log.Warning("Can't render guest list for: " + houseId);
        return;
      }
    }

    drawOptions(houseId);
    drawGuestTable(houseId);
  }

  private void drawOptions(long houseId) {
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
    if (showDownloadButtons) {
      ImGui.SameLine();
      // Allow the user to save the log to a file 
      if (ImGui.Button("Save Json")) {
        var startPath = GetUserPath();
        if (startPath.Length == 0)
          startPath = null;
        // Setup the save dialog 
        fileDialog.SaveFileDialog("Save File...", ".json", "VenueGuestLog.json", ".json", (confirm, path) => {
          if (confirm && plugin.guestLists.ContainsKey(houseId)) {
            plugin.guestLists[houseId].saveToFile(path);
          }
          drawSaveDialog = false;
        }, startPath);
        drawSaveDialog = true;
      }
      ImGui.SameLine();
      if (ImGui.Button("Save CSV")) {
        var startPath = GetUserPath();
        if (startPath.Length == 0)
          startPath = null;
        // Setup the save dialog 
        fileDialog.SaveFileDialog("Save File...", ".csv", "VenueGuestLog.csv", ".csv", (confirm, path) => {
          if (confirm && plugin.guestLists.ContainsKey(houseId)) {
            plugin.guestLists[houseId].saveToFileCSV(path, plugin.pluginState.currentHouse.houseId == houseId);
          }
          drawSaveDialog = false;
        }, startPath);
        drawSaveDialog = true;
      }
    }
    // Simple or advanced table format 
    ImGui.Checkbox("Simple View", ref simpleFormat);

    // Table filter 
    ImGui.SameLine();
    ImGui.PushItemWidth(200);
    ImGui.InputTextWithHint($"##filter", "Filter Name", ref filter, 256);
  }

  private List<KeyValuePair<string, Player>> getSortedGuests(ImGuiTableSortSpecsPtr sortSpecs, long houseId)
  {
    ImGuiTableColumnSortSpecsPtr currentSpecs = sortSpecs.Specs;

    var guestList = plugin.guestLists[houseId].guests.ToList();
    bool isCurrentHouse = plugin.pluginState.currentHouse.houseId == houseId;

    // Filter down if string provided
    if (filter.Length > 0) {
      guestList = guestList.Where(item => item.Value.Name.ToLower().Contains(filter.ToLower())).ToList();
    }

    guestList.Sort((pair1, pair2) => {
      // Filter friends to top 
      if (plugin.Configuration.sortFriendsToTop && pair1.Value.isFriend != pair2.Value.isFriend && 
        ((plugin.Configuration.sortCurrentVisitorsTop && pair1.Value.inHouse == pair2.Value.inHouse) || !plugin.Configuration.sortCurrentVisitorsTop)) {
        return pair2.Value.isFriend.CompareTo(pair1.Value.isFriend);
      } 
      // Filter in house to top 
      else if (plugin.Configuration.sortCurrentVisitorsTop && pair1.Value.inHouse != pair2.Value.inHouse) {
        return pair2.Value.inHouse.CompareTo(pair1.Value.inHouse);
      } 
      // Other general sorts 
      else {
        switch (currentSpecs.ColumnIndex)
        {
          case 0: // Latest Entry
            if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) return pair2.Value.latestEntry.CompareTo(pair1.Value.latestEntry);
            else if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) return pair1.Value.latestEntry.CompareTo(pair2.Value.latestEntry);
            break;
          case 1: // Name
            if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) return pair2.Value.Name.CompareTo(pair1.Value.Name);
            else if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) return pair1.Value.Name.CompareTo(pair2.Value.Name);
            break;
          case 2: // Entry Count
            if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) return pair2.Value.entryCount.CompareTo(pair1.Value.entryCount);
            else if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) return pair1.Value.entryCount.CompareTo(pair2.Value.entryCount);
            break;
          case 3: // Minutes Inside
            if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) return pair2.Value.milisecondsInVenue.CompareTo(pair1.Value.milisecondsInVenue);
            else if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) return pair1.Value.milisecondsInVenue.CompareTo(pair2.Value.milisecondsInVenue);
            break;
          case 4: // First Seen
            if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) return pair2.Value.firstSeen.CompareTo(pair1.Value.firstSeen);
            else if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) return pair1.Value.firstSeen.CompareTo(pair2.Value.firstSeen);
            break;
          case 5: // Last Seen
            if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) return pair2.Value.lastSeen.CompareTo(pair1.Value.lastSeen);
            else if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) return pair1.Value.lastSeen.CompareTo(pair2.Value.lastSeen);
            break;
          case 6: // Last Seen
            if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) return pair2.Value.WorldName.CompareTo(pair1.Value.WorldName);
            else if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) return pair1.Value.WorldName.CompareTo(pair2.Value.WorldName);
            break;
          default:
            break;
        }
      }
      return 0;
    });

    return guestList;
  }

  private void drawGuestTable(long houseId) {
    ImGui.BeginChild(1);

    if (ImGui.BeginTable("Guests", simpleFormat ? 2 : 7, ImGuiTableFlags.Sortable))
    {
      ImGui.TableSetupColumn("Latest Entry", ImGuiTableColumnFlags.DefaultSort);
      ImGui.TableSetupColumn("Name");
      if (!simpleFormat) {
        ImGui.TableSetupColumn("Entries", ImGuiTableColumnFlags.WidthFixed, 60);
        ImGui.TableSetupColumn("Minutes", ImGuiTableColumnFlags.WidthFixed, 60);
        ImGui.TableSetupColumn("First Seen");
        ImGui.TableSetupColumn("Last Seen");
        ImGui.TableSetupColumn("World");
      }
      ImGui.TableHeadersRow();

      ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();
      var sortedGuestList = getSortedGuests(sortSpecs, houseId);

      foreach (var player in sortedGuestList)
      {
        var playerColor = Colors.getGuestListColor(player.Value, true);
        var color = Colors.getGuestListColor(player.Value, false);

        if (!player.Value.inHouse && plugin.pluginState.currentHouse.houseId == houseId) {
          color[3] = .5f;
          playerColor[3] = .5f;
        }

        ImGui.TableNextColumn();
        ImGui.TextColored(color, player.Value.latestEntry.ToString("h:mm tt"));
        ImGui.TableNextColumn();
        ImGui.TextColored(playerColor, player.Value.Name);
        if (ImGui.IsItemClicked()) {
          plugin.chatPlayerLink(player.Value);
        }

        if (!simpleFormat) {
          ImGui.TableNextColumn();
          ImGui.TextColored(color, "" + player.Value.entryCount);
          ImGui.TableNextColumn();
          ImGui.TextColored(color, "" + player.Value.getTimeInVenue(plugin.pluginState.currentHouse.houseId == houseId));
          ImGui.TableNextColumn();
          ImGui.TextColored(color, player.Value.firstSeen.ToString("h:mm tt"));
          ImGui.TableNextColumn();
          ImGui.TextColored(color, player.Value.lastSeen.ToString("h:mm tt"));
          ImGui.TableNextColumn();
          ImGui.TextColored(color, player.Value.WorldName);
        }
      }

      ImGui.EndTable();
    }

    ImGui.EndChild();
  }
}