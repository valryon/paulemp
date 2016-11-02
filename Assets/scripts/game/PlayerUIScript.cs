using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerUIScript : MonoBehaviour
{
  public Text objectives;
  public Text tickets;

  public PlayerScript player;
  
  void Update()
  {
    if (player == null) return;

    objectives.text = "TODO";

    tickets.text = player.tickets.ToReadableString();
  }
}
