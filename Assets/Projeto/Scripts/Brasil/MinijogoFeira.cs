using UnityEngine;
using UnityEngine.EventSystems;
namespace NexaQuest.Brasil
{
    public enum EstadoMinijogoFeira { Fechado, Instrucoes, Jogando, Vitoria, Derrota }
    public class MinijogoFeira : MonoBehaviour
    {
        public BrazilWorld mundo;
        public QuestCestoFeira quest;
        public QuedaItensFeira queda;
        public InterfaceMinijogoFeira interfaceJogo;
        public EstadoMinijogoFeira Estado {get;private set;}=EstadoMinijogoFeira.Fechado;
        public bool EmPartida=>Estado==EstadoMinijogoFeira.Jogando;
        public bool ObjetivoConcluido {get;private set;}
        public bool ChaveConquistadaNestaPartida {get;private set;}
        public int FrutasPerdidas {get;private set;}
        public int ItensErrados {get;private set;}
        public int FrutasColetadas {get;private set;}
        public int MoedasDaPartida {get;private set;}
        readonly int[] frutas=new int[3];
        bool possuiBloqueio;
        ProgressionManager Progresso=>ProgressionManager.Instance;
        public int Quantidade(TipoItemFeira tipo)=>((int)tipo<3)?frutas[(int)tipo]:0;
        public bool TentarAbrir(PainelDesafiosFeira origem){
            if(Estado!=EstadoMinijogoFeira.Fechado||origem==null||!origem.EstaAberto||mundo.IsTransitioning||Progresso==null)return false;
            if(Progresso.Energia<20){interfaceJogo.AvisarEnergia();return false;}
            origem.Fechar();gameObject.SetActive(true);possuiBloqueio=true;mundo.player.SetControlsLocked(true);
            interfaceJogo.Preparar();quest.Reiniciar();
            if(Progresso.InstrucoesFeiraVistas){Estado=EstadoMinijogoFeira.Instrucoes;IniciarPartida();}
            else AlterarEstado(EstadoMinijogoFeira.Instrucoes);
            return true;
        }
        public void IniciarPartida(){
            if(Estado!=EstadoMinijogoFeira.Instrucoes&&Estado!=EstadoMinijogoFeira.Vitoria&&Estado!=EstadoMinijogoFeira.Derrota)return;
            if(!Progresso.TentarGastarEnergia(20)){interfaceJogo.AvisarEnergia();return;}
            Progresso.MarcarInstrucoesFeiraVistas();
            for(int i=0;i<3;i++)frutas[i]=0;
            FrutasPerdidas=ItensErrados=FrutasColetadas=MoedasDaPartida=0;
            ObjetivoConcluido=ChaveConquistadaNestaPartida=false;
            queda.Reiniciar();quest.Reiniciar();AtualizarHUD();AlterarEstado(EstadoMinijogoFeira.Jogando);
        }
        // Chamado exclusivamente apos um item ser removido da lista ativa de queda.
        public void RegistrarItem(TipoItemFeira tipo,bool capturado){
            if(!EmPartida)return;
            bool fruta=(int)tipo<3;
            if(fruta&&capturado){
                frutas[(int)tipo]=Mathf.Min(5,frutas[(int)tipo]+1);FrutasColetadas++;
                if(FrutasColetadas%3==0){MoedasDaPartida++;Progresso.AddCoins(1);}
                if(!ObjetivoConcluido&&frutas[0]==5&&frutas[1]==5&&frutas[2]==5){
                    ObjetivoConcluido=true;ChaveConquistadaNestaPartida=Progresso.ConquistarChaveVila();Progresso.AddXP(50);
                }
            }else if(fruta||capturado){
                if(fruta)FrutasPerdidas++;else ItensErrados++;
                interfaceJogo.Impacto();
            }
            AtualizarHUD();
            if(FrutasPerdidas>=3||ItensErrados>=3)FinalizarPartida();
        }
        void AtualizarHUD()=>interfaceJogo.Atualizar(frutas,FrutasPerdidas,ItensErrados);
        void AlterarEstado(EstadoMinijogoFeira estado){Estado=estado;interfaceJogo.Mostrar(estado,ChaveConquistadaNestaPartida,MoedasDaPartida);if(EventSystem.current!=null)EventSystem.current.SetSelectedGameObject(null);}
        public void FinalizarPartida(){
            if(!EmPartida)return;
            queda.Limpar();AlterarEstado(ObjetivoConcluido?EstadoMinijogoFeira.Vitoria:EstadoMinijogoFeira.Derrota);quest.Mover(0,0);
        }
        public void Sair(){
            if(EmPartida){FinalizarPartida();return;}
            if(Estado==EstadoMinijogoFeira.Fechado)return;
            Estado=EstadoMinijogoFeira.Fechado;gameObject.SetActive(false);
        }
        void Update(){if(Input.GetKeyDown(KeyCode.Escape))Sair();}
        void OnDisable(){
            Estado=EstadoMinijogoFeira.Fechado;
            if(queda!=null)queda.Limpar();if(interfaceJogo!=null)interfaceJogo.Restaurar();
            if(possuiBloqueio&&mundo!=null&&!mundo.IsTransitioning)mundo.player.SetControlsLocked(false);
            possuiBloqueio=false;
        }
    }
}
