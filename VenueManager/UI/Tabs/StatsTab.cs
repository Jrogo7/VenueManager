using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using ImPlotNET;

namespace VenueManager.Tabs;

public class StatsTab
{
  private Plugin plugin;

  public StatsTab(Plugin plugin)
  {
    this.plugin = plugin;
  }

  // Draw venue list menu 
  public unsafe void draw()
  {
    this.drawHelpInfo();
    this.drawTimeGraph();
  }

  public void drawHelpInfo() {
    if (plugin.pluginState.userInHouse)
    {
      if (plugin.venueList.venues.ContainsKey(plugin.pluginState.currentHouse.houseId))
      {
        var venue = plugin.venueList.venues[plugin.pluginState.currentHouse.houseId];
        ImGui.Text($"{plugin.pluginState.playersInHouse} Players at " + venue.name);
      }
      else
      {
        ImGui.Text($"{plugin.pluginState.playersInHouse} Players at untracked house");
      }
    }
    else if (plugin.pluginState.isTrackingOutside) {
      ImGui.Text($"{plugin.pluginState.playersInHouse} Players at outdoor event");
    } 
    else
    {
      ImGui.Text("You are not tracking right now");      
      if (ImGui.Button("Track outdoor event?")) {
        plugin.pluginState.isTrackingOutside = true;
        plugin.startTimers();
      }
    }
    ImGui.Text("(Note: This only shows players you have loaded, the area can contain more sometimes)");
  }

  float t = 0;
  float historySize = 300.0f;
  Stack<float> dataStackTime = new Stack<float>(300);
  Stack<float> dataStackPlayerCount = new Stack<float>(300);

  public void drawTimeGraph() {
    t += ImGui.GetIO().DeltaTime;
    dataStackTime.Push(t);
    dataStackPlayerCount.Push(plugin.currentVisitorCount);

    if (ImPlot.BeginPlot("Players in Area", new Vector2(-1,150))) {
      ImPlot.SetupAxes("Time","Players", ImPlotAxisFlags.NoTickLabels | ImPlotAxisFlags.NoLabel, ImPlotAxisFlags.NoLabel);
      ImPlot.SetupAxisLimits(ImAxis.X1, t - historySize, t, ImPlotCond.Always);
      ImPlot.SetupAxisLimits(ImAxis.Y1, 0, 120, ImPlotCond.Always);
      ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
      ImPlot.PlotShaded("Players", ref dataStackTime.ToArray()[0], ref dataStackPlayerCount.ToArray()[0], dataStackTime.ToArray().Length, 0, ImPlotShadedFlags.None);
      ImPlot.PlotLine("Players", ref dataStackTime.ToArray()[0], ref dataStackPlayerCount.ToArray()[0],  dataStackTime.ToArray().Length, 0);
      ImPlot.EndPlot();

    }
  }
}