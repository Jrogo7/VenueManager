using System;
using System.Numerics;
using Dalamud.Interface.Internal;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace ClubManager.Windows;

public class MainWindow : Window, IDisposable
{
    private Plugin plugin;
    private Configuration configuration;

    public MainWindow(Plugin plugin) : base(
        "Club Manager", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(300, 300),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.plugin = plugin;
        this.configuration = plugin.Configuration;
    }

    public void Dispose()
    {
    }

    public override void Draw()
    {
        ImGui.BeginTabBar("Tabs");
        // Render Guests tab if selected 
        if (ImGui.BeginTabItem("Guests")) {
          ImGui.Text(this.configuration.userInHouse ? 
            "You are in a " + TerritoryUtils.getHouseType(this.configuration.territory) + " in " + TerritoryUtils.getHouseLocation(this.configuration.territory) : 
            "You are not in a house.");
          ImGui.Text("Guests:");
          ImGui.Indent(55);
          foreach (var guest in this.configuration.guests) {
            ImGui.Text(guest.Value.Name);
          }
          ImGui.Unindent(55);

          ImGui.EndTabItem();
        }
        // Render Settings Tab if selected 
        if (ImGui.BeginTabItem("Settings")) {
          ImGui.Text($"The random config bool is {this.plugin.Configuration.SomePropertyToBeSavedAndWithADefault}");

          if (ImGui.Button("Show Settings"))
          {
              this.plugin.DrawConfigUI();
          }

          ImGui.EndTabItem();
        }
        ImGui.EndTabBar();
    }
}
