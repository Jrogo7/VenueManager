using System;
using Sheet = Lumina.Excel.Sheets;
using System.Linq;

namespace VenueManager
{
  internal class TerritoryUtils
  {
    // Mist locations 
    public static readonly ushort MIST_SMALL = 282;
    public static readonly ushort MIST_MEDIUM = 283;
    public static readonly ushort MIST_LARGE = 284;
    public static readonly ushort MIST_CHAMBER = 384;
    public static readonly ushort MIST_APARTMENT = 608;

    // The Lavender Beds locations 
    public static readonly ushort LAVENDER_SMALL = 342;
    public static readonly ushort LAVENDER_MEDIUM = 343;
    public static readonly ushort LAVENDER_LARGE = 344;
    public static readonly ushort LAVENDER_CHAMBER = 385;
    public static readonly ushort LAVENDER_APARTMENT = 609;

    // The Goblet
    public static readonly ushort GOBLET_SMALL = 345;
    public static readonly ushort GOBLET_MEDIUM = 346;
    public static readonly ushort GOBLET_LARGE = 347;
    public static readonly ushort GOBLET_LARGE_2 = 1251;
    public static readonly ushort GOBLET_CHAMBER = 386;
    public static readonly ushort GOBLET_APARTMENT = 610;

    // Shirogane 
    public static readonly ushort SHIROGANE_SMALL = 649;
    public static readonly ushort SHIROGANE_MEDIUM = 650;
    public static readonly ushort SHIROGANE_LARGE = 651;
    public static readonly ushort SHIROGANE_CHAMBER = 652;
    public static readonly ushort SHIROGANE_APARTMENT = 655;

    // Empyreum 
    public static readonly ushort EMPYREUM_SMALL = 980;
    public static readonly ushort EMPYREUM_MEDIUM = 981;
    public static readonly ushort EMPYREUM_LARGE = 982;
    public static readonly ushort EMPYREUM_CHAMBER = 983;
    public static readonly ushort EMPYREUM_APARTMENT = 999;

    private static readonly ushort[] HouseTerritoryIds = {
      MIST_SMALL, MIST_MEDIUM, MIST_LARGE, MIST_CHAMBER, MIST_APARTMENT,
      LAVENDER_SMALL, LAVENDER_MEDIUM, LAVENDER_LARGE, LAVENDER_CHAMBER, LAVENDER_APARTMENT, 
      GOBLET_SMALL, GOBLET_MEDIUM, GOBLET_LARGE, GOBLET_LARGE_2, GOBLET_CHAMBER, GOBLET_APARTMENT,
      SHIROGANE_SMALL, SHIROGANE_MEDIUM, SHIROGANE_LARGE, SHIROGANE_CHAMBER, SHIROGANE_APARTMENT,
      EMPYREUM_SMALL, EMPYREUM_MEDIUM, EMPYREUM_LARGE, EMPYREUM_CHAMBER, EMPYREUM_APARTMENT, 
    };

    private static readonly ushort[] ChambrerTerritoryIds = {
      MIST_CHAMBER, LAVENDER_CHAMBER, GOBLET_CHAMBER, SHIROGANE_CHAMBER, EMPYREUM_CHAMBER, 
    };

    private static readonly ushort[] PlotTerritoryIds = {
      MIST_SMALL, MIST_MEDIUM, MIST_LARGE,
      LAVENDER_SMALL, LAVENDER_MEDIUM, LAVENDER_LARGE,
      GOBLET_SMALL, GOBLET_MEDIUM, GOBLET_LARGE, GOBLET_LARGE_2,
      SHIROGANE_SMALL, SHIROGANE_MEDIUM, SHIROGANE_LARGE,
      EMPYREUM_SMALL, EMPYREUM_MEDIUM, EMPYREUM_LARGE, 
    };

    private static readonly ushort[] SmallHouseTypes = {
      MIST_SMALL, LAVENDER_SMALL, GOBLET_SMALL, SHIROGANE_SMALL, EMPYREUM_SMALL,
    };

    private static readonly ushort[] MediumHouseTypes = {
      MIST_MEDIUM, LAVENDER_MEDIUM, GOBLET_MEDIUM, SHIROGANE_MEDIUM, EMPYREUM_MEDIUM
    };

    private static readonly ushort[] LargeHouseTypes = {
      MIST_LARGE, LAVENDER_LARGE, GOBLET_LARGE, GOBLET_LARGE_2, SHIROGANE_LARGE, EMPYREUM_LARGE
    };

    private static readonly ushort[] ChamberTypes = {
      MIST_CHAMBER, LAVENDER_CHAMBER, GOBLET_CHAMBER, SHIROGANE_CHAMBER, EMPYREUM_CHAMBER
    };

    private static readonly ushort[] AppartmentTypes = {
      MIST_APARTMENT, LAVENDER_APARTMENT, GOBLET_APARTMENT, SHIROGANE_APARTMENT, EMPYREUM_APARTMENT
    };

    private static readonly ushort[] MistHouses = {
      MIST_SMALL, MIST_MEDIUM, MIST_LARGE, MIST_CHAMBER, MIST_APARTMENT
    };

    private static readonly ushort[] LavenderHouses = {
      LAVENDER_SMALL, LAVENDER_MEDIUM, LAVENDER_LARGE, LAVENDER_CHAMBER, LAVENDER_APARTMENT
    };

    private static readonly ushort[] GobletHouses = {
      GOBLET_SMALL, GOBLET_MEDIUM, GOBLET_LARGE, GOBLET_LARGE_2, GOBLET_CHAMBER, GOBLET_APARTMENT
    };

    private static readonly ushort[] ShiroganeHouses = {
      SHIROGANE_SMALL, SHIROGANE_MEDIUM, SHIROGANE_LARGE, SHIROGANE_CHAMBER, SHIROGANE_APARTMENT
    };

    private static readonly ushort[] EmpyreumHouses = {
      EMPYREUM_SMALL, EMPYREUM_MEDIUM, EMPYREUM_LARGE, EMPYREUM_CHAMBER, EMPYREUM_APARTMENT
    };

    private static readonly uint SmallHouseIcon = 60751;
    private static readonly uint MediumHouseIcon = 60752;
    private static readonly uint LargeHouseIcon = 60753;
    private static readonly uint AppartmentHouseIcon = 60789;

    // Returns true if sent territory id is a house 
    static public bool isHouse(ushort territory)
    {
      return HouseTerritoryIds.Contains(territory);
    }

    static public string getHouseType(ushort territory)
    {
      if (SmallHouseTypes.Contains(territory)) return "Small House";
      if (MediumHouseTypes.Contains(territory)) return "Medium House";
      if (LargeHouseTypes.Contains(territory)) return "Large House";
      if (ChamberTypes.Contains(territory)) return "Chamber";
      if (AppartmentTypes.Contains(territory)) return "Apartment";
      return "[unknown house type]";
    }

    static public string getHouseDistrict(ushort territory)
    {
      if (MistHouses.Contains(territory)) return "Mist";
      if (LavenderHouses.Contains(territory)) return "The Lavender Beds";
      if (GobletHouses.Contains(territory)) return "The Goblet";
      if (ShiroganeHouses.Contains(territory)) return "Shirogane";
      if (EmpyreumHouses.Contains(territory)) return "Empyreum";
      return "[unknown district]";
    }

    static public bool isChamber(ushort territory)
    {
      return ChambrerTerritoryIds.Contains(territory);
    }

    static public bool isPlotType(ushort territory)
    {
      return PlotTerritoryIds.Contains(territory);
    }

    public static uint getHouseIcon(ushort territory)
    {
      if (SmallHouseTypes.Contains(territory)) return SmallHouseIcon;
      if (MediumHouseTypes.Contains(territory)) return MediumHouseIcon;
      if (LargeHouseTypes.Contains(territory)) return LargeHouseIcon;
      if (ChamberTypes.Contains(territory)) return LargeHouseIcon;
      if (AppartmentTypes.Contains(territory)) return AppartmentHouseIcon;
      return 0;
    }

    public static string getDistrict (long houseId) {
      uint territoryId = (uint)((houseId >> 32) & 0xFFFF);
      var district = Plugin.DataManager.GetExcelSheet<Sheet.TerritoryType>().GetRow(territoryId).PlaceNameZone.RowId;

      switch (district) {
        case 502: 
          return "Mist";
        case 505: 
          return "Goblet";
        case 507:
          return "Lavender Beds";
        case 512: 
          return "Empyreum";
        case 513: 
          return "Shirogane";
        default: 
          return "";
      }
    }
  }
}
