using System.Numerics;

namespace VenueManager.UI; 

public static class Colors {
  public static readonly Vector4 Green = new Vector4(0,1,0,1);
  public static readonly Vector4 White = new Vector4(1,1,1,1);
  public static readonly Vector4 HalfWhite = new Vector4(.5f,.5f,.5f,1);

  // Colors for different entry counts 
  public static readonly Vector4 PlayerEntry2 = new Vector4(.92f,.7f,.35f,1);
  public static readonly Vector4 PlayerEntry3 = new Vector4(.97f,.47f,.1f,1);
  public static readonly Vector4 PlayerEntry4 = new Vector4(.89f,0.01f,0,1);

  public static readonly Vector4 PlayerBlue = new Vector4(.25f,0.65f,0.89f,1);

  public static ushort getChatColor(Player player, bool nameOnly) {
    if (nameOnly) {
      if (player.isFriend) return 526; // Blue 
    }

    if (player.entryCount == 1)
      return 060; // Green. `/xldata` -> UIColor in chat in game 
    else if (player.entryCount == 2)
      return 063;
    else if (player.entryCount == 3)
      return 500;
    else if (player.entryCount >= 4)
      return 518;

    return 003; // default 
  }

  public static Vector4 getGuestListColor(Player player, bool nameOnly) {
    if (nameOnly) {
      if (player.isFriend) return PlayerBlue; // Blue 
    }

    if (player.entryCount == 2)
      return PlayerEntry2;
    else if (player.entryCount == 3)
      return PlayerEntry3;
    else if (player.entryCount >= 4)
      return PlayerEntry4;

    return White; // default 
  }
}