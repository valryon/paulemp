using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public struct TicketData
{
  public string name;
  public int booth;
  public int number;
}

public class TicketList : SyncListStruct<TicketData>
{
  public string ToReadableString()
  {
    string t = string.Empty;

    foreach (var ticket in this)
    {
      t += ticket.name + " " + ticket.number + "\n";
    }

    return t;
  }

  public int GetFor(int boothId)
  {
    foreach (var ticket in this)
    {
      if (ticket.booth == boothId)
      {
        return ticket.number;
      }
    }
    return -1;
  }

  public void RemoveFor(int boothId)
  {
    foreach (var ticket in this)
    {
      if (ticket.booth == boothId)
      {
        Remove(ticket);
        break;
      }
    }
  }
}

public class TicketScript : NetworkBehaviour
{
  private static float TimeToLive = 120f;

  [Header("Bindings")]
  public Text numberText;
  public Text boothText;

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
    numberText.text = data.number.ToString();
    boothText.text = data.name;

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
