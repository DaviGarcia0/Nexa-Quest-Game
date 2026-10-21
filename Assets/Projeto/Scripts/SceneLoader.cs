using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneLoader:MonoBehaviour {
 bool carregando;
 public bool Carregando=>carregando;
 public void CarregarCena(string nomeDaCena){if(carregando)return;carregando=true;SceneManager.LoadScene(nomeDaCena);}
}
