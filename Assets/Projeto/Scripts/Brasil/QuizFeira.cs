using UnityEngine;
using UnityEngine.EventSystems;

namespace NexaQuest.Brasil
{
    public enum EstadoQuizFeira { Fechado, Inicial, Pergunta, Erro, Acerto, Concluido }

    public class QuizFeira : MonoBehaviour
    {
        public BrazilWorld mundo;
        public DadosQuizFeira dados;
        public InterfaceQuizFeira interfaceQuiz;
        [Min(1)] public float duracaoPergunta = 30;
        [Min(.1f)] public float duracaoExibicaoFeedback = 1.2f;
        [Min(.1f)] public float duracaoFadeFeedback = .3f;

        public EstadoQuizFeira Estado { get; private set; } = EstadoQuizFeira.Fechado;
        public int PerguntaAtual { get; private set; }
        public float TempoRestante { get; private set; }
        public int MoedasDaRodada { get; private set; }
        public bool ChaveConquistadaNestaRodada { get; private set; }
        public bool RodadaConcluida { get; private set; }
#if UNITY_EDITOR
        public bool SimulacaoManual { get; set; }
#endif
        readonly bool[] perguntasPremiadas = new bool[3];
        PainelDesafiosFeira origem;
        bool possuiBloqueio, reiniciarTempo;
        float tempoFeedback;
        ProgressionManager Progresso => ProgressionManager.Instance;

        public bool TentarAbrir(PainelDesafiosFeira painel)
        {
            if (Estado != EstadoQuizFeira.Fechado || painel == null || !painel.EstaAberto ||
                mundo == null || mundo.IsTransitioning || Progresso == null ||
                painel.minijogo != null && painel.minijogo.Estado != EstadoMinijogoFeira.Fechado) return false;
            if (dados == null || !dados.EstaValido())
            {
                Debug.LogError("Quiz da Feira: configure exatamente três perguntas e quatro alternativas por pergunta.", this);
                return false;
            }
            origem = painel;
            painel.Fechar();
            gameObject.SetActive(true);
            possuiBloqueio = true;
            mundo.player.SetControlsLocked(true);
            interfaceQuiz.Preparar();
            MudarEstado(EstadoQuizFeira.Inicial);
            return true;
        }

        public void Comecar()
        {
            if (Estado != EstadoQuizFeira.Inicial) return;
            if (!Progresso.TentarGastarEnergia(20)) { interfaceQuiz.AvisarEnergia(); return; }
            for (int i = 0; i < perguntasPremiadas.Length; i++) perguntasPremiadas[i] = false;
            PerguntaAtual = MoedasDaRodada = 0;
            RodadaConcluida = ChaveConquistadaNestaRodada = false;
            interfaceQuiz.OcultarAviso();
            CarregarPergunta();
        }

        void CarregarPergunta()
        {
            TempoRestante = Mathf.Max(1, duracaoPergunta);
            interfaceQuiz.Carregar(dados.perguntas[PerguntaAtual], PerguntaAtual);
            interfaceQuiz.MostrarTempo(TempoRestante);
            MudarEstado(EstadoQuizFeira.Pergunta);
        }

        public void Responder(int alternativa)
        {
            if (Estado != EstadoQuizFeira.Pergunta || alternativa < 0 || alternativa > 3 ||
                perguntasPremiadas[PerguntaAtual]) return;
            bool acertou = alternativa == dados.perguntas[PerguntaAtual].respostaCorreta;
            reiniciarTempo = false;
            if (acertou)
            {
                perguntasPremiadas[PerguntaAtual] = true;
                MoedasDaRodada += 10;
                Progresso.AddCoins(10);
            }
            IniciarFeedback(acertou, false);
        }

        void IniciarFeedback(bool acerto, bool esgotou)
        {
            reiniciarTempo = esgotou;
            tempoFeedback = 0;
            MudarEstado(acerto ? EstadoQuizFeira.Acerto : EstadoQuizFeira.Erro);
            interfaceQuiz.MostrarFeedback(acerto, esgotou);
        }

        void MudarEstado(EstadoQuizFeira estado)
        {
            Estado = estado;
            interfaceQuiz.MostrarEstado(estado);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        void Update()
        {
            if (NexaQuest.PauseSettingsManager.GameplayBlocked) return;
            if (Input.GetKeyDown(KeyCode.Escape)) { ProcessarFechamento(true); return; }
            #if UNITY_EDITOR
            if (SimulacaoManual) return;
#endif
            Simular(Time.unscaledDeltaTime);
        }

        // O mesmo avanço temporal é utilizado pelo jogo e pela validação em Play Mode.
        public void Simular(float segundos)
        {
            segundos = Mathf.Max(0, segundos);
            if (Estado == EstadoQuizFeira.Pergunta)
            {
                TempoRestante = Mathf.Max(0, TempoRestante - segundos);
                interfaceQuiz.MostrarTempo(TempoRestante);
                if (TempoRestante <= 0) IniciarFeedback(false, true);
            }
            else if (Estado == EstadoQuizFeira.Erro || Estado == EstadoQuizFeira.Acerto)
            {
                tempoFeedback += segundos;
                float exibicao = Mathf.Max(.1f, duracaoExibicaoFeedback);
                float fade = Mathf.Max(.1f, duracaoFadeFeedback);
                interfaceQuiz.DefinirAlphaFeedback(1 - Mathf.Clamp01((tempoFeedback - exibicao) / fade));
                if (tempoFeedback < exibicao + fade) return;
                bool acertou = Estado == EstadoQuizFeira.Acerto;
                interfaceQuiz.OcultarFeedback();
                if (!acertou)
                {
                    if (reiniciarTempo) TempoRestante = Mathf.Max(1, duracaoPergunta);
                    interfaceQuiz.MostrarTempo(TempoRestante);
                    MudarEstado(EstadoQuizFeira.Pergunta);
                }
                else if (PerguntaAtual < 2) { PerguntaAtual++; CarregarPergunta(); }
                else Concluir();
            }
        }

        void Concluir()
        {
            if (RodadaConcluida) return;
            RodadaConcluida = true;
            Progresso.AddXP(50);
            Progresso.MarcarQuizFeiraConcluido();
            ChaveConquistadaNestaRodada = Progresso.ConquistarChaveVila();
            MudarEstado(EstadoQuizFeira.Concluido);
            interfaceQuiz.MostrarRecompensas(MoedasDaRodada, ChaveConquistadaNestaRodada);
        }

        public void ProcessarFechamento(bool pressionouEsc) { if (pressionouEsc) Voltar(); }

        public void Voltar()
        {
            if (Estado == EstadoQuizFeira.Fechado) return;
            var painel = origem;
            interfaceQuiz.Restaurar();
            gameObject.SetActive(false);
            if (painel != null) painel.Abrir();
        }

        void OnDisable()
        {
            Estado = EstadoQuizFeira.Fechado;
            tempoFeedback = 0;
            if (interfaceQuiz != null) interfaceQuiz.Restaurar();
            if (possuiBloqueio && mundo != null && !mundo.IsTransitioning) mundo.player.SetControlsLocked(false);
            possuiBloqueio = false;
        }
    }
}
