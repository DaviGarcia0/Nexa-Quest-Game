using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
namespace NexaQuest.Brasil {
 public enum EstadoMergeAmazonia {Fechado,Instrucoes,Jogando,ObjetivoConcluido,GameOver}
 public class MinijogoAmazoniaController:MonoBehaviour {
  public BrazilWorld mundo;public AmazoniaMinigameUI interfaceJogo;public EvolucaoAmazonia[] evolucoes;
  public Sprite cenario;public Sprite[] efeitos;public PhysicsMaterial2D materialFisico;
  public float Esquerda=-5.9f,Direita=5.9f,Chao=-3.42f,SpawnY=3.25f,LinhaY=2.4f;
  public float CentroX=>10000;
  public EstadoMergeAmazonia Estado {get;private set;}=EstadoMergeAmazonia.Fechado;
  public bool EmPartida=>Estado==EstadoMergeAmazonia.Jogando;
  public int CoinsDaRodada {get;private set;}
  public bool MissaoConcluidaNaRodada {get;private set;}
  public bool ChaveNova {get;private set;}
  public bool CupuacuObjetivo {get;private set;}
  public bool AraraObjetivo {get;private set;}
  public bool OncaObjetivo {get;private set;}
  public Camera cameraJogo {get;private set;}
  public AmazoniaDangerZone perigo {get;private set;}
  public AmazoniaItemSpawner spawner {get;private set;}
  public readonly List<AmazoniaMergeItem> Itens=new List<AmazoniaMergeItem>();
  Scene cenaFisica;PhysicsScene2D fisica;GameObject mundoJogo;Transform itensRoot;PainelDesafiosAmazonia origem;bool bloqueio;int quadroEstado;
  ProgressionManager Progresso=>ProgressionManager.Instance;
  public bool TentarAbrir(PainelDesafiosAmazonia painel){
   if(Estado!=EstadoMergeAmazonia.Fechado||painel==null||!painel.EstaAberto||mundo.IsTransitioning||Progresso==null)return false;
   origem=painel;painel.Fechar();gameObject.SetActive(true);bloqueio=true;mundo.player.SetControlsLocked(true);interfaceJogo.Preparar();Mudar(EstadoMergeAmazonia.Instrucoes);return true;
  }
  public void Comecar(){
   if(Estado!=EstadoMergeAmazonia.Instrucoes&&Estado!=EstadoMergeAmazonia.GameOver)return;
   if(!Progresso.TentarGastarEnergia(20)){interfaceJogo.AvisarEnergia();return;}
   interfaceJogo.OcultarAviso();if(mundoJogo==null)CriarMundo();LimparItens();
   CoinsDaRodada=0;CupuacuObjetivo=AraraObjetivo=OncaObjetivo=MissaoConcluidaNaRodada=ChaveNova=false;
   perigo.Reiniciar();interfaceJogo.Reiniciar();Mudar(EstadoMergeAmazonia.Jogando);spawner.Reiniciar();
  }
  void CriarMundo(){
   cenaFisica=SceneManager.CreateScene("FruitMergeAmazonia_"+System.Guid.NewGuid().ToString("N"),new CreateSceneParameters(LocalPhysicsMode.Physics2D));fisica=cenaFisica.GetPhysicsScene2D();
   mundoJogo=new GameObject("GameplayWorldAmazonia");SceneManager.MoveGameObjectToScene(mundoJogo,cenaFisica);
   var cam=Novo("CameraMinijogo",new Vector2(CentroX,0)).AddComponent<Camera>();cam.transform.position=new Vector3(CentroX,0,-10);cam.orthographic=true;cam.orthographicSize=5.4f;cam.depth=100;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=Color.black;cam.cullingMask=1<<30;cameraJogo=cam;
   var bg=Novo("BackgroundAmazonia",new Vector2(CentroX,0)).AddComponent<SpriteRenderer>();bg.sprite=cenario;bg.sortingOrder=-10;bg.transform.localScale=new Vector3(19.2f/cenario.bounds.size.x,10.8f/cenario.bounds.size.y,1);
   Parede("LeftWall",new Vector2(CentroX+Esquerda-.1f,0),new Vector2(.2f,8));Parede("RightWall",new Vector2(CentroX+Direita+.1f,0),new Vector2(.2f,8));Parede("Floor",new Vector2(CentroX,Chao-.1f),new Vector2(Direita-Esquerda+.4f,.2f));
   var zona=Novo("DangerZone",new Vector2(CentroX,(LinhaY+4.5f)/2));var trigger=zona.AddComponent<BoxCollider2D>();trigger.isTrigger=true;trigger.size=new Vector2(Direita-Esquerda,4.5f-LinhaY);perigo=zona.AddComponent<AmazoniaDangerZone>();perigo.jogo=this;
   itensRoot=Novo("ItemsRuntime",Vector2.zero).transform;spawner=gameObject.AddComponent<AmazoniaItemSpawner>();spawner.jogo=this;
  }
  GameObject Novo(string nome,Vector2 pos){var g=new GameObject(nome);g.layer=30;g.transform.SetParent(mundoJogo.transform);g.transform.position=pos;return g;}
  void Parede(string nome,Vector2 pos,Vector2 size){var c=Novo(nome,pos).AddComponent<BoxCollider2D>();c.size=size;c.sharedMaterial=materialFisico;}
  public AmazoniaMergeItem CriarItem(int nivel,Vector2 pos,bool solto=true){
   var g=Novo(evolucoes[nivel-1].nome,pos);g.transform.SetParent(itensRoot);var item=g.AddComponent<AmazoniaMergeItem>();item.Preparar(this,nivel,solto);Itens.Add(item);return item;
  }
  public bool TentarFundir(AmazoniaMergeItem a,AmazoniaMergeItem b){
   if(!EmPartida||a==null||b==null||a==b||a.jogo!=this||b.jogo!=this||!a.Solto||!b.Solto||a.IsMerging||b.IsMerging||a.Nivel!=b.Nivel||a.Nivel>=8)return false;
   int nivel=a.Nivel+1;Vector2 ponto=(a.transform.position+b.transform.position)*.5f;Vector2 velocidade=(a.Corpo.velocity+b.Corpo.velocity)*.5f;float giro=(a.Corpo.angularVelocity+b.Corpo.angularVelocity)*.5f;
   a.BloquearMerge();b.BloquearMerge();Itens.Remove(a);Itens.Remove(b);Destroy(a.gameObject);Destroy(b.gameObject);
   var novo=CriarItem(nivel,ponto);novo.Corpo.velocity=Vector2.ClampMagnitude(velocidade,8);novo.Corpo.angularVelocity=Mathf.Clamp(giro,-120,120);novo.Corpo.AddForce(Vector2.up*.06f,ForceMode2D.Impulse);
   CoinsDaRodada++;Progresso.AddCoins(1);NexaQuest.NexaAudioManager.PlayMerge();interfaceJogo.Fusao(ponto);Efeito(ponto);
   if(nivel==4)CupuacuObjetivo=true;if(nivel==6)AraraObjetivo=true;if(nivel==8)OncaObjetivo=true;
   interfaceJogo.AtualizarObjetivos(CupuacuObjetivo,AraraObjetivo,OncaObjetivo);interfaceJogo.AtualizarCoins(CoinsDaRodada);
   if(!MissaoConcluidaNaRodada&&CupuacuObjetivo&&AraraObjetivo&&OncaObjetivo){
    MissaoConcluidaNaRodada=true;ChaveNova=Progresso.ConquistarChaveAmazonia();Progresso.AddXP(50);Progresso.MarcarMinijogoAmazoniaConcluido();Mudar(EstadoMergeAmazonia.ObjetivoConcluido);
   }return true;
  }
  void Efeito(Vector2 ponto){if(efeitos==null||efeitos.Length==0)return;var g=Novo("EfeitoFusao",ponto);var r=g.AddComponent<SpriteRenderer>();r.sprite=efeitos[0];r.sortingOrder=20;g.transform.localScale=Vector3.one*(1.1f/r.sprite.bounds.size.x);var e=g.AddComponent<AmazoniaMergeEffect>();e.frames=efeitos;}
  public void Continuar(){if(Estado==EstadoMergeAmazonia.ObjetivoConcluido)Mudar(EstadoMergeAmazonia.Jogando);}
  public void GameOver(){if(EmPartida)Mudar(EstadoMergeAmazonia.GameOver);}
  void Mudar(EstadoMergeAmazonia estado){Estado=estado;quadroEstado=Time.frameCount;
   foreach(var item in Itens)if(item!=null&&!item.IsMerging)item.Corpo.simulated=EmPartida;
   interfaceJogo.Mostrar(estado,ChaveNova,MissaoConcluidaNaRodada,CoinsDaRodada);
   if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(null);
  }
  void FixedUpdate(){if(EmPartida&&fisica.IsValid()){fisica.Simulate(Time.fixedDeltaTime);perigo.Avancar(Time.fixedDeltaTime);}}
  void Update(){if(NexaQuest.PauseSettingsManager.GameplayBlocked)return;ProcessarSaida(Input.GetKeyDown(KeyCode.Escape));}
  public void ProcessarSaida(bool esc){if(esc&&Time.frameCount>quadroEstado)Sair();}
  public void Sair(){if(Estado==EstadoMergeAmazonia.Fechado)return;Estado=EstadoMergeAmazonia.Fechado;interfaceJogo.Restaurar();gameObject.SetActive(false);if(origem!=null)origem.Abrir();}
  public void LimparItens(){foreach(var item in Itens)if(item!=null){item.BloquearMerge();Destroy(item.gameObject);}Itens.Clear();if(itensRoot!=null)foreach(Transform filho in itensRoot)if(filho.GetComponent<AmazoniaMergeItem>()==null)Destroy(filho.gameObject);}
  void OnDisable(){Estado=EstadoMergeAmazonia.Fechado;LimparItens();
   if(mundoJogo!=null){mundoJogo.SetActive(false);SceneManager.UnloadSceneAsync(cenaFisica);mundoJogo=null;}
   if(spawner!=null)Destroy(spawner);spawner=null;cameraJogo=null;
   if(bloqueio&&mundo!=null&&!mundo.IsTransitioning)mundo.player.SetControlsLocked(false);bloqueio=false;
  }
 }
 public class AmazoniaMergeEffect:MonoBehaviour {
  public Sprite[] frames;float tempo;
  void Update(){if(NexaQuest.PauseSettingsManager.GameplayBlocked)return;tempo+=Time.unscaledDeltaTime;int i=Mathf.FloorToInt(tempo*14);if(i>=frames.Length){Destroy(gameObject);return;}GetComponent<SpriteRenderer>().sprite=frames[i];}
 }
}
