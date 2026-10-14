using System.Collections.Generic;
using UnityEngine;
namespace NexaQuest.Brasil {
 public class AmazoniaDangerZone:MonoBehaviour {
  public MinijogoAmazoniaController jogo;
  public float tempoLimite=1.5f;
  readonly Dictionary<AmazoniaMergeItem,float> tempos=new Dictionary<AmazoniaMergeItem,float>();
  readonly HashSet<AmazoniaMergeItem> presentes=new HashSet<AmazoniaMergeItem>();
  public float TempoAtual {get;private set;}
  void OnTriggerEnter2D(Collider2D c){var item=c.GetComponent<AmazoniaMergeItem>();if(item!=null&&item.Solto)presentes.Add(item);}
  void OnTriggerExit2D(Collider2D c){var item=c.GetComponent<AmazoniaMergeItem>();if(item!=null){presentes.Remove(item);tempos.Remove(item);}}
  public void Avancar(float dt){
   if(!jogo.EmPartida)return;TempoAtual=0;
   foreach(var item in new List<AmazoniaMergeItem>(presentes)) {
    if(item==null||item.IsMerging||!item.Solto){presentes.Remove(item);tempos.Remove(item);continue;}
    float t;tempos.TryGetValue(item,out t);t+=dt;tempos[item]=t;TempoAtual=Mathf.Max(TempoAtual,t);
   }
   if(TempoAtual>=tempoLimite)jogo.GameOver();
  }
  public void Reiniciar(){presentes.Clear();tempos.Clear();TempoAtual=0;}
 }
}
