using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class QTEPhoneNumber : QTEScript
{
  #region Members

  [Header("Bindings: Phone Number")]
  public Slider[] numbers;
  public Text[] numbersText;
  public Text phoneNumber;

  private int[] expected;

  #endregion

  #region Timeline

  protected override void Update()
  {
    base.Update();

    if (isPlaying == false) return;
    
    for (int i = 0; i < expected.Length; i++)
    {
      int v = (int)numbers[i].value;
      numbersText[i].text = v.ToString();
    }

  }

  #endregion

  #region Methods

  public void Validate()
  {
    int valids = 0;

    for (int i = 0; i < expected.Length; i++)
    {
      int v = (int)numbers[i].value;

      if (v == expected[i])
      {
        valids++;
      }
    }

    if (valids == expected.Length)
    {
      End(QTEResult.Success);
    }
  }

  protected override void Init()
  {
    string p = string.Empty;
    expected = new int[10];

    for (int i = 0; i < expected.Length; i++)
    {
      if (i == 0)
      {
        expected[i] = 0;
        numbers[i].value = Random.Range(5, 10);
      }
      else
      {
        expected[i] = Random.Range(i == 1 ? 1 : 0, 10);
        numbers[i].value = Random.Range(0, 10);
      }
      p += expected[i];

    }

    phoneNumber.text = p;
  }

  #endregion

  #region Properties

  public override float MaxDuration
  {
    get
    {
      return 40f;
    }
  }

  public override QTEEnum Type
  {
    get
    {
      return QTEEnum.PhoneNumber;
    }
  }

  #endregion
}
