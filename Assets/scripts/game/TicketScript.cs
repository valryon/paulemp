using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public struct TicketData
{
  public int booth;
  public int number;
  public System.DateTime printDate;
}

public class TicketList : SyncListStruct<TicketData>
{

}

public class TicketScript : NetworkBehaviour
{
  private static float TimeToLive = 120f;

  [Header("Bindings")]
  public Text numberText;

  [Header("Data")]
  [SyncVar]
  public TicketData data;

  [SyncVar]
  public bool picked;

  private float ttl;

  void Start()
  {
    ttl = TimeToLive;
    picked = false;
  }

  void Update()
  {
    ttl -= Time.deltaTime;
    numberText.text = data.booth + "-" + data.number;

    if (ttl < 0f)
    {
      Destroy();
    }
  }

  void Interact(PlayerScript p)
  {
    if (picked == false)
    {
      p.RequestPickTicket(this);
    }
  }

  [Server]
  public void Destroy()
  {
    NetworkServer.Destroy(this.gameObject);
  }

  [Server]
  public void Recycle(TicketData recycleData, Vector3 position, Vector3 dir)
  {
    ttl = TimeToLive;
    picked = false;

    transform.position = position;
    data = recycleData;

    Rigidbody r = GetComponent<Rigidbody>();
    r.AddForce(new Vector3(150f * dir.x, -15f, 150f * dir.z));
  }
}
