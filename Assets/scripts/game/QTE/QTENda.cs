﻿using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class QTENda : QTEScript
{
  #region Members

  public Scrollbar scroll;

  #endregion

  #region Timeline

  void Start()
  {
  }

  #endregion

  #region Methods

  protected override void Init()
  {
    scroll.value = 1f;
  }

  public void Validate()
  {
    End(QTEResult.Success);
  }

  public void Cancel()
  {
    End(QTEResult.Failure);
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
      return QTEEnum.Nda;
    }
  }

  #endregion
}
