using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PlayerUIScript : MonoBehaviour
{
  private const float ARM_MIN_SCALE = 0.75f;
  private const float ARM_MAX_SCALE = 2f;

  private const float ARM_MIN_OFFSET = -100;
  private const float ARM_MAX_OFFSET = -20;


  #region Members

  public static PlayerUIScript Instance;

  [Header("HUD")]
  public GameObject hudPanel;
  public Text objectives;
  public Text ticketName;
  public Text ticketNumber;
  public Image crosshair;
  public Text elapsedTime;
  public GameObject arm;
  public GameObject armLeft;

  [Header("Game Over")]
  public GameObject gameOverPanel;
  public Text elapsedTimeFinal;
  
  private bool zoomed;
  private float zoom;
  private float zoomTarget;

  private float offset;
  private float offsetTarget;

  private Vector3 originalArmPos;


  private Vector3 originalLeftArmPos;
  private Vector3 originalLeftArmGlobalPos;
  private float offsetLeft;
  private float offsetTargetLeft;

  #endregion

  #region Timeline

  void Awake()
  {
    Instance = this;
    hudPanel.SetActive(false);
    gameOverPanel.SetActive(false);
    
    zoom = ARM_MIN_SCALE;
    zoomTarget = zoom;
    offset = ARM_MIN_OFFSET;
    offsetTarget = ARM_MAX_OFFSET;

    originalArmPos = arm.transform.localPosition;
    originalLeftArmPos = armLeft.transform.localPosition;
    originalLeftArmGlobalPos = armLeft.transform.position;
    
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
        if (Player.hasTicket()) {
         var ti = Player.getCurrentTicket();
         ticketName.text = ti.name;
         ticketNumber.text = ti.number.ToString();
         armLeft.transform.position = originalLeftArmGlobalPos;
        }
        else
        {
            ticketName.text = ticketNumber.text = "";
            armLeft.transform.position = Vector3.one * 999999;
        }


        var t = System.TimeSpan.FromSeconds(Player.elapsedTime);
        string time = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                        t.Hours,
                        t.Minutes,
                        t.Seconds);
        elapsedTime.text = time;

        zoom = Mathf.Lerp(zoom, zoomTarget, Time.deltaTime);
        arm.transform.localScale = Vector3.one * zoom;
        

        offset = Mathf.Lerp(offset, offsetTarget, Time.deltaTime * 8);
        if (Mathf.Abs(offset - ARM_MAX_OFFSET) < 0.1) offsetTarget = ARM_MIN_OFFSET;
        if (Mathf.Abs(offset - ARM_MIN_OFFSET) < 0.1) offsetTarget = ARM_MAX_OFFSET;
        // YOLO 
        if (Player.isMoving)
        {
           arm.transform.localPosition = originalArmPos + offset * Vector3.right;
           if (Player.hasTicket())
            armLeft.transform.localPosition = originalLeftArmPos + offset * Vector3.right;
        }
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

  public void ZoomArm()
  {
    if (zoomed)
    {
      zoomed = false;
      zoomTarget = ARM_MIN_SCALE;
    }
    else
    {
      zoomed = true;
      zoomTarget = ARM_MAX_SCALE;
    }
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
