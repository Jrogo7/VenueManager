using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Bindings.ImGui;

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
    ImGui.BeginChild(1);
    ImGui.TextWrapped("The below configuration is used to sync the guest log for the current house you are in to the designated server endpoint provided below.");
    ImGui.TextWrapped("You should only use this tab if you know what you are doing.");
    ImGui.Separator();
    ImGui.Spacing();

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

    // Headers
    drawHeaders();

    ImGui.Spacing();
    ImGui.Spacing();

    // Send users that left 
    var sendUsersThatLeft = plugin.Configuration.webserverConfig.sendUsersThatLeft;
    if (ImGui.Checkbox("Send users that left", ref sendUsersThatLeft))
    {
      plugin.Configuration.webserverConfig.sendUsersThatLeft = sendUsersThatLeft;
      plugin.Configuration.Save();
    }
    if (ImGui.IsItemHovered())
    {
      ImGui.SetTooltip("Send user data for users that are no longer detected in the area");
    }

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
    ImGui.Spacing();

    var disableSend = !(plugin.pluginState.userInHouse || plugin.pluginState.isTrackingOutside) || plugin.Configuration.webserverConfig.endpoint.Length == 0;
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
      else if (!plugin.pluginState.userInHouse && !plugin.pluginState.isTrackingOutside) {
        ImGui.SetTooltip("You must be in a house or enable outside events to send current guest log");
      }
    }
    if (disableSend) ImGui.EndDisabled();

    // Error for failed requests
    if (RestUtils.failedRequests > RestUtils.maxFailedRequests) {
      ImGui.TextColored(new Vector4(1.0f,0.25f,0.25f,1f), "Interval paused as max failed requests reached");
      ImGui.SameLine();
      if (ImGui.Button("Reset"))
      {
        RestUtils.successfulRequests = 0;
        RestUtils.failedRequests = 0;
      }
    }
    ImGui.TextWrapped("Last request sent at: " + (RestUtils.lastTimeSentSet ? RestUtils.lastTimeSent.ToString("MM/dd h:mm tt") : "-"));
    ImGui.TextWrapped($"Successful Requests: {RestUtils.successfulRequests}");
    ImGui.TextWrapped($"Failed Requests: {RestUtils.failedRequests}");

    ImGui.EndChild();
  } // End Draw 

  private void drawHeaders() {
    ImGui.Spacing();
    if (ImGui.Button("Add Header"))
    {
      plugin.Configuration.webserverConfig.headers.Add(new HeaderPair());
      plugin.Configuration.Save();
    }
    ImGui.SameLine();
    ImGui.TextWrapped("Note: Headers are stored in plain text.");

    int itemToRemove = -1;

    for (var i = 0; i < plugin.Configuration.webserverConfig.headers.Count; i++) {
      ImGui.PushItemWidth(200);
      var key = plugin.Configuration.webserverConfig.headers[i].key;
      ImGui.InputTextWithHint($"##headerkey{i}", "Key", ref key, 100);
      ImGui.SameLine();
      var value = plugin.Configuration.webserverConfig.headers[i].value;
      ImGui.InputTextWithHint($"##headervalue{i}", "Value", ref value, 100);
      ImGui.PopItemWidth();

      if (key != plugin.Configuration.webserverConfig.headers[i].key || value != plugin.Configuration.webserverConfig.headers[i].value) {
        plugin.Configuration.webserverConfig.headers[i].key = key;
        plugin.Configuration.webserverConfig.headers[i].value = value;
        plugin.Configuration.Save();
        RestUtils.headersChanged = true;
      }

      ImGui.SameLine();
      if (ImGuiComponents.IconButton($"##headerdelete{i}", FontAwesomeIcon.Trash))
      {
        itemToRemove = i;
      }
    }

    if (itemToRemove != -1) {
      plugin.Configuration.webserverConfig.headers.Remove(plugin.Configuration.webserverConfig.headers[itemToRemove]);
      plugin.Configuration.Save();
      RestUtils.headersChanged = true;
    }
  }
}