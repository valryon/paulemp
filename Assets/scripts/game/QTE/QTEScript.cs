using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public abstract class QTEScript : MonoBehaviour
{
  #region Members
  
  [Header("Bindings")]
  public Canvas ui;
  public Text clock;

  protected PlayerScript player;
  protected bool isPlaying;
  protected float timeLeft;
  protected System.Action<QTEResult> callback;

  #endregion

  #region Timeline

  protected void Awake()
  {
    ui.gameObject.SetActive(false);
  }

  protected virtual void Update()
  {
    if (isPlaying)
    {
      timeLeft -= Time.deltaTime;
      if (timeLeft < 0f)
      {
        End(QTEResult.Failure);
      }

      clock.text = timeLeft.ToString("00");
    }
  }

  #endregion

  #region Methods

  public virtual void Launch(PlayerScript playerScript, System.Action<QTEResult> endQTE)
  {
    player = playerScript;
    isPlaying = true;
    timeLeft = MaxDuration;
    callback = endQTE;
    ui.gameObject.SetActive(true);

    playerScript.fpsController.enabled = false;
    Cursor.visible = true;
    Cursor.lockState = CursorLockMode.None;

    Init();
  }

  public virtual void End(QTEResult result)
  {
    ui.gameObject.SetActive(false);
    timeLeft = 0;
    isPlaying = false;

    player.fpsController.enabled = true;
    Cursor.visible = false;
    Cursor.lockState = CursorLockMode.Locked;

    if (callback != null)
    {
      callback(result);
    }
  }

  protected abstract void Init();

  #endregion

  #region Properties

  public abstract QTEEnum Type
  {
    get;
  }

  public abstract float MaxDuration
  {
    get;
  }

  #endregion
}
