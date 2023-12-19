using System.Linq;
using System.Numerics;
using Dalamud.Interface.ImGuiFileDialog;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using ImGuiNET;

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
        GuestList guestList = new GuestList(houseId, "");
        guestList.load();
        plugin.guestLists.Add(houseId, guestList);
      }
      // A house Id has been sent to be rendered that is not saved. This is invalid. 
      else {
        Plugin.Log.Warning("Can't render guest list for: " + houseId);
        return;
      }
    }

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
    if (ImGui.Button("Save Log")) {
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

    // Draw Guests 
    ImGui.Text($"Guests ({plugin.guestLists[houseId].guests.Count})");
    ImGui.BeginChild(1);
    ImGui.Indent(10);

    // Generate sorted guest list 
    var sortedGuestList = plugin.guestLists[houseId].guests.ToList();
    sortedGuestList.Sort((pair1,pair2) => pair2.Value.firstSeen.CompareTo(pair1.Value.firstSeen));
    foreach (var guest in sortedGuestList) {
      var color = guest.Value.inHouse ? new Vector4(1,1,1,1) : new Vector4(.5f,.5f,.5f,1);
      ImGui.TextColored(color, guest.Value.firstSeen.ToString("hh:mm") + " - " + guest.Value.Name);
    }
    ImGui.Unindent(10);
    ImGui.EndChild();
  }
}