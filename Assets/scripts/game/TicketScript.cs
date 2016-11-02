using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class TicketScript : NetworkBehaviour
{
  [SyncVar]
  public int booth;
  [SyncVar]
  public int number;

  public Text numberText;

  void Update()
  {
    numberText.text = booth + "-" + number;
  }
}
