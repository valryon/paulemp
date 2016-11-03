﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameServer : NetworkBehaviour
{
  private List<BoothScript> booths = new List<BoothScript>();

  [ServerCallback]
  void Start()
  {
    // TODO : Génération procédurale du niveau ? Ce serait à faire ici !

    booths = FindObjectsOfType<BoothScript>().ToList();
    var qtes = FindObjectsOfType<QTEScript>();

    for (int i = 0; i < booths.Count; i++)
    {
      // Random y rotation
      booths[i].transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);

      booths[i].EnableBooth(i, qtes[Random.Range(0, qtes.Length)].Type);
    }
  }
}