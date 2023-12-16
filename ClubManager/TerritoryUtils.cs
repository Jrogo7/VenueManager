using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;

namespace ClubManager
{
    internal class TerritoryUtils
    {
        // Found at : https://github.com/SoyaX/Doorbell/blob/main/Plugin.cs#L39
        private static readonly ushort[] HouseTerritoryIds = {
            // Small, Medium, Large, Chamber, Apartment
            282, 283, 284, 384, 608, // Mist
            342, 343, 344, 385, 609, // The Lavender Beds
            345, 346, 347, 386, 610, // The Goblet
            649, 650, 651, 652, 655, // Shirogane
            980, 981, 982, 983, 999, // Empyreum 
        };

        private static Dictionary<ushort, string> HouseTypeMap = new Dictionary<ushort, string>(){
          {282, "Small House"}, {342, "Small House"}, {345, "Small House"}, {649, "Small House"}, {980, "Small House"},
          {283, "Medium House"}, {343, "Medium House"}, {346, "Medium House"}, {650, "Medium House"}, {981, "Medium House"},
          {284, "Large House"}, {344, "Large House"}, {347, "Large House"}, {651, "Large House"}, {982, "Large House"},
          {384, "Chamber"}, {385, "Chamber"}, {386, "Chamber"}, {652, "Chamber"}, {983, "Chamber"},
          {608, "Apartment"}, {609, "Apartment"}, {610, "Apartment"}, {655, "Apartment"}, {999, "Apartment"},
        };
        private static Dictionary<ushort, string> HouseLocationMap = new Dictionary<ushort, string>(){
          {282, "Mist"}, {342, "The Lavender Beds"}, {345, "The Goblet"}, {649, "Shirogane"}, {980, "Empyreum"},
          {283, "Mist"}, {343, "The Lavender Beds"}, {346, "The Goblet"}, {650, "Shirogane"}, {981, "Empyreum"},
          {284, "Mist"}, {344, "The Lavender Beds"}, {347, "The Goblet"}, {651, "Shirogane"}, {982, "Empyreum"},
          {384, "Mist"}, {385, "The Lavender Beds"}, {386, "The Goblet"}, {652, "Shirogane"}, {983, "Empyreum"},
          {608, "Mist"}, {609, "The Lavender Beds"}, {610, "The Goblet"}, {655, "Shirogane"}, {999, "Empyreum"},
        };

        // Returns true if sent territory id is a house 
        static public bool isHouse(ushort territory)
        {
            return HouseTerritoryIds.Contains(territory);
        }

        static public string getHouseType(ushort territory) {
          return HouseTypeMap.ContainsKey(territory) ? HouseTypeMap[territory] : "";
        }

        static public string getHouseLocation(ushort territory) {
          return HouseLocationMap.ContainsKey(territory) ? HouseLocationMap[territory] : "";
        }
    }
}
