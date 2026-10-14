using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace NexaQuest.Brasil {
 public class AmazoniaMinigameUI:MonoBehaviour {
  public MinijogoAmazoniaController jogo;
  public GameObject instrucoes,hud,painelComChave,painelSemChave,gameOver,faixaRodadaEncerrada;
  public TMP_Text coinsRodada,coinsFinal,avisoEnergia;
  public Image proximo,quest,popupCoin;
  public Sprite[] idle,comemoracao;
  public GameObject[] checks;
  public GameObject[] hudsGlobais;
  bool[] hudsAtivos;Transform paiAviso;int indiceAviso;Vector2 posicaoAviso;bool preparada;
  float festaAte,inicioFesta,popupAte;Vector2 popupOrigem;
  public void Preparar(){
   hudsAtivos=new bool[hudsGlobais.Length];for(int i=0;i<hudsAtivos.Length;i++)hudsAtivos[i]=hudsGlobais[i].activeSelf;
   paiAviso=avisoEnergia.transform.parent;indiceAviso=avisoEnergia.transform.GetSiblingIndex();posicaoAviso=avisoEnergia.rectTransform.anchoredPosition;
   avisoEnergia.transform.SetParent(transform,false);avisoEnergia.transform.SetAsLastSibling();avisoEnergia.rectTransform.anchoredPosition=new Vector2(0,-510);preparada=true;OcultarAviso();
  }
  public void Restaurar(){if(!preparada)return;for(int i=0;i<hudsGlobais.Length;i++)hudsGlobais[i].SetActive(hudsAtivos[i]);
   OcultarAviso();avisoEnergia.transform.SetParent(paiAviso,false);avisoEnergia.transform.SetSiblingIndex(indiceAviso);avisoEnergia.rectTransform.anchoredPosition=posicaoAviso;preparada=false;
  }
  public void Mostrar(EstadoMergeAmazonia estado,bool chave,bool concluida,int moedas){
   bool partida=estado!=EstadoMergeAmazonia.Instrucoes&&estado!=EstadoMergeAmazonia.Fechado;
   instrucoes.SetActive(estado==EstadoMergeAmazonia.Instrucoes);hud.SetActive(partida);
   painelComChave.SetActive(estado==EstadoMergeAmazonia.ObjetivoConcluido&&chave);painelSemChave.SetActive(estado==EstadoMergeAmazonia.ObjetivoConcluido&&!chave);
   gameOver.SetActive(estado==EstadoMergeAmazonia.GameOver);faixaRodadaEncerrada.SetActive(concluida);coinsFinal.text=moedas.ToString();
   if(preparada)for(int i=0;i<hudsGlobais.Length;i++)hudsGlobais[i].SetActive(!partida&&hudsAtivos[i]);
  }
  public void Reiniciar(){festaAte=popupAte=0;popupCoin.gameObject.SetActive(false);AtualizarCoins(0);AtualizarObjetivos(false,false,false);}
  public void AtualizarCoins(int n)=>coinsRodada.text=n.ToString();
  public void AtualizarProximo(int n){proximo.sprite=jogo.evolucoes[n-1].sprite;}
  public void AtualizarObjetivos(bool c,bool a,bool o){checks[0].SetActive(c);checks[1].SetActive(a);checks[2].SetActive(o);}
  public void Fusao(Vector2 ponto){if(Time.unscaledTime>=festaAte)inicioFesta=Time.unscaledTime;festaAte=Time.unscaledTime+1;popupAte=Time.unscaledTime+.85f;
   Vector3 tela=jogo.cameraJogo.WorldToScreenPoint(ponto);RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)hud.transform,tela,null,out popupOrigem);popupCoin.rectTransform.anchoredPosition=popupOrigem;popupCoin.color=Color.white;popupCoin.gameObject.SetActive(true);
  }
  public void AvisarEnergia(){avisoEnergia.text="Energia insuficiente. Você precisa de 20 para jogar.";avisoEnergia.gameObject.SetActive(true);}
  public void OcultarAviso()=>avisoEnergia.gameObject.SetActive(false);
  void Update(){
   bool festa=Time.unscaledTime<festaAte;var frames=festa?comemoracao:idle;
   if(frames!=null&&frames.Length>0)quest.sprite=frames[festa?Mathf.Clamp(Mathf.FloorToInt((Time.unscaledTime-inicioFesta)*8),0,frames.Length-1):Mathf.FloorToInt(Time.unscaledTime*8)%frames.Length];
   if(popupCoin.gameObject.activeSelf){float t=1-(popupAte-Time.unscaledTime)/.85f;popupCoin.rectTransform.anchoredPosition=popupOrigem+Vector2.up*t*45;popupCoin.color=new Color(1,1,1,1-t);if(t>=1)popupCoin.gameObject.SetActive(false);}
  }
  System.Collections.IEnumerator RestaurarDepois(){yield return null;if(this!=null)Restaurar();}
  void OnDisable(){if(preparada&&jogo!=null&&jogo.mundo!=null&&jogo.mundo.gameObject.activeInHierarchy)jogo.mundo.StartCoroutine(RestaurarDepois());}
 }
}
