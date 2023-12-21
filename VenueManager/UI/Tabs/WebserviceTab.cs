using System.Numerics;
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

    ImGui.Spacing();
    ImGui.Spacing();

    // Send data on interval 
    var sendDataOnInterval = plugin.Configuration.webserverConfig.sendDataOnInterval;
    if (ImGui.Checkbox("Send data on interval", ref sendDataOnInterval))
    {
      plugin.Configuration.webserverConfig.sendDataOnInterval = sendDataOnInterval;
      plugin.Configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
      ImGui.SetTooltip("Periodically send guest list to the provided endpoint on the interval set below");
    }

    // Interval 
    var interval = plugin.Configuration.webserverConfig.interval;
    if (ImGui.SliderFloat("Seconds", ref interval, 5, 60))
    {
      plugin.Configuration.webserverConfig.interval = interval;
      plugin.Configuration.Save();
    }

    ImGui.Spacing();
    ImGui.Separator();
    if (RestUtils.failedRequests < RestUtils.maxFailedRequests) {
      ImGui.TextColored(new Vector4(1.0f,0.25f,0.25f,1f), "Interval paused as max failed requests reached");
      ImGui.SameLine();
      if (ImGui.Button("Reset"))
      {
        RestUtils.successfulRequests = 0;
        RestUtils.failedRequests = 0;
      }
    }
    ImGui.TextWrapped($"Successful Requests: {RestUtils.successfulRequests}");
    ImGui.TextWrapped($"Failed Requests: {RestUtils.failedRequests}");
  }
}