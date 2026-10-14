using UnityEngine;
namespace NexaQuest.Brasil {
 [System.Serializable] public class EvolucaoAmazonia {
  public string nome; public Sprite sprite;
  public float diametro; public float corpoVisual=1; public Vector2 centroVisual;
 }
 public class AmazoniaMergeItem:MonoBehaviour {
  public MinijogoAmazoniaController jogo {get;private set;}
  public int Nivel {get;private set;}
  public bool Solto {get;private set;}
  public bool IsMerging {get;private set;}
  public Rigidbody2D Corpo {get;private set;}
  public CircleCollider2D Colisor {get;private set;}
  public void Preparar(MinijogoAmazoniaController dono,int nivel,bool solto) {
   jogo=dono;Nivel=nivel;var d=jogo.evolucoes[nivel-1];
   var visual=new GameObject("Sprite").AddComponent<SpriteRenderer>();visual.transform.SetParent(transform,false);visual.gameObject.layer=30;visual.sprite=d.sprite;visual.sortingOrder=10;
   float escala=d.diametro/(d.sprite.bounds.size.x*d.corpoVisual);visual.transform.localScale=Vector3.one*escala;visual.transform.localPosition=-new Vector3(d.centroVisual.x,d.centroVisual.y)*d.diametro;
   Corpo=gameObject.AddComponent<Rigidbody2D>();Corpo.gravityScale=1;Corpo.mass=Mathf.Pow(d.diametro/.8f,2);Corpo.drag=.12f;Corpo.angularDrag=.35f;Corpo.collisionDetectionMode=CollisionDetectionMode2D.Continuous;Corpo.interpolation=RigidbodyInterpolation2D.Interpolate;
   Colisor=gameObject.AddComponent<CircleCollider2D>();Colisor.radius=d.diametro/2;Colisor.sharedMaterial=jogo.materialFisico;
   Solto=solto;Corpo.bodyType=solto?RigidbodyType2D.Dynamic:RigidbodyType2D.Kinematic;Colisor.enabled=solto;
  }
  public void Soltar(){if(Solto)return;Solto=true;Colisor.enabled=true;Corpo.bodyType=RigidbodyType2D.Dynamic;}
  public void BloquearMerge(){IsMerging=true;Colisor.enabled=false;Corpo.simulated=false;}
  void OnCollisionEnter2D(Collision2D c)=>Contato(c);
  void OnCollisionStay2D(Collision2D c)=>Contato(c);
  void Contato(Collision2D c){var outro=c.collider.GetComponent<AmazoniaMergeItem>();if(outro!=null)jogo.TentarFundir(this,outro);}
 }
}
