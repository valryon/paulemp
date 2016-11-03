using UnityEngine;
using System.Linq;
using UnityEngine.Networking;

[System.Serializable]
public struct Quest
{
  public string description;
  public int boothID;
  public int boothIDDependency;
  public bool revealed;
  public bool completed;
  public int order;

  public Quest(BoothScript booth, BoothScript dependency, bool first, bool last, int priority)
  {
    boothID = (int)booth.netId.Value;
    boothIDDependency = -1;
    if (dependency != null)
    {
      boothIDDependency = (int)dependency.netId.Value;
    }

    revealed = false;
    completed = false;
    description = string.Empty;
    order = priority;

    description = GetDescription(booth, first, last);
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

  private string GetDescription(BoothScript booth, bool first, bool last)
  {
    var list = Actions;
    if (first) { list = FirstActions; }
    if (last) { list = LastActions; }

    var s = list[Random.Range(0, list.Length)];

    return string.Format(s, booth.data.boothName);
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

  public bool CanBeCompleted(BoothScript booth)
  {
    bool weNeedToGoDeeper = true;
    int startId = (int)booth.netId.Value;
    int id = startId;
    bool allow = true;

    while (weNeedToGoDeeper)
    {
      // Get dependency
      Quest quest = this.Where(q => q.boothID == id).FirstOrDefault();

      if (id != startId)
      {
        allow &= quest.completed;
        if (!allow) weNeedToGoDeeper = false;
      }

      if (quest.boothIDDependency < 0)
      {
        weNeedToGoDeeper = false;
      }
      else
      {
        id = quest.boothIDDependency;
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

      if (q.boothID == id)
      {
        q.revealed = true;

        this[i] = q;

        if (recursive)
        {
          Reveal(q.boothIDDependency, false);
        }

        break;
      }
    }
  }
}
