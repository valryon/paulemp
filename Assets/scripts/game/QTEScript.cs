using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public enum QTEResult
{
  NotCompleted,
  Success,
  Failure,
}

public abstract class QTEScript : MonoBehaviour
{
  public abstract void Launch();

  public abstract QTEResult Result
  {
    get;
    set;
  }
}
