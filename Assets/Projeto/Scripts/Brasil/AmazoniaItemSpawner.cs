using UnityEngine;
using UnityEngine.EventSystems;
namespace NexaQuest.Brasil {
 public class AmazoniaItemSpawner:MonoBehaviour {
  public MinijogoAmazoniaController jogo;
  public AmazoniaMergeItem Preview {get;private set;}
  public int ItemAtual {get;private set;}
  public int ItemProximo {get;private set;}
  public float cooldown=.45f;
  float restante,previewX;Vector3 mouseAnterior;int quadroDrop;
  int Sortear()=>Random.Range(1,4);
  public void Reiniciar(){Preview=null;restante=0;previewX=jogo.CentroX;ItemAtual=Sortear();ItemProximo=Sortear();mouseAnterior=Input.mousePosition;CriarPreview();}
  void CriarPreview(){Preview=jogo.CriarItem(ItemAtual,new Vector2(Mathf.Clamp(previewX,jogo.CentroX+jogo.Esquerda+jogo.evolucoes[ItemAtual-1].diametro/2,jogo.CentroX+jogo.Direita-jogo.evolucoes[ItemAtual-1].diametro/2),jogo.SpawnY),false);jogo.interfaceJogo.AtualizarProximo(ItemProximo);}
  public void Mover(float x){if(Preview==null||!jogo.EmPartida)return;float r=Preview.Colisor.radius;previewX=Mathf.Clamp(x,jogo.CentroX+jogo.Esquerda+r,jogo.CentroX+jogo.Direita-r);Preview.transform.position=new Vector3(previewX,jogo.SpawnY,0);}
  public bool Soltar(){
   if(!jogo.EmPartida||Preview==null||restante>0||quadroDrop==Time.frameCount)return false;
   quadroDrop=Time.frameCount;Preview.Soltar();Preview=null;ItemAtual=ItemProximo;ItemProximo=Sortear();restante=cooldown;jogo.interfaceJogo.AtualizarProximo(ItemProximo);return true;
  }
  void Update(){
   if(!jogo.EmPartida)return;
   if(Preview==null){restante-=Time.deltaTime;if(restante<=0){
    float r=jogo.evolucoes[ItemAtual-1].diametro/2;
    bool livre=true;foreach(var item in jogo.Itens)if(item!=null&&!item.IsMerging&&item.Solto&&item.transform.position.y+item.Colisor.radius>jogo.SpawnY-r&&Mathf.Abs(item.transform.position.x-previewX)<item.Colisor.radius+r){livre=false;break;}
    if(livre)CriarPreview();
   }return;}
   float teclado=Input.GetAxisRaw("Horizontal");if(teclado!=0)Mover(Preview.transform.position.x+teclado*6*Time.deltaTime);
   else if(Input.mousePosition!=mouseAnterior){Mover(jogo.cameraJogo.ScreenToWorldPoint(Input.mousePosition).x);mouseAnterior=Input.mousePosition;}
   if(Input.GetKeyDown(KeyCode.Space)||(Input.GetMouseButtonDown(0)&&(EventSystem.current==null||!EventSystem.current.IsPointerOverGameObject())))Soltar();
  }
 }
}
