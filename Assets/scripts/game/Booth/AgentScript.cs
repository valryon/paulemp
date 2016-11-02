using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AgentScript : NetworkBehaviour
{
  [SyncVar]
  public GameObject booth;

  void Interact(PlayerScript p)
  {

  }
}
