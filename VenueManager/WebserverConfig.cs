using System;

namespace VenueManager
{
  [Serializable]
  public class HeaderPair
  {
    public string key { get; set; } = "";
    public string value { get; set; } = "";

    public HeaderPair() { }
  }

  [Serializable]
  public class WebserverConfig
  {
    public string endpoint { get; set; } = "";
    public HeaderPair[] headers { get; set; } = [];

    public WebserverConfig() { }
  }
}
