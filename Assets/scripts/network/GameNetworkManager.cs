using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GameNetworkManager : NetworkManager
{
  [HideInInspector]
  public bool launchedFromMenu;

  private GameServer gameServer;

  void Awake()
  {
  }

  public override void OnClientConnect(NetworkConnection conn)
  {
    if (NetworkServer.active)
    {
      base.OnClientConnect(conn);
    }

    GameServer.PlaySound("player_connect", this.transform.position);
  }

  public override void OnClientDisconnect(NetworkConnection conn)
  {
    base.OnClientDisconnect(conn);

    GameServer.PlaySound("player_disconnect", this.transform.position);
  }

  public override void OnServerReady(NetworkConnection conn)
  {
    base.OnServerReady(conn);

    var gameServer = FindObjectOfType<GameServer>();
    gameServer.CreateLevelAndStuff();
  }

  public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
  {
    base.OnServerAddPlayer(conn, playerControllerId);
    
    var gameServer = FindObjectOfType<GameServer>();
    gameServer.RequestLevelCreation();
  }
}
