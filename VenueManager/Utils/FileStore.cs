using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using Dalamud.Utility;
using Newtonsoft.Json;

namespace VenueManager
{
  public class FileStore
  {
    public static void SaveStringToFile(string path, string output) 
    {
      try
      {
        FilesystemUtil.WriteAllTextSafe(path, output);
      }
      catch (Exception exception)
      {
        Plugin.Log.Error("Failed to save file: " + exception.ToString());
      }
    }

    public static void SaveClassToFile(string path, Type fileType, object objectData)
    {
      try
      {
        string output = JsonConvert.SerializeObject(objectData, fileType, new JsonSerializerSettings { Formatting = Formatting.Indented });
        FilesystemUtil.WriteAllTextSafe(path, output);
      }
      catch (Exception exception)
      {
        Plugin.Log.Error("Failed to save file: " + exception.ToString());
      }
    }

    public static void SaveClassToFileInPluginDir(string fileName, Type fileType, object objectData)
    {
      var fileInfo = GetFileInfo(fileName);
      SaveClassToFile(fileInfo.FullName, fileType, objectData);
    }

    public static T LoadFile<T>(string filePath, object targetObject)
    {
      if (LoadFile(filePath, targetObject.GetType(), out var loadedData))
      {
        return (T)loadedData;
      }

      SaveClassToFileInPluginDir(filePath, targetObject.GetType(), targetObject);
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
