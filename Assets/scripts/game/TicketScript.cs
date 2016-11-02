using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TicketScript : NetworkBehaviour
{
  [Header("Bindings")]
  public Text numberText;

  [Header("Data")]
  [SyncVar]
  public int booth;
  [SyncVar]
  public int number;
  [SyncVar]
  public bool picked;

  private float ttl;

  void Start()
  {
    ttl = 120f;
    picked = false;
  }

  void Update()
  {
    ttl -= Time.deltaTime;
    numberText.text = booth + "-" + number;

    if(ttl < 0f)
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

  public void Destroy()
  {
    NetworkServer.Destroy(this.gameObject);
  }
}
