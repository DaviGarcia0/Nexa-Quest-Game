using TMPro;
using UnityEngine;
namespace NexaQuest.Brasil
{
    public class InterfaceMinijogoFeira : MonoBehaviour
    {
        public RectTransform visual;
        public Transform camadaHUD;
        public Transform[] hudsCompartilhadas;
        public GameObject instrucoes, vitoriaComChave, vitoriaSemChave, derrota;
        public TMP_Text[] frutas;
        public TMP_Text frutasPerdidas,itensErrados,moedasPrimeiraVitoria,moedasOutrasVitorias,avisoEntrada,avisoPartida;
        [Range(.1f,.2f)]public float duracaoImpacto=.14f;
        [Range(0,8)]public float intensidadeImpacto=4;
        Transform[] pais;int[] indices;
        Vector2 origemVisual;float impactoAte;
        bool hudsEmprestadas;
        public void Preparar(){
            origemVisual=visual.anchoredPosition;impactoAte=0;
            pais=new Transform[hudsCompartilhadas.Length];indices=new int[pais.Length];
            for(int i=0;i<pais.Length;i++){pais[i]=hudsCompartilhadas[i].parent;indices[i]=hudsCompartilhadas[i].GetSiblingIndex();}
            for(int i=0;i<pais.Length;i++)hudsCompartilhadas[i].SetParent(camadaHUD,false);
            hudsEmprestadas=true;avisoEntrada.gameObject.SetActive(false);avisoPartida.gameObject.SetActive(false);
        }
        public void Restaurar(){
            visual.anchoredPosition=origemVisual;impactoAte=0;
            if(!hudsEmprestadas)return;
            // A ordem configurada segue a ordem original do Canvas.
            for(int i=0;i<pais.Length;i++)if(pais[i]!=null){hudsCompartilhadas[i].SetParent(pais[i],false);hudsCompartilhadas[i].SetSiblingIndex(indices[i]);}
            hudsEmprestadas=false;
        }
        public void Mostrar(EstadoMinijogoFeira estado,bool primeiraChave,int moedas){
            instrucoes.SetActive(estado==EstadoMinijogoFeira.Instrucoes);
            vitoriaComChave.SetActive(estado==EstadoMinijogoFeira.Vitoria&&primeiraChave);
            vitoriaSemChave.SetActive(estado==EstadoMinijogoFeira.Vitoria&&!primeiraChave);
            derrota.SetActive(estado==EstadoMinijogoFeira.Derrota);
            moedasPrimeiraVitoria.text=moedasOutrasVitorias.text="+"+moedas;
            avisoPartida.gameObject.SetActive(false);
        }
        public void Atualizar(int[] quantidades,int perdidas,int errados){
            for(int i=0;i<3;i++)frutas[i].text=Mathf.Min(5,quantidades[i])+"/5";
            frutasPerdidas.text=Mathf.Min(3,perdidas)+"/3";itensErrados.text=Mathf.Min(3,errados)+"/3";
        }
        public void AvisarEnergia(){
            var texto=gameObject.activeInHierarchy?avisoPartida:avisoEntrada;
            texto.text="Energia insuficiente. Você precisa de 20 para jogar.";texto.gameObject.SetActive(true);
        }
        public void Impacto(){impactoAte=Time.unscaledTime+duracaoImpacto;}
        void Update(){
            float restante=impactoAte-Time.unscaledTime;
            visual.anchoredPosition=restante>0?origemVisual+Random.insideUnitCircle*intensidadeImpacto*Mathf.Clamp01(restante/duracaoImpacto):origemVisual;
        }
        void OnDisable(){Restaurar();}
    }
}
