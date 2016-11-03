using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerUIScript : MonoBehaviour
{
  public static PlayerUIScript Instance;

  public Text objectives;
  public Text tickets;
  public Image crosshair;

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
    objectives.text = player.quests.ToReadableString();

    tickets.text = player.tickets.ToReadableString();
  }

  public void SetCrosshairColor(Color c)
  {
    crosshair.color = c;
  }
}
