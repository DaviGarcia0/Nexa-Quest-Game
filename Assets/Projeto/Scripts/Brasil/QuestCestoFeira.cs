using UnityEngine;
using UnityEngine.UI;
namespace NexaQuest.Brasil
{
    public class QuestCestoFeira : MonoBehaviour
    {
        public MinijogoFeira minijogo;
        public Image imagem;
        public Animator animador;
        public RectTransform aberturaCesto;
        [Min(1)] public float velocidade = 680;
        public Vector2 limites = new Vector2(-550,550);
        [Min(.01f)] public float escalaArte = .9f;
        public Vector2 posicaoInicial = new Vector2(0,-385);
        RectTransform retangulo;
        public float Entrada {get;private set;}
        void Awake(){retangulo=(RectTransform)transform;}
        void Update(){Mover(Input.GetAxisRaw("Horizontal"),Time.unscaledDeltaTime);}
        public void Mover(float horizontal,float intervalo) {
            Entrada=minijogo.EmPartida?Mathf.Clamp(horizontal,-1,1):0;
            var posicao=retangulo.anchoredPosition;
            posicao.x=Mathf.Clamp(posicao.x+Entrada*velocidade*Mathf.Max(0,intervalo),limites.x,limites.y);
            posicao.y=posicaoInicial.y;retangulo.anchoredPosition=posicao;
            animador.SetInteger("Direcao",Entrada<0?-1:Entrada>0?1:0);
        }
        void LateUpdate(){AlinharArte();}
        public void AlinharArte(){
            if(imagem.sprite==null)return;
            var sprite=imagem.sprite;var r=imagem.rectTransform;
            r.pivot=new Vector2(sprite.pivot.x/sprite.rect.width,sprite.pivot.y/sprite.rect.height);
            r.sizeDelta=sprite.rect.size*escalaArte;r.anchoredPosition=Vector2.zero;
        }
        public void Reiniciar(){
            if(retangulo==null)retangulo=(RectTransform)transform;
            retangulo.anchoredPosition=posicaoInicial;Entrada=0;animador.SetInteger("Direcao",0);
        }
    }
}
