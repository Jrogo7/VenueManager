using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;

namespace VenueManager
{
  public enum DOORBELL_TYPE {
    DOORBELL = 0, 
    RECEPTION_BELL,
  };

  
  public class DoorbellSound
  {
    public static readonly string[] DoorbellSoundTypes = {
      "Standard Doorbell",
      "Reception Bell"
    };
    
    private string file = "";
    private WaveStream? audioFileReader;
    private WaveOutEvent? wavEvent;

    private Plugin plugin;

    public DoorbellSound(Plugin plugin, DOORBELL_TYPE type) {
      this.plugin = plugin;
      setType(type);
    }

    public void setType(DOORBELL_TYPE type) {
      if (type == DOORBELL_TYPE.DOORBELL) file = "doorbell_home.wav";
      if (type == DOORBELL_TYPE.RECEPTION_BELL) file = "reception_bell.wav";
    }

    public void load() {
      disposeFile();
      try {
        var fileToLoad = Path.Join(Plugin.PluginInterface.AssemblyLocation.Directory!.FullName, file);
        
        if (!File.Exists(fileToLoad)) {
            Plugin.Log.Warning($"{file} does not exist.");
            return;
        }

        audioFileReader = new AudioFileReader(fileToLoad);
        (audioFileReader as AudioFileReader)!.Volume = plugin.Configuration.soundVolume;
        wavEvent = new WaveOutEvent();
        wavEvent.Init(audioFileReader);
      } catch (Exception ex) {
          Plugin.Log.Error(ex, "Error loading sound file " + file);
      }
    }

    public void play() {
      Task.Run(() => {
        if (audioFileReader == null || wavEvent == null) load();
        if (audioFileReader == null || wavEvent == null) return;
        wavEvent.Stop();
        audioFileReader.Position = 0;
        wavEvent.Play();
      });
    }

    public void disposeFile() {
      audioFileReader?.Dispose();
      wavEvent?.Dispose();

      audioFileReader = null;
      wavEvent = null;
    }
  }
}
