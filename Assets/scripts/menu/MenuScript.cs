using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text.RegularExpressions;

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

      Debug.Log("Starting Server on localhost:" + p);
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
     // if (ADDRESS.IsMatch(ip))
     // {
        network.networkPort = p;
        network.networkAddress = ip;

        serverPanel.SetActive(false);
        clientPanel.SetActive(false);

        Debug.Log("Starting client " + ip + ":" + p);
        status.text = "Client : connexion à " + ip + ":" + p;
        status.gameObject.SetActive(true);

        network.StartClient();
      //}
      //else
      //{
      //  ip = "127.0.0.1";
      //}
    }
    else
    {
      clientPort.text = "7777";
    }
  }

  #endregion
}
