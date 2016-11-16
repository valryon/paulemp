using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class QTELuck : QTEScript
{
  #region Members

  [Header("Bindings: Luck")]
  public Text text;
  public Button[] buttons;

  private int rightIndex = 0;

  #endregion

  #region Timeline

  void Start()
  {
    for (int i = 0; i < buttons.Length; i++)
    {
      var b = buttons[i];

      int buttonIndex = i;
      b.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
      {
        Click(buttonIndex);
      }));
    }
  }

  #endregion

  #region Methods

  protected override void Init()
  {
    rightIndex = Random.Range(0, buttons.Length);
    text.text = "";

    foreach (var b in buttons)
    {
      b.gameObject.SetActive(true);
    }
  }

  public void Click(int i)
  {
    isPlaying = false;

    foreach (var b in buttons)
    {
      b.gameObject.SetActive(false);
    }

    if (i == rightIndex)
    {
      text.text = "YES!";
      StartCoroutine(EndDelay(1f, QTEResult.Success));
    }
    else
    {
      text.text = "lol nope.";
      StartCoroutine(EndDelay(1f, QTEResult.Failure));
    }
  }

  private IEnumerator EndDelay(float delay, QTEResult r)
  {
    yield return new WaitForSeconds(delay);

    End(r);
  }

  #endregion

  #region Properties

  public override float MaxDuration
  {
    get
    {
      return 15f;
    }
  }

  public override QTEEnum Type
  {
    get
    {
      return QTEEnum.Luck;
    }
  }

  #endregion
}
