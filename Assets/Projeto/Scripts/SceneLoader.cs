using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader:MonoBehaviour {
 bool carregando;
 public bool Carregando=>carregando;
 public void CarregarCena(string nomeDaCena){if(carregando)return;carregando=true;if(nomeDaCena=="MinhaQuest")NexaQuest.MinhaQuest.MinhaQuestNavigation.RegisterCurrent();SceneManager.LoadScene(nomeDaCena);}
}
