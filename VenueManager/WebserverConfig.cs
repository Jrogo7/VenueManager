using System;
using System.Collections.Generic;

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
    public List<HeaderPair> headers { get; set; } = new List<HeaderPair>();
    public bool sendDataOnInterval { get; set; } = false;
    // Send users that no longer are in the area over webserver
    public bool sendUsersThatLeft { get; set; } = false;
    public float interval { get; set; } = 5;
    public int IntervalMiliseconds => (int)(interval * 1000.0f);

    public WebserverConfig() { }
  }
}
