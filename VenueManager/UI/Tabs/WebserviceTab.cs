using ImGuiNET;

namespace VenueManager.Tabs;

public class WebserviceTab
{
  private Plugin plugin;
  // Endpoint in the input box
  private string endpointUrl = string.Empty;

  public WebserviceTab(Plugin plugin)
  {
    this.plugin = plugin;
    endpointUrl = this.plugin.Configuration.webserverConfig.endpoint;
  }

  public unsafe void draw()
  {
    ImGui.TextWrapped("The below configuration is used to sync the guest log for the current house you are in to the designated server endpoint provided below.");
    ImGui.TextWrapped("You should only use this tab if you know what you are doing.");
    ImGui.Separator();
    
    // TODO headers Mention that headers are not encrypted 

    // Endpoing Url section 
    ImGui.Text("Endpoint:");
    ImGui.Text("POST");
    if (ImGui.IsItemHovered())
      ImGui.SetTooltip("Request will be sent via POST");
    ImGui.SameLine();
    ImGui.InputTextWithHint("", "https://example.com/guestlist", ref endpointUrl, 256);
    ImGui.SameLine();
    bool canAdd = endpointUrl.Length > 0;
    if (!canAdd) ImGui.BeginDisabled();
    if (ImGui.Button("Save Endpoint"))
    {
      plugin.Configuration.webserverConfig.endpoint = endpointUrl;
      plugin.Configuration.Save();
    }
    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
    {
      if (endpointUrl.Length == 0)
        ImGui.SetTooltip("Please enter a Url");
    }
    if (!canAdd) ImGui.EndDisabled();

    var disableSend = !plugin.pluginState.userInHouse || plugin.Configuration.webserverConfig.endpoint.Length == 0;
    if (disableSend) ImGui.BeginDisabled();
    // Send the guest list now to the server
    if (ImGui.Button("Send Now"))
    {
      plugin.getCurrentGuestList().sentToWebserver(plugin);
    }
    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled) && !plugin.pluginState.userInHouse)
    {
      if (plugin.Configuration.webserverConfig.endpoint.Length == 0) {
        ImGui.SetTooltip("You must enter an endpoint to POST to");
      }
      else if (!plugin.pluginState.userInHouse) {
        ImGui.SetTooltip("You must be in a house to send current guest log");
      }
    }
    if (disableSend) ImGui.EndDisabled();

    // TODO status information 
    // TODO send on interval checkmark 
    // TODO interval timing 
  }
}