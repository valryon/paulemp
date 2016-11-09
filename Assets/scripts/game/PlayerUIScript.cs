using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerUIScript : MonoBehaviour
{
  public static PlayerUIScript Instance;

  public Text objectives;
  public Text tickets;
  public Image crosshair;
  public Text elapsedTime;

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
    if (player == null) return;

    objectives.text = player.quests.ToReadableString();

    tickets.text = player.tickets.ToReadableString();

    var t = System.TimeSpan.FromSeconds(player.elapsedTime);
    string time = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                    t.Hours,
                    t.Minutes,
                    t.Seconds);
    elapsedTime.text = time;
  }

  public void SetCrosshairColor(Color c)
  {
    crosshair.color = c;
  }
}
