using System;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace VenueManager.Windows;

public class NotesWindow : Window, IDisposable
{
    public Venue venue {get;set;} = new Venue();
    private Plugin plugin;

    public NotesWindow(Plugin plugin) : base("Notes", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoScrollbar)
    {
        this.SizeConstraints = new WindowSizeConstraints
        {
          MinimumSize = new Vector2(415, 160),
          MaximumSize = new Vector2(415, 160)
        };
      
        this.plugin = plugin;
    }

    public void Dispose() { }

    public override void Draw()
    {
        // Render Notes 
        var notes = venue.notes;
        ImGui.Text($"Write any notes you would like for {venue.name} (notes autosave)");
        ImGui.InputTextMultiline("", ref notes, 256, new Vector2(400.0f,100.0f));
        if (notes != venue.notes) {
          plugin.venueList.venues[venue.houseId].notes = notes;
          plugin.venueList.save();
        }
    }
}