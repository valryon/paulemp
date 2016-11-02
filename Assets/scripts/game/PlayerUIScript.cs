using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerUIScript : MonoBehaviour
{
  public static PlayerUIScript Instance;

  public Text objectives;
  public Text tickets;

  public PlayerScript player;
  
  void Awake()
  {
    Instance = this;
  }

  void Start()
  {
    gameObject.SetActive(false);
  }

  void Update()
  {
    objectives.text = "TODO";

    tickets.text = player.tickets.ToReadableString();
  }
}
