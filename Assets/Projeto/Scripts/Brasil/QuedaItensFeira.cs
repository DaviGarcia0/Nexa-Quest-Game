using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace NexaQuest.Brasil
{
    public enum TipoItemFeira { Banana, Abacaxi, Melancia, Chinelo, Disco, ConsoleXbox }
    public class QuedaItensFeira : MonoBehaviour
    {
        public MinijogoFeira minijogo;
        public QuestCestoFeira quest;
        public RectTransform camada;
        public Sprite[] sprites;
        [Min(.3f)] public float intervaloSurgimento=1.25f;
        [Min(20)] public float velocidadeQueda=210;
        [Range(0, .5f)] public float chanceItemErrado=.18f;
        public Vector2 limitesHorizontais=new Vector2(-520,520);
        public float alturaSurgimento=400, alturaChao=-390;
        [Min(1)] public int maximoItens=14;
        class Item { public int id; public TipoItemFeira tipo; public RectTransform retangulo; public float velocidade; }
        readonly List<Item> itens=new List<Item>();
        readonly List<int> sacoFrutas=new List<int>();
        readonly Vector3[] cantos=new Vector3[4];
        float restante;int proximoId;
        public int QuantidadeAtiva=>itens.Count;
        void Update(){Simular(Time.unscaledDeltaTime);}
        public void Reiniciar(){Limpar();sacoFrutas.Clear();restante=.7f;}
        public void Limpar(){foreach(var item in itens)if(item.retangulo!=null){item.retangulo.gameObject.SetActive(false);Destroy(item.retangulo.gameObject);}itens.Clear();}
        void OnDisable(){Limpar();}
        int ProximaFruta(){
            if(sacoFrutas.Count==0)sacoFrutas.AddRange(new[]{0,1,2});
            int indice=Random.Range(0,sacoFrutas.Count),tipo=sacoFrutas[indice];sacoFrutas.RemoveAt(indice);return tipo;
        }
        public int CriarItem(TipoItemFeira tipo,Vector2 posicao,float velocidade) {
            if(!minijogo.EmPartida||itens.Count>=maximoItens||sprites==null||(int)tipo>=sprites.Length)return -1;
            var objeto=new GameObject("Item_"+tipo,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            objeto.layer=5;var r=objeto.GetComponent<RectTransform>();r.SetParent(camada,false);r.anchoredPosition=posicao;
            var imagem=objeto.GetComponent<Image>();imagem.sprite=sprites[(int)tipo];imagem.preserveAspect=true;imagem.raycastTarget=false;
            var tamanho=imagem.sprite.rect.size;r.sizeDelta=tamanho/Mathf.Max(tamanho.x,tamanho.y)*76f;
            var item=new Item{id=++proximoId,tipo=tipo,retangulo=r,velocidade=Mathf.Max(0,velocidade)};itens.Add(item);return item.id;
        }
        public bool Resolver(int id,bool capturado) {
            int indice=itens.FindIndex(i=>i.id==id);if(indice<0||!minijogo.EmPartida)return false;
            var item=itens[indice];itens.RemoveAt(indice);
            item.retangulo.gameObject.SetActive(false);Destroy(item.retangulo.gameObject);
            minijogo.RegistrarItem(item.tipo,capturado);return true;
        }
        public void Simular(float intervalo) {
            if(!minijogo.EmPartida)return;
            intervalo=Mathf.Clamp(intervalo,0,.1f);
            restante-=intervalo;
            if(restante<=0){restante=Mathf.Max(.3f,intervaloSurgimento);var tipo=Random.value<chanceItemErrado?(TipoItemFeira)Random.Range(3,6):(TipoItemFeira)ProximaFruta();CriarItem(tipo,new Vector2(Random.Range(limitesHorizontais.x,limitesHorizontais.y),alturaSurgimento),velocidadeQueda);}
            quest.aberturaCesto.GetWorldCorners(cantos);
            var minimo=(Vector2)camada.InverseTransformPoint(cantos[0]);var maximo=(Vector2)camada.InverseTransformPoint(cantos[2]);
            // Somente atravessar a abertura por cima captura. Corpo, cauda e pes nao participam.
            for(int i=itens.Count-1;i>=0&&minijogo.EmPartida;i--){
                var item=itens[i];var r=item.retangulo;var antes=r.anchoredPosition;var depois=antes+Vector2.down*item.velocidade*intervalo;
                r.anchoredPosition=depois;float metade=r.rect.height*.5f;
                float topo=maximo.y;
                if(antes.y-metade>=topo&&depois.y-metade<=topo){
                    float t=Mathf.InverseLerp(antes.y-metade,depois.y-metade,topo);float x=Mathf.Lerp(antes.x,depois.x,t);
                    if(x>=minimo.x&&x<=maximo.x){Resolver(item.id,true);continue;}
                }
                if(depois.y-metade<=alturaChao)Resolver(item.id,false);
            }
        }
    }
}
