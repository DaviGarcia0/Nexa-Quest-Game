using UnityEngine;
using UnityEngine.Events;

namespace NexaQuest.Brasil
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class NPCInteraction : MonoBehaviour
    {
        public DialogueController controller;
        public DialogueData dialogue;
        [Tooltip("Vazio nesta etapa. Pode abrir uma selecao de desafios no futuro.")]
        public UnityEvent onDialogueFinished = new UnityEvent();
        void Reset() { GetComponent<CircleCollider2D>().isTrigger = true; }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (controller != null && other.GetComponentInParent<PlayerMovement>() == controller.world.player)
                controller.Register(this);
        }
        void OnTriggerExit2D(Collider2D other)
        {
            if (controller != null && other.GetComponentInParent<PlayerMovement>() == controller.world.player)
                controller.Unregister(this);
        }
        void OnDisable() { if (controller != null) controller.Unregister(this); }
    }
}
