using UnityEngine;
using UnityEngine.EventSystems;

namespace NexaQuest.Brasil
{
    public class PainelDesafiosFeira : MonoBehaviour
    {
        [SerializeField] private BrazilWorld mundo;
        public MinijogoFeira minijogo;
        public QuizFeira quiz;
        int quadroAbertura = -1;
        private bool possuiBloqueio;
        public bool EstaAberto => gameObject.activeInHierarchy && possuiBloqueio;

        public void Abrir()
        {
            if (EstaAberto || mundo == null || mundo.player == null ||
                mundo.IsTransitioning || mundo.player.ControlsLocked) return;
            gameObject.SetActive(true);
            possuiBloqueio = true;
            quadroAbertura = Time.frameCount;
            mundo.player.SetControlsLocked(true);
            if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
        }

        private void Update() { if(!NexaQuest.PauseSettingsManager.GameplayBlocked) ProcessarFechamento(Input.GetKeyDown(KeyCode.Escape)); }

        // A mesma entrada pode ser exercitada pelos testes de Play Mode.
        public void ProcessarFechamento(bool pressionouEsc)
        {
            if (pressionouEsc && EstaAberto && Time.frameCount > quadroAbertura) Fechar();
        }

        public void Fechar()
        {
            if (EstaAberto) gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (possuiBloqueio && mundo != null && mundo.player != null && !mundo.IsTransitioning)
                mundo.player.SetControlsLocked(false);
            possuiBloqueio = false;
            var eventos = EventSystem.current;
            if (eventos != null && eventos.currentSelectedGameObject != null &&
                eventos.currentSelectedGameObject.transform.IsChildOf(transform))
                eventos.SetSelectedGameObject(null);
        }

        public void SelecionarQuiz()
        {
            if (EstaAberto && quiz != null) quiz.TentarAbrir(this);
        }

        public void SelecionarMinijogo()
        {
            if (EstaAberto && minijogo != null) minijogo.TentarAbrir(this);
        }
    }
}
