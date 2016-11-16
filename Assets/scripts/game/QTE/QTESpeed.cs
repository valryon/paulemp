using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class QTESpeed : QTEScript
{
  #region Members

  [Header("Bindings: Speed")]
  public Button button;

  private Vector3 startPosition;

  #endregion

  #region Timeline

  void Start()
  {
    startPosition = ((RectTransform)button.transform).anchoredPosition;
    button.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
    {
      End(QTEResult.Success);
    }));
  }

  #endregion

  #region Methods

  protected override void Init()
  {
    ((RectTransform)button.transform).anchoredPosition = startPosition + new Vector3(Random.Range(-400, 400), Random.Range(-300, 300));
    button.gameObject.SetActive(false);

    StartCoroutine(Show());
  }

  private IEnumerator Show()
  {
    yield return new WaitForSeconds(Random.Range(2f, 6f));

    button.gameObject.SetActive(true);
    timeLeft = 0.92f;
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
      return 120f;
    }
  }

  public override QTEEnum Type
  {
    get
    {
      return QTEEnum.Speed;
    }
  }

  #endregion
}
