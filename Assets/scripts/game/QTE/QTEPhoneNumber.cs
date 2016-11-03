using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class QTEPhoneNumber : QTEScript
{
  private System.Action<QTEResult> callback;

  public override void Launch(PlayerScript playerScript, System.Action<QTEResult> endQTE)
  {
    callback = endQTE;

    StartCoroutine(Delay());
  }

  private IEnumerator Delay()
  {
    yield return new WaitForSeconds(3f);
    
    if (callback != null)
    {
      callback(QTEResult.Success);
    }
  }
  public override QTEEnum Type
  {
    get
    {
      return QTEEnum.PhoneNumber;
    }
  }
}
