using Newtonsoft.Json;
using System;
using UnityEngine;

[Serializable]
public class ZeroMQConfig
{
  private static ZeroMQConfig _instance;
  private static readonly object _lock = new object();

  [JsonProperty("method")]
  public string Method { get; set; }

  [JsonProperty("publish_ip")]
  public string PublishIP { get; set; }

  [JsonProperty("publish_port")]
  public int PublishPort { get; set; }

  [JsonProperty("listen_ip")]
  public string ListenIP { get; set; }

  [JsonProperty("listen_port")]
  public int ListenPort { get; set; }

  private ZeroMQConfig() { }

  public static ZeroMQConfig Instance
  {
    get
    {
      lock (_lock)
      {
        if (_instance == null)
        {
          LoadConfig();
        }
        return _instance;
      }
    }
  }

  public static void LoadConfig()
  {
    try
    {
      TextAsset configText = Resources.Load<TextAsset>("config");
      if (configText != null)
      {
        _instance = JsonConvert.DeserializeObject<ZeroMQConfig>(configText.text);
      }
      else
      {
        Debug.LogError("ZeroMQConfig file not found in Resources.");
      }
    }
    catch (Exception ex)
    {
      Debug.LogError($"Error loading config: {ex.Message}");
    }
  }
}
