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
  public GameObject pnjPrefab;
  public GameObject[] propsPrefabs;

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
        var pnjGo = Instantiate(pnjPrefab, b.pnjLocation.position, b.pnjLocation.rotation) as GameObject;
        var agent = pnjGo.GetComponent<AgentScript>();
        NetworkServer.Spawn(pnjGo);

        // Init agents
        agent.boothGeneratedID = b.GeneratedID;
        agent.EnableBooth(i, i == 0, i == booths.Length - 1, qtes[Random.Range(0, qtes.Length)].Type, b.Floor);
        i++;

        agent.LookAtTicketMachine();

        // Add some props!
        int propsCount = Random.Range(0, Mathf.Min(4, b.propsSpawnLocations.Length));
        List<Transform> propsPossiblePositions = b.propsSpawnLocations.OrderBy(t => Random.Range(0f, 1f)).ToList();

        for (int p = 0; p < propsCount; p++)
        {
          // Random position
          var t = propsPossiblePositions[Random.Range(0, propsPossiblePositions.Count)];
          propsPossiblePositions.Remove(t);

          // Random prop
          var propPrefab = propsPrefabs[Random.Range(0, propsPrefabs.Length)];

          // Go!
          var propObject = Instantiate(propPrefab, t.position, t.rotation) as GameObject;
          NetworkServer.Spawn(propObject);
        }
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
