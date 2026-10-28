using System.Collections;
using UnityEngine;
namespace NexaQuest.MinhaQuest
{
    public class QuestFeedingController : MonoBehaviour
    {
        public Animator animator;
        // Referência serializada existente, agora aponta para HeadFeedingZone.
        public RectTransform mouth;
        public float eatingDuration = 1.6f;
        public bool Eating { get; private set; }
        public bool MouthOpen { get; private set; }
        public bool Contains(Vector2 screen, Camera camera)
        {
            Vector2 local;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(mouth, screen, camera, out local)) return false;
            Rect r = mouth.rect;
            return r.Contains(local);
        }
        public void Approach(bool near)
        {
            if (Eating || MouthOpen == near) return;
            MouthOpen = near; animator.Play(near ? "MouthOpen" : "Idle", 0, 0);
        }
        public void Eat()
        {
            if (Eating) return;
            Eating = true; MouthOpen = false;
            // Interrompe MouthOpen e aplica o primeiro frame de Eating nesta soltura.
            animator.Play("Eating", 0, 0); animator.Update(0);
            StartCoroutine(Finish());
        }
        IEnumerator Finish()
        {
            float elapsed = 0;
            while (elapsed < eatingDuration) { elapsed += Time.deltaTime; yield return null; }
            Eating = false; animator.Play("Idle", 0, 0);
        }
    }
}
