using UnityEngine;
using System.Linq;
using UnityEngine.Networking;

[System.Serializable]
public struct Quest
{
  public string description;
  public int agentID;
  public int agentIDDependency;
  public bool revealed;
  public bool completed;
  public int order;

  public Quest(AgentScript agent, AgentScript dependency, bool first, bool last, int priority)
  {
    agentID = (int)agent.netId.Value;
    agentIDDependency = -1;
    if (dependency != null)
    {
      agentIDDependency = (int)dependency.netId.Value;
    }

    revealed = false;
    completed = false;
    description = string.Empty;
    order = priority;

    description = GetDescription(agent, first, last);
  }

  private static string[] FirstActions = {
    "S'inscrire au fichier {0}",
    "Demander à vérifier son dossier {0}",
  };

  private static string[] Actions = {
    "Obtenir le formulaire {0}",
    "Faire tamponner le document {0}",
    "Fournir le justificatif {0}",
    "Aller au guichet {0}",
    "Faire faire une copie de {0}",
  };

  private static string[] LastActions = {
    "Valider auprès du manager {0}",
    "Déposer le dossier au bureau {0}",
  };

  private string GetDescription(AgentScript agent, bool first, bool last)
  {
    var list = Actions;
    if (first) { list = FirstActions; }
    if (last) { list = LastActions; }

    var s = list[Random.Range(0, list.Length)];

    return string.Format(s, agent.data.boothName);
  }

  public override string ToString()
  {
    return description;
  }
}

public class QuestList : SyncListStruct<Quest>
{
  public string ToReadableString()
  {
    string s = string.Empty;

    for (int i = 0; i < Count; i++)
    {
      var q = this[i];
      if (q.revealed && q.completed == false)
      {
        s += "- " + q.description + "\n";
      }
    }

    return s;
  }

  public bool CanBeCompleted(AgentScript agent)
  {
    bool weNeedToGoDeeper = true;
    int startId = (int)agent.netId.Value;
    int id = startId;
    bool allow = true;

    while (weNeedToGoDeeper && allow)
    {
      // Get dependency
      Quest quest = this.Where(q => q.agentID == id).FirstOrDefault();

      if (id != startId)
      {
        allow &= quest.completed;
      }
      else
      {
        // First quest
        // Cannot be completed if not revealed
        allow &= quest.revealed;
      }

      if (quest.agentIDDependency < 0)
      {
        weNeedToGoDeeper = false;
      }
      else
      {
        id = quest.agentIDDependency;
      }
    }

    return allow;
  }

  public void Reveal(int id, bool recursive = true)
  {
    if (id < 0) return;

    for (int i = 0; i < Count; i++)
    {
      var q = this[i];

      if (q.agentID == id)
      {
        q.revealed = true;

        this[i] = q;

        if (recursive)
        {
          Reveal(q.agentIDDependency, false);
        }
      }
    }
  }
}
