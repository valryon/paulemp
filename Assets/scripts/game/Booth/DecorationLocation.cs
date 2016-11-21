using UnityEngine;
using System.Collections;


public enum DecorationType
{
  Desk = 1,
  Chairs = 2,
  OnDesk = 3,
  OnWall = 4,
  OnFloor = 5
}

public class DecorationLocation : MonoBehaviour
{
  public DecorationType DecorationType;
  public bool Mandatory = false;
}
