using System;
using System.Linq.Expressions;
using System.Numerics;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using VenueManager.Tabs;

namespace VenueManager.Windows;

public class MainWindow : Window, IDisposable
{
  private Configuration configuration;

  private VenuesTab venuesTab;
  private SettingsTab settingsTab;
  private GuestsTab guestsTab;
  private GuestLogTab guestLogTab;
  private WebserviceTab webserviceTab;

  public MainWindow(Plugin plugin) : base(
      "Venue Manager", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
  {
    this.SizeConstraints = new WindowSizeConstraints
    {
      MinimumSize = new Vector2(250, 300),
      MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
    };

    this.configuration = plugin.Configuration;
    this.venuesTab = new VenuesTab(plugin);
    this.settingsTab = new SettingsTab(plugin);
    this.guestsTab = new GuestsTab(plugin);
    this.guestLogTab = new GuestLogTab(plugin);
    this.webserviceTab = new WebserviceTab(plugin);
  }

  public void Dispose()
  {
  }

  public override void Draw()
  {
    try
    {
      ImGui.BeginTabBar("Tabs");
      // Render Guests tab if selected 
      if (this.configuration.showGuestsTab)
      {
        if (ImGui.BeginTabItem("Guests"))
        {
          this.guestsTab.draw();

          ImGui.EndTabItem();
        }
        if (ImGui.BeginTabItem("Logs"))
        {
          this.guestLogTab.draw();

          ImGui.EndTabItem();
        }
        if (this.configuration.showWebserviceLogging)
        {
          if (ImGui.BeginTabItem("Webserver Logging"))
          {
            this.webserviceTab.draw();

            ImGui.EndTabItem();
          }
        }
      }
      // Render Venues Tab 
      if (this.configuration.showVenueTab)
      {
        if (ImGui.BeginTabItem("Venues"))
        {
          venuesTab.draw();
          ImGui.EndTabItem();
        }
      }
      // Render Settings Tab if selected 
      if (ImGui.BeginTabItem("Settings"))
      {
        this.settingsTab.draw();
        ImGui.EndTabItem();
      }
      ImGui.EndTabBar();
    }
    catch (Exception e)
    {
      Plugin.Log.Error("Crash while drawing main window");
      Plugin.Log.Error(e.ToString());
    }
  }
}
