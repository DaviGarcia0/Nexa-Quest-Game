using UnityEngine;

namespace NexaQuest.Brasil
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class PortalInteraction : MonoBehaviour
    {
        public BrazilWorld world;
        public GameObject interactionPrompt;
        public GameObject keysPanel;
        public SpriteRenderer portalMap;
        [Tooltip("Referencia futura; nenhuma chave ou ativacao e implementada agora.")]
        public Sprite activatedPortal;
        public Sprite filledKeysPanel;
        public bool IsOpen { get; private set; }
        bool nearby;

        void Update()
        {
            HandleInput(Input.GetKeyDown(KeyCode.E), Input.GetKeyDown(KeyCode.Escape));
        }

        public void HandleInput(bool interact, bool cancel)
        {
            if (world == null || world.IsTransitioning) return;
            if (IsOpen && (interact || cancel)) ClosePanel();
            else if (nearby && interact && !world.player.ControlsLocked)
            {
                IsOpen = true;
                keysPanel.SetActive(true);
                interactionPrompt.SetActive(false);
                world.player.SetControlsLocked(true);
            }
        }

        public void ClosePanel()
        {
            bool wasOpen = IsOpen;
            IsOpen = false;
            if (keysPanel != null) keysPanel.SetActive(false);
            if (wasOpen && world != null && !world.IsTransitioning) world.player.SetControlsLocked(false);
            if (interactionPrompt != null) interactionPrompt.SetActive(nearby && isActiveAndEnabled);
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (world == null || other.GetComponentInParent<PlayerMovement>() != world.player) return;
            nearby = true;
            if (!IsOpen) interactionPrompt.SetActive(true);
        }
        void OnTriggerExit2D(Collider2D other)
        {
            if (world == null || other.GetComponentInParent<PlayerMovement>() != world.player) return;
            nearby = false;
            ClosePanel();
        }
        void OnDisable() { nearby = false; ClosePanel(); }
    }
}
