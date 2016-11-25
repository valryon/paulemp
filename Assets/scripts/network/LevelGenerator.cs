using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;


class LevelGenerator : MonoBehaviour
{


  [Header("Floor")]
  public GameObject floorInst;

  [Header("Stairs")]
  public GameObject stairCaseFirst;
  public GameObject stairCase;
  public GameObject stairCaseLast;

  [Header("Rooms")]
  public GameObject[] rooms;

  [Header("Props")]
  public DecorationScript[] propsPrefabs;

  public int floorsMin = 1;
  public int floorsMax = 1;

  public int nbMinStair = 1;
  public int nbMaxStair = 1;
  public int size = 5;
  public Vector3 blockSize = Vector3.one;

  private int levelSizeX;
  private int levelSizeY;
  private int generationID;

  private static float getRandom(float min, float max)
  {
    return UnityEngine.Random.value * (max - min) + min;
  }

  private bool isFree(ref int[][] grid, int x, int y, int sizeX, int sizeY, int marginLeft, int marginTop, int marginRigth, int marginBottom)
  {
    for (var j = y - marginTop; j < y + sizeY + marginBottom; ++j)
    {
      if (j >= grid.Length) return false;
      for (var i = x + marginLeft; i < x + sizeX + marginRigth; ++i)
      {
        if (i >= grid[y].Length) return false;
        if (grid[j][i] != 0) return false;
      }
    }
    return true;
  }


  private void fillGrid(ref int[][] grid, int x, int y, int offX, int offY, int marginLeft, int marginTop, int marginRigth, int marginBottom, int type)
  {
    int p = 0;
    for (var j = 0 - marginTop; j < offY + marginBottom; ++j)
    {
      for (var i = 0 - marginLeft; i < offX + marginRigth; ++i)
      {
        if (j < 0 || j >= offY || i < 0 || i >= offX)
        {
          grid[j + y][i + x] = 2; // it s a margin
        }
        else
        {
          grid[j + y][i + x] = type + p;
        }
        if (i == 0 && j == 0) p = 1; //upper left corner parent, others are child => lower bit set to 1
      }
    }

  }



  private void setRooms(ref int[][] grid)
  {
    for (var y = 0; y < levelSizeY; ++y)
    {
      for (var x = 0; x < levelSizeX; ++x)
      {
        var p = getRandom(0, 100);
        if (p > 80) continue;
        if (!isFree(ref grid, x, y, 2, 2, 0, 0, 0, 1)) continue;
        fillGrid(ref grid, x, y, 2, 2, 0, 0, 0, 1, 20);
      }
    }
  }


  private void initGrid(ref int[][] grid, int sizeX, int sizeY, ref Vector2[] stairs)
  {
    if (grid == null) grid = new int[sizeY][];
    for (var y = 0; y < sizeY; ++y)
    {
      if (grid[y] == null) grid[y] = new int[sizeX];
      for (var x = 0; x < sizeX; ++x)
      {
        grid[y][x] = 0;
        if (stairs != null)
        {
          if (stairs.Contains(new Vector2(x, y)))
          {

            fillGrid(ref grid, x, y, 2, 2, 1, 1, 1, 1, 10);
          }
        }
      }
    }
    // for stairs
    if (stairs != null)
    {
      for (var y = 0; y < sizeY; ++y)
      {
        for (var x = 0; x < sizeX; ++x)
        {
          if (stairs.Contains(new Vector2(x, y)))
          {
            fillGrid(ref grid, x, y, 2, 2, 1, 1, 1, 1, 10);
          }
        }
      }
    }
  }


  private void buildLevel(ref int[][] grid, int floor, int nbFloors)
  {
    for (var y = 0; y < levelSizeY; ++y)
    {
      for (var x = 0; x < levelSizeX; ++x)
      {
        GameObject go = null;
        Vector3 v = new Vector3(y * blockSize.x, floor * blockSize.y + 1, x * blockSize.z);
        if (grid[y][x] == 10) // stairCase
        {
          Debug.Log("Instantiate stairs at " + v);
          if (floor == 0)
          {
            go = Instantiate(stairCaseFirst, v, Quaternion.identity) as GameObject;
          }
          else if (floor == nbFloors - 1)
          {
            go = Instantiate(stairCaseLast, v, Quaternion.identity) as GameObject;
          }
          else
          {
            go = Instantiate(stairCase, v, Quaternion.identity) as GameObject;
          }
        }
        else if (grid[y][x] == 20) //rooms
        {

          var r = (int)getRandom(0, rooms.Length);
          go = Instantiate(rooms[r], v, Quaternion.identity) as GameObject;

          var b = go.GetComponent<BoothBaseScript>();
          if (b != null)
          {
            b.GeneratedID = generationID;
            generationID++;
            b.Floor = floor;
          }
        }
        else if (grid[y][x] < 10 && grid[y][x] % 2 == 0)
        {
          go = Instantiate(floorInst, v, Quaternion.identity) as GameObject;
        }

        if (go != null)
        {
          go.transform.parent = this.transform;
        }
      }
    }
  }

  private void dumpFloor(ref int[][] grid, int floor)
  {
    Debug.Log("======= Dump floor " + floor + "====================================");
    for (var y = 0; y < levelSizeY; ++y)
    {
      var str = String.Format("y {0,4}: ", y.ToString("D2"));
      for (var x = 0; x < levelSizeX; ++x)
      {
        str += String.Format("{0,-4}", grid[y][x].ToString("D2"));
      }
      Debug.Log(str);
    }
    Debug.Log("====================================================================");
  }

  public void Generate(int seed)
  {
    Debug.Log("Start procedural level generation");
    UnityEngine.Random.InitState(seed);
    levelSizeX = 30;
    levelSizeY = 10;
    generationID = 0;
    int[][] grid = null;
    Vector2[] stairsPosition = null;

    // Warmup
    initGrid(ref grid, levelSizeX, levelSizeY, ref stairsPosition);
    var nbStairs = (int)getRandom(nbMinStair, nbMaxStair);
    stairsPosition = new Vector2[nbStairs];
    for (var s = 0; s < nbStairs; ++s)
    {
      bool added = false;
      do
      {
        var x = (int)getRandom(0, levelSizeX);
        var y = (int)getRandom(0, levelSizeY / 2) * 2;
        if (isFree(ref grid, x, y, 2, 2, 1, 1, 1, 1))
        {
          added = true;
          fillGrid(ref grid, x, y, 2, 2, 1, 1, 1, 1, 10);
          stairsPosition[s] = new Vector2(x, y);
        }
      } while (!added);

    }
    //  Warmup done
    var nbFloors = (int)getRandom(floorsMin, floorsMax);
    for (var f = 0; f < nbFloors; ++f)
    {
      initGrid(ref grid, levelSizeX, levelSizeY, ref stairsPosition);
      setRooms(ref grid);
      //last step, build level floor
      buildLevel(ref grid, f, nbFloors);
      dumpFloor(ref grid, f);
    }
  }



  public void GenerateProps(bool serverSide)
  {
    var booths = FindObjectsOfType<BoothBaseScript>();

    foreach (var b in booths)
    {
      var propsLoc = b.GetComponentsInChildren<DecorationLocation>();

      for (int p = 0; p < propsLoc.Length; p++)
      {
        var t = propsLoc[p];

        // Optionnal prop?
        if(t.Mandatory == false && UnityEngine.Random.Range(0, 10) > 4)
        {
          continue;
        }

        // Random prop of the given type
        var possibilities = propsPrefabs.Where(prop => prop != null && prop.DecorationType == t.DecorationType).ToArray();
        if (possibilities.Length > 0)
        {
          var propPrefab = possibilities[UnityEngine.Random.Range(0, possibilities.Length)];

          var network = propPrefab.GetComponent<NetworkIdentity>();

          // Props can be networked or local.
          // - Local props are created by client (same seed, same random should spawn same decorations)
          // - Networked props are spawned by server
          if (network == null && serverSide == false || network != null && serverSide)
          {
            // Go!
            var propObject = Instantiate(propPrefab.gameObject, t.transform.position + new Vector3(0, propPrefab.transform.position.y, 0), t.transform.rotation) as GameObject;
            propObject.transform.parent = b.transform;

            if (serverSide)
            {
              NetworkServer.Spawn(propObject);
            }
          }
        }
      }

      b.ClearSpawns();
    }
  }

}
