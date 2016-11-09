using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerUIScript : MonoBehaviour
{
  #region Members

  public static PlayerUIScript Instance;

  [Header("HUD")]
  public GameObject hudPanel;
  public Text objectives;
  public Text tickets;
  public Image crosshair;
  public Text elapsedTime;

  [Header("Game Over")]
  public GameObject gameOverPanel;
  public Text elapsedTimeFinal;

  #endregion

  #region Timeline

  void Awake()
  {
    Instance = this;
    hudPanel.SetActive(false);
    gameOverPanel.SetActive(false);
  }

  void Start()
  {

  }

  void Update()
  {
    if (Player == null) return;

    if (hudPanel.activeInHierarchy)
    {
      objectives.text = Player.quests.ToReadableString();

      tickets.text = Player.tickets.ToReadableString();

      var t = System.TimeSpan.FromSeconds(Player.elapsedTime);
      string time = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                      t.Hours,
                      t.Minutes,
                      t.Seconds);
      elapsedTime.text = time;
    }
  }

  #endregion

  #region HUD

  public void ShowHUD()
  {
    hudPanel.SetActive(true);
  }

  public void SetCrosshairColor(Color c)
  {
    crosshair.color = c;
  }

  #endregion

  #region Game Over

  public void ShowGameOver(float elapsedTime)
  {
    hudPanel.SetActive(false);
    gameOverPanel.SetActive(true);

    var t = System.TimeSpan.FromSeconds(elapsedTime);
    string time = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                    t.Hours,
                    t.Minutes,
                    t.Seconds);
    elapsedTimeFinal.text = time;
  }

  public void EndGame()
  {
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;

    DebugStartMenu menu = FindObjectOfType<DebugStartMenu>();
    if (menu != null)
    {
      Destroy(menu);
    }

    GameNetworkManager network = FindObjectOfType<GameNetworkManager>();
    
    network.StopClient();
    network.StopHost();
  }

  #endregion

  #region Properties

  public PlayerScript Player
  {
    get; set;
  }

  #endregion

}
