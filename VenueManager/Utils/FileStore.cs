using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dalamud.Utility;
using Lumina.Excel.GeneratedSheets;
using Newtonsoft.Json;

namespace VenueManager
{
  public class FileStore
  {
    public static void SaveFile(string fileName, Type fileType, object objectData)
    {
      try
      {
        string output = JsonConvert.SerializeObject(objectData, fileType, new JsonSerializerSettings { Formatting = Formatting.Indented });
        var fileInfo = GetFileInfo(fileName);
        Util.WriteAllTextSafe(fileInfo.FullName, output);
      }
      catch (Exception exception)
      {
        Plugin.Log.Error("Failed to save file: " + exception.ToString());
      }
    }

    public static T LoadFile<T>(string filePath, object targetObject)
    {
      if (LoadFile(filePath, targetObject.GetType(), out var loadedData))
      {
        return (T)loadedData;
      }

      SaveFile(filePath, targetObject.GetType(), targetObject);
      return (T)targetObject;
    }

    private static bool LoadFile(string fileName, Type fileType, [NotNullWhen(true)] out object? loadedData)
    {
      try
      {
        var fileInfo = GetFileInfo(fileName);

        if (fileInfo is { Exists: false })
        {
          loadedData = null;
          return false;
        }

        var jsonString = File.ReadAllText(fileInfo.FullName);
        loadedData = JsonConvert.DeserializeObject(jsonString, fileType)!;
        return true;
      }
      catch (Exception exception)
      {
        Plugin.Log.Error("Error loading file " + fileName + "." + exception.ToString());
        loadedData = null;
        return false;
      }
    }

    public static FileInfo GetFileInfo(string fileName)
    {
      var configDirectory = Plugin.PluginInterface.ConfigDirectory;
      return new FileInfo(Path.Combine(configDirectory.FullName, fileName));
    }
  }
}
