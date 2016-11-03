using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GameNetworkManager : NetworkManager
{
  [HideInInspector]
  public bool launchedFromMenu;

  public override void OnClientConnect(NetworkConnection conn)
  {
    base.OnClientConnect(conn);

    GameServer.PlaySound("player_connect", this.transform.position);
  }

  public override void OnClientDisconnect(NetworkConnection conn)
  {
    base.OnClientDisconnect(conn);

    GameServer.PlaySound("player_disconnect", this.transform.position);
  }
}
