using UnityEngine;

public class DebugStartMenu : MonoBehaviour
{
  private const int DEFAULT_PORT = 6666;
  private const string DEFAULT_ADRESS = "127.0.0.1";

  private GameNetworkManager networkManager;
  private string adress;
  private int port;
  private string playerName;

  void Start()
  {
    networkManager = FindObjectOfType<GameNetworkManager>();

    // From Menu?
    if (networkManager.launchedFromMenu)
    {
      Destroy(this);
      return;
    }

    // Default values
    adress = DEFAULT_ADRESS;
    port = DEFAULT_PORT;
#if UNITY_STANDALONE
    playerName = System.Environment.UserName;
#else
      playerName = "Player";
#endif
  }

  private void ConfigureNetworkManager()
  {
    networkManager.networkAddress = adress;
    networkManager.networkPort = port;
  }

  /// <summary>
  /// Server+client start (useful in editor)
  /// </summary>
  public void StartHost()
  {
    networkManager.StartHost();
  }

  /// <summary>
  /// Default player start
  /// </summary>
  /// <param name="ip"></param>
  public void StartClient()
  {
    networkManager.StartClient();
  }

  /// <summary>
  /// Temp UI
  /// </summary>
  void OnGUI()
  {
    if (networkManager != null && networkManager.isNetworkActive == false)
    {
      Cursor.visible = true;
      Cursor.lockState = CursorLockMode.None;

      int middleX = Screen.width / 2;
      int middleY = Screen.height / 2;

      playerName = GUI.TextField(new Rect(middleX - 80, middleY - 170, 160, 30), playerName.ToString());

      // Simple Start UI
      if (GUI.Button(new Rect(middleX - 80, middleY - 30 - 40, 160, 60), "Host"))
      {
        StartHost();
      }
      if (GUI.Button(new Rect(middleX - 80, middleY - 30 + 40, 160, 60), "Join"))
      {
        StartClient();
      }

      GUI.Label(new Rect(middleX - 80, middleY - 30 + 140, 40, 30), "IP: ");
      adress = GUI.TextField(new Rect(middleX - 40, middleY - 30 + 140, 120, 30), adress);

      GUI.Label(new Rect(middleX - 80, middleY - 30 + 180, 40, 30), "port: ");
      string p = GUI.TextField(new Rect(middleX - 40, middleY - 30 + 180, 120, 30), port.ToString());

      int newPort = DEFAULT_PORT;
      if (int.TryParse(p, out newPort))
      {
        port = newPort;
      }
    }
  }
}