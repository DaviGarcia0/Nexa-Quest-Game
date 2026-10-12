using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NexaQuest.Brasil
{
    public class PainelDesafiosAmazonia : MonoBehaviour
    {
        public BrazilWorld mundo;
        public BrazilArea area;
        public QuizAmazonia quiz;
        public TMP_Text avisoEnergia, mensagem;
        bool possuiBloqueio, avisoEmprestado;
        int quadroAbertura;
        Transform paiAviso; int indiceAviso; Vector2 posicaoAviso;
        public bool EstaAberto => gameObject.activeInHierarchy && possuiBloqueio;
        public void Abrir()
        {
            if (EstaAberto || mundo == null || mundo.CurrentArea != area || mundo.IsTransitioning || mundo.player.ControlsLocked) return;
            gameObject.SetActive(true); possuiBloqueio = true; quadroAbertura = Time.frameCount;
            mundo.player.SetControlsLocked(true); mensagem.text = "Esc: voltar ao mapa";
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }
        void Update() => ProcessarFechamento(Input.GetKeyDown(KeyCode.Escape));
        public void ProcessarFechamento(bool esc) { if (esc && EstaAberto && Time.frameCount > quadroAbertura) Fechar(); }
        public void Fechar() { if (!EstaAberto) return; RestaurarAviso(); gameObject.SetActive(false); }
        public void SelecionarQuiz() { if (EstaAberto) quiz.TentarAbrir(this); }
        public void SelecionarMinijogo() { if (EstaAberto) mensagem.text = "Minijogo em desenvolvimento."; }
        public void AvisarEnergia()
        {
            if (!avisoEmprestado) {
                paiAviso = avisoEnergia.transform.parent; indiceAviso = avisoEnergia.transform.GetSiblingIndex();
                posicaoAviso = avisoEnergia.rectTransform.anchoredPosition;
                avisoEnergia.transform.SetParent(transform, false); avisoEnergia.transform.SetAsLastSibling();
                avisoEnergia.rectTransform.anchoredPosition = new Vector2(0, -510); avisoEmprestado = true;
            }
            avisoEnergia.text = "Energia insuficiente. Você precisa de 20 para jogar.";
            avisoEnergia.gameObject.SetActive(true);
        }
        void RestaurarAviso()
        {
            if (!avisoEmprestado) return;
            avisoEnergia.gameObject.SetActive(false); avisoEnergia.transform.SetParent(paiAviso, false);
            avisoEnergia.transform.SetSiblingIndex(indiceAviso); avisoEnergia.rectTransform.anchoredPosition = posicaoAviso;
            avisoEmprestado = false;
        }
        void OnDisable()
        {
            RestaurarAviso();
            if (possuiBloqueio && mundo != null && !mundo.IsTransitioning) mundo.player.SetControlsLocked(false);
            possuiBloqueio = false;
        }
    }
}