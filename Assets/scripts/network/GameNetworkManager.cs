using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GameNetworkManager : NetworkManager
{
  [HideInInspector]
  public bool launchedFromMenu;

  public override void OnClientConnect(NetworkConnection conn)
  {
    Debug.Log("CLIENT connected from " + conn.address);
    if (NetworkServer.active)
    {
      base.OnClientConnect(conn);
    }

    GameServer.PlaySound("player_connect", this.transform.position);
  }

  //public override void OnClientDisconnect(NetworkConnection conn)
  //{
  //  Debug.Log("CLIENT disconnected from " + conn.address);
  //  base.OnClientDisconnect(conn);

  //  GameServer.PlaySound("player_disconnect", this.transform.position);
  //}

  public override void OnServerReady(NetworkConnection conn)
  {
    Debug.Log("SERVER ready! IP " + conn.address);
    base.OnServerReady(conn);

    var gameServer = FindObjectOfType<GameServer>();
    gameServer.CreateLevelAndStuff();
    
    PlayerScript.HasGeneratedLevel = true;
  }

  public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
  {
    base.OnServerAddPlayer(conn, playerControllerId);

    Debug.Log("SERVER creating player");

    //GameObject playerObj = GameObject.Instantiate(playerPrefab);
    //playerObj.transform.position = GetStartPosition().position;
    //NetworkServer.Spawn(playerObj);
    //NetworkServer.AddPlayerForConnection(conn, playerObj, 0);

    var gameServer = FindObjectOfType<GameServer>();
    gameServer.RequestLevelCreation();
  }
}
