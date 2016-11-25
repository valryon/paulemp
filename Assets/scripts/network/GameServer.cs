using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GameServer : NetworkBehaviour
{
  [Header("Settings")]
  public int seed = 1;

  [Header("Prefabs")]
  public GameObject agentPrefab;
  public GameObject[] pnjsPrefab;

  private bool boothCreated = false;

  void Awake()
  {
    // seed = Random.Range(0, 600000);
    seed = 42;
  }

  [ServerCallback]
  void Start()
  {

  }

  [Server]
  public void CreateLevelAndStuff()
  {
    Debug.Log("SERVER create level and agents");

    // Create level
    LevelGenerator l = FindObjectOfType<LevelGenerator>();
    l.Generate(seed);

    // Initialize booth, agents, tickets machines, etc.
    // Do it server-side!
    if (boothCreated == false)
    {
      boothCreated = true;

      System.Random r = new System.Random();

      // Find all booth created by level generator
      var qtes = FindObjectsOfType<QTEScript>();
      var booths = FindObjectsOfType<BoothBaseScript>().OrderBy(b => r.NextDouble()).ToArray();

      int i = 0;
      foreach (var b in booths)
      {
        // Create PNJ
        var pnjGo = Instantiate(agentPrefab, b.pnjLocation.position, b.pnjLocation.rotation) as GameObject;
        var agent = pnjGo.GetComponent<AgentScript>();
        NetworkServer.Spawn(pnjGo);

        // Init agents
        agent.boothGeneratedID = b.GeneratedID;
        agent.EnableBooth(i, i == 0, i == booths.Length - 1, qtes[Random.Range(0, qtes.Length)].Type, b.Floor);
        i++;

        agent.LookAtTicketMachine();
      }

      // Add some SERVER-SIDE props!
      l.GenerateProps(true);

      // And generate LOCAL props for host (server+client)
      l.GenerateProps(false);

      // Add PNJ by server
      foreach(var p in l.PNJPositions)
      {
        var prefab = pnjsPrefab[Random.Range(0, pnjsPrefab.Length)];

        var pnj = Instantiate(prefab, p, prefab.transform.rotation) as GameObject;
        NetworkServer.Spawn(pnj);
      }
    }
  }

  [Server]
  public void RequestLevelCreation()
  {
    // Ask ALL players to create the level (do it for each new connection)
    foreach (var p in FindObjectsOfType<PlayerScript>())
    {
      p.RpcGenerateLevel(seed);
    }
  }

  public static void PlayEffect(string effect, Vector3 position)
  {
    foreach (var p in FindObjectsOfType<PlayerScript>())
    {
      p.RpcPlayEffect(effect, position);
    }
  }

  public static void PlaySound(string sound, Vector3 position)
  {
    foreach (var p in FindObjectsOfType<PlayerScript>())
    {
      p.RpcPlaySound(sound, position);
    }
  }
}
