using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class AgentScript : NetworkBehaviour
{
  [SyncVar]
  public GameObject booth;

  private BoothScript boothScript;

  void Interact(PlayerScript p)
  {
    if (boothScript == null)
    {
      if (booth != null)
      {
        boothScript = booth.GetComponent<BoothScript>();
      }
    }

    p.RequestCheckTicket(boothScript);
  }
}
