using UnityEngine;
namespace NexaQuest {
 public class GlobalUIActions:MonoBehaviour {
  public void Configuracoes(){if(PauseSettingsManager.Instance!=null)PauseSettingsManager.Instance.Open();}
  public void Sair(){if(PauseSettingsManager.Instance!=null)PauseSettingsManager.Instance.Sair();}
 }
}
