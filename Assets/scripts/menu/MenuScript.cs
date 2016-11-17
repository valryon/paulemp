using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.Networking;

public class MenuScript : MonoBehaviour
{
  #region Constants

  private static readonly Regex ADDRESS = new Regex("^[0-9]{1,3}[.][0-9]{1,3}[.][0-9]{1,3}[.][0-9]{1,3}$");

  #endregion

  #region Members

  [Header("Bindings Server")]
  public GameObject serverPanel;
  public InputField serverPort;
  public Text serverIP;
  public Text serverInternetIP;

  [Header("Bindings Client")]
  public GameObject clientPanel;
  public InputField clientIP;
  public InputField clientPort;

  [Header("Bindings ")]
  public Text status;

  private GameNetworkManager network;
  private int lastServerPort, lastClientPort;

  #endregion

  #region Timeline

  void Start()
  {
    network = FindObjectOfType<GameNetworkManager>();
    network.launchedFromMenu = true;

    status.gameObject.SetActive(false);

    lastServerPort = 7777;
    serverPort.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((s) =>
    {
      int p = 0;
      if (int.TryParse(serverPort.text, out p) == false)
      {
        serverPort.text = lastServerPort.ToString();
      }
      else
      {
        lastServerPort = p;
      }
    }));
    lastClientPort = 7777;
    clientPort.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((s) =>
    {
      int p = 0;
      if (int.TryParse(clientPort.text, out p) == false)
      {
        clientPort.text = lastClientPort.ToString();
      }
      else
      {
        lastClientPort = p;
      }
    }));

    Network.Connect("http://www.google.com");
  }

  void OnEnable()
  {
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;
  }

  void Update()
  {
    serverIP.text = Network.player.ipAddress;
    serverInternetIP.text = Network.player.externalIP;
    if (!Network.player.externalIP.Contains("UNASSIGNED"))
    {
      Network.Disconnect();
    }
  }

  #endregion

  #region Events

  public void LaunchServer()
  {
    int p = 0;
    if (int.TryParse(serverPort.text, out p))
    {
      network.networkPort = p;

      serverPanel.SetActive(false);
      clientPanel.SetActive(false);

      Debug.Log("SERVER starting on localhost:" + p);
      status.text = "Server : démarrage sur localhost:" + p;
      status.gameObject.SetActive(true);

      network.StartHost();
    }
    else
    {
      serverPort.text = "7777";
    }
  }

  public void LaunchClient()
  {
    int p = 0;
    if (int.TryParse(clientPort.text, out p))
    {
      var ip = clientIP.text;
      if (ADDRESS.IsMatch(ip))
      {
        network.networkPort = p;
        network.networkAddress = "::ffff:" + ip;

        serverPanel.SetActive(false);
        clientPanel.SetActive(false);

        Debug.Log("CLIENT starting " + ip + ":" + p);
        status.text = "Client : connexion à " + ip + ":" + p;
        status.gameObject.SetActive(true);

        network.StartClient();
      }
      else
      {
        ip = "127.0.0.1";
      }
    }
    else
    {
      clientPort.text = "7777";
    }
  }

  #endregion
}
