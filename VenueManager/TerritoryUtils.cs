using System;
using System.Collections.Generic;
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
      GOBLET_SMALL, GOBLET_MEDIUM, GOBLET_LARGE, GOBLET_CHAMBER, GOBLET_APARTMENT,
      SHIROGANE_SMALL, SHIROGANE_MEDIUM, SHIROGANE_LARGE, SHIROGANE_CHAMBER, SHIROGANE_APARTMENT,
      EMPYREUM_SMALL, EMPYREUM_MEDIUM, EMPYREUM_LARGE, EMPYREUM_CHAMBER, EMPYREUM_APARTMENT, 
    };

    private static readonly ushort[] ChambrerTerritoryIds = {
      MIST_CHAMBER, LAVENDER_CHAMBER, GOBLET_CHAMBER, SHIROGANE_CHAMBER, EMPYREUM_CHAMBER, 
    };

     private static readonly ushort[] PlotTerritoryIds = {
      MIST_SMALL, MIST_MEDIUM, MIST_LARGE,
      LAVENDER_SMALL, LAVENDER_MEDIUM, LAVENDER_LARGE,
      GOBLET_SMALL, GOBLET_MEDIUM, GOBLET_LARGE,
      SHIROGANE_SMALL, SHIROGANE_MEDIUM, SHIROGANE_LARGE,
      EMPYREUM_SMALL, EMPYREUM_MEDIUM, EMPYREUM_LARGE, 
    };

    private static Dictionary<ushort, string> HouseTypeMap = new Dictionary<ushort, string>(){
      {MIST_SMALL, "Small House"}, {LAVENDER_SMALL, "Small House"}, {GOBLET_SMALL, "Small House"}, {SHIROGANE_SMALL, "Small House"}, {EMPYREUM_SMALL, "Small House"},
      {MIST_MEDIUM, "Medium House"}, {LAVENDER_MEDIUM, "Medium House"}, {GOBLET_MEDIUM, "Medium House"}, {SHIROGANE_MEDIUM, "Medium House"}, {EMPYREUM_MEDIUM, "Medium House"},
      {MIST_LARGE, "Large House"}, {LAVENDER_LARGE, "Large House"}, {GOBLET_LARGE, "Large House"}, {SHIROGANE_LARGE, "Large House"}, {EMPYREUM_LARGE, "Large House"},
      {MIST_CHAMBER, "Chamber"}, {LAVENDER_CHAMBER, "Chamber"}, {GOBLET_CHAMBER, "Chamber"}, {SHIROGANE_CHAMBER, "Chamber"}, {EMPYREUM_CHAMBER, "Chamber"},
      {MIST_APARTMENT, "Apartment"}, {LAVENDER_APARTMENT, "Apartment"}, {GOBLET_APARTMENT, "Apartment"}, {SHIROGANE_APARTMENT, "Apartment"}, {EMPYREUM_APARTMENT, "Apartment"},
    };

    private static Dictionary<ushort, string> HouseDistrictMap = new Dictionary<ushort, string>(){
      {MIST_SMALL, "Mist"}, {LAVENDER_SMALL, "The Lavender Beds"}, {GOBLET_SMALL, "The Goblet"}, {SHIROGANE_SMALL, "Shirogane"}, {EMPYREUM_SMALL, "Empyreum"},
      {MIST_MEDIUM, "Mist"}, {LAVENDER_MEDIUM, "The Lavender Beds"}, {GOBLET_MEDIUM, "The Goblet"}, {SHIROGANE_MEDIUM, "Shirogane"}, {EMPYREUM_MEDIUM, "Empyreum"},
      {MIST_LARGE, "Mist"}, {LAVENDER_LARGE, "The Lavender Beds"}, {GOBLET_LARGE, "The Goblet"}, {SHIROGANE_LARGE, "Shirogane"}, {EMPYREUM_LARGE, "Empyreum"},
      {MIST_CHAMBER, "Mist"}, {LAVENDER_CHAMBER, "The Lavender Beds"}, {GOBLET_CHAMBER, "The Goblet"}, {SHIROGANE_CHAMBER, "Shirogane"}, {EMPYREUM_CHAMBER, "Empyreum"},
      {MIST_APARTMENT, "Mist"}, {LAVENDER_APARTMENT, "The Lavender Beds"}, {GOBLET_APARTMENT, "The Goblet"}, {SHIROGANE_APARTMENT, "Shirogane"}, {EMPYREUM_APARTMENT, "Empyreum"},
    };

    private static readonly uint SmallHouseIcon = 60751;
    private static readonly uint MediumHouseIcon = 60752;
    private static readonly uint LargeHouseIcon = 60753;
    private static readonly uint AppartmentHouseIcon = 60789;
    private static Dictionary<ushort, uint> HouseIconMap = new Dictionary<ushort, uint>(){
      {MIST_SMALL, SmallHouseIcon}, {LAVENDER_SMALL, SmallHouseIcon}, {GOBLET_SMALL, SmallHouseIcon}, {SHIROGANE_SMALL, SmallHouseIcon}, {EMPYREUM_SMALL, SmallHouseIcon},
      {MIST_MEDIUM, MediumHouseIcon}, {LAVENDER_MEDIUM, MediumHouseIcon}, {GOBLET_MEDIUM, MediumHouseIcon}, {SHIROGANE_MEDIUM, MediumHouseIcon}, {EMPYREUM_MEDIUM, MediumHouseIcon},
      {MIST_LARGE, LargeHouseIcon}, {LAVENDER_LARGE, LargeHouseIcon}, {GOBLET_LARGE, LargeHouseIcon}, {SHIROGANE_LARGE, LargeHouseIcon}, {EMPYREUM_LARGE, LargeHouseIcon},
      {MIST_CHAMBER, LargeHouseIcon}, {LAVENDER_CHAMBER, LargeHouseIcon}, {GOBLET_CHAMBER, LargeHouseIcon}, {SHIROGANE_CHAMBER, LargeHouseIcon}, {EMPYREUM_CHAMBER, LargeHouseIcon},
      {MIST_APARTMENT, AppartmentHouseIcon}, {LAVENDER_APARTMENT, AppartmentHouseIcon}, {GOBLET_APARTMENT, AppartmentHouseIcon}, {SHIROGANE_APARTMENT, AppartmentHouseIcon}, {EMPYREUM_APARTMENT, AppartmentHouseIcon},
    };

    // Returns true if sent territory id is a house 
    static public bool isHouse(ushort territory)
    {
      return HouseTerritoryIds.Contains(territory);
    }

    static public string getHouseType(ushort territory)
    {
      return HouseTypeMap.ContainsKey(territory) ? HouseTypeMap[territory] : "[unknown house type]";
    }

    static public string getHouseDistrict(ushort territory)
    {
      return HouseDistrictMap.ContainsKey(territory) ? HouseDistrictMap[territory] : "[unknown district]";
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
      return HouseIconMap.ContainsKey(territory) ? HouseIconMap[territory] : 0;
    }
  }
}
