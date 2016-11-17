﻿using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class GameNetworkManager : NetworkManager
{
  [HideInInspector]
  public bool launchedFromMenu;

  public override void OnClientConnect(NetworkConnection conn)
  {
    base.OnClientConnect(conn);
    Debug.Log("CLIENT connected to " + conn.address);

    GameServer.PlaySound("player_connect", this.transform.position);
  }

  public override void OnClientDisconnect(NetworkConnection conn)
  {
    base.OnClientDisconnect(conn);
    Debug.Log("CLIENT disconnected from " + conn.address);
  }

  public override void OnServerReady(NetworkConnection conn)
  {
    base.OnServerReady(conn);

    // This shit is called every time a client connects
    if(PlayerScript.HasGeneratedLevel == false)
    {
      Debug.Log("SERVER started!");

      var gameServer = FindObjectOfType<GameServer>();
      gameServer.CreateLevelAndStuff();

      PlayerScript.HasGeneratedLevel = true;
    }
  }

  public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId)
  {
    base.OnServerAddPlayer(conn, playerControllerId);

    Debug.Log("SERVER creating player");

    var gameServer = FindObjectOfType<GameServer>();
    gameServer.RequestLevelCreation();
  }

  public override void OnServerConnect(NetworkConnection conn)
  {
    base.OnServerConnect(conn);

    Debug.Log("SERVER connection from " + conn.address);

    GameServer.PlaySound("player_connect", this.transform.position);
  }

  public override void OnServerDisconnect(NetworkConnection conn)
  {
    base.OnServerDisconnect(conn);
    Debug.Log("SERVER disconnected from " + conn.address);

    GameServer.PlaySound("player_disconnect", this.transform.position);

    if(conn.playerControllers.Count > 0)
    {
      NetworkServer.Destroy(conn.playerControllers[0].gameObject);
    }
  }
}
