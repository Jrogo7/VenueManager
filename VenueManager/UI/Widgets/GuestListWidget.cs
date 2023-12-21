using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.ImGuiFileDialog;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;
using VenueManager.UI;

namespace VenueManager.Widgets;

public class GuestListWidget
{
  private Plugin plugin;
  private readonly FileDialogManager fileDialog = new();
  private bool drawSaveDialog = false;
  private static unsafe string GetUserPath() => Framework.Instance()->UserPath;

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
    ImGui.SameLine();
    // Allow the user to save the log to a file 
    if (ImGui.Button("Save List")) {
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
  }

  private List<KeyValuePair<string, Player>> getSortedGuests(ImGuiTableSortSpecsPtr sortSpecs, long houseId)
  {
    ImGuiTableColumnSortSpecsPtr currentSpecs = sortSpecs.Specs;

    var guestList = plugin.guestLists[houseId].guests.ToList();

    switch (currentSpecs.ColumnIndex)
    {
      case 0: // Latest Entry
        if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) guestList.Sort((pair1, pair2) => pair2.Value.latestEntry.CompareTo(pair1.Value.latestEntry));
        else if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) guestList.Sort((pair1, pair2) => pair1.Value.latestEntry.CompareTo(pair2.Value.latestEntry));
        break;
      case 1: // Name
        if (currentSpecs.SortDirection == ImGuiSortDirection.Ascending) guestList.Sort((pair1, pair2) => pair2.Value.Name.CompareTo(pair1.Value.Name));
        else if (currentSpecs.SortDirection == ImGuiSortDirection.Descending) guestList.Sort((pair1, pair2) => pair1.Value.Name.CompareTo(pair2.Value.Name));
        break;
      default:
        break;
    }

    return guestList;
  }

  private void drawGuestTable(long houseId) {
    ImGui.BeginChild(1);

    if (ImGui.BeginTable("Guests", 2, ImGuiTableFlags.Sortable))
    {
      ImGui.TableSetupColumn("Latest Entry", ImGuiTableColumnFlags.DefaultSort);
      ImGui.TableSetupColumn("Name");
      ImGui.TableHeadersRow();

      ImGuiTableSortSpecsPtr sortSpecs = ImGui.TableGetSortSpecs();
      var sortedGuestList = getSortedGuests(sortSpecs, houseId);

      foreach (var player in sortedGuestList)
      {
        var color = player.Value.inHouse ? Colors.White : Colors.HalfWhite;

        ImGui.TableNextColumn();
        ImGui.TextColored(color, player.Value.latestEntry.ToString("h:mm tt"));
        ImGui.TableNextColumn();
        ImGui.TextColored(color, player.Value.Name);
      }

      ImGui.EndTable();
    }

    ImGui.EndChild();
  }
}