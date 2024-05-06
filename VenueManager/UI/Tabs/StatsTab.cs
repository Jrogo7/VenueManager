using System.Linq;
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
    this.drawPopGraph();
  }

  public void drawHelpInfo() {
    if (plugin.pluginState.userInHouse)
    {
      if (plugin.venueList.venues.ContainsKey(plugin.pluginState.currentHouse.houseId))
      {
        var venue = plugin.venueList.venues[plugin.pluginState.currentHouse.houseId];
        ImGui.Text("Players at " + venue.name);
      }
      else
      {
        ImGui.Text("Players at untracked house");
      }
    }
    else if (plugin.pluginState.isTrackingOutside) {
      ImGui.Text("Players at outdoor event");
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

  public void drawPopGraph() {
    double[] xs1 = new double[Plugin.MAX_POP_EVENTS], ys1 = new double[Plugin.MAX_POP_EVENTS];
    for (int i = 0; i < Plugin.MAX_POP_EVENTS; i++) {
      
      xs1[i] = (float)i;
      if (i >= plugin.populationEvents.Count) 
        ys1[i] = 0;
      else
        ys1[i] = plugin.populationEvents.ElementAt(i).playerCount;
    }

    if (ImPlot.BeginPlot($"Players in Area ({plugin.pluginState.playersInHouse})")) {
      ImPlot.SetupAxes("Time","Players", ImPlotAxisFlags.NoTickLabels | ImPlotAxisFlags.NoLabel, ImPlotAxisFlags.NoLabel);
      ImPlot.SetupAxesLimits(0,Plugin.MAX_POP_EVENTS-1,0,110);
      
      // Fills 
      ImPlot.PushStyleVar(ImPlotStyleVar.FillAlpha, 0.25f);
      ImPlot.PlotShaded("Players", ref xs1[0], ref ys1[0], Plugin.MAX_POP_EVENTS, 0, ImPlotShadedFlags.None);
      ImPlot.PopStyleVar();
      
      // Lines 
      ImPlot.PlotLine("Players", ref xs1[0], ref ys1[0], Plugin.MAX_POP_EVENTS);

      ImPlot.EndPlot();
    }
  }
}