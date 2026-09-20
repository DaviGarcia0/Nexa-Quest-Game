using System.Collections;
using UnityEngine;

namespace NexaQuest.Brasil
{
    public class BrazilWorld : MonoBehaviour
    {
        public PlayerMovement player;
        public CameraFollow followCamera;
        public BrazilArea[] areas;
        public BrazilArea initialArea;
        public Transform initialSpawn;
        public CanvasGroup fadePanel;
        [Min(0f)] public float fadeDuration = 0.25f;
        public BrazilArea CurrentArea { get; private set; }
        public bool IsTransitioning { get; private set; }
        float nextTransitionTime;

        void Start()
        {
            if (!ValidDestination(initialArea, initialSpawn != null ? initialSpawn : initialArea != null ? initialArea.defaultSpawn : null))
            {
                Debug.LogError("Brasil: configure a area inicial, spawn, player, camera e fade no GameManager.", this);
                return;
            }
            fadePanel.alpha = 0f;
            fadePanel.blocksRaycasts = false;
            SwitchArea(initialArea, initialSpawn != null ? initialSpawn : initialArea.defaultSpawn);
        }

        bool ValidDestination(BrazilArea area, Transform spawn)
        {
            return player != null && followCamera != null && fadePanel != null && areas != null &&
                area != null && area.map != null && System.Array.IndexOf(areas, area) >= 0 && area.ContainsSpawn(spawn);
        }

        public bool TravelTo(BrazilArea destinationArea, Transform destinationSpawnPoint)
        {
            if (IsTransitioning || Time.unscaledTime < nextTransitionTime) return false;
            if (!ValidDestination(destinationArea, destinationSpawnPoint))
            {
                Debug.LogWarning("Brasil: destino invalido. Arraste uma area cadastrada e um Spawn Point filho dela.", this);
                return false;
            }
            StartCoroutine(Transition(destinationArea, destinationSpawnPoint));
            return true;
        }

        IEnumerator Transition(BrazilArea area, Transform spawn)
        {
            IsTransitioning = true;
            player.SetControlsLocked(true);
            fadePanel.blocksRaycasts = true;
            yield return FadeTo(1f);
            SwitchArea(area, spawn);
            yield return FadeTo(0f);
            fadePanel.blocksRaycasts = false;
            player.SetControlsLocked(false);
            nextTransitionTime = Time.unscaledTime + 0.2f;
            IsTransitioning = false;
        }

        IEnumerator FadeTo(float targetAlpha)
        {
            float start = fadePanel.alpha;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                fadePanel.alpha = Mathf.Lerp(start, targetAlpha, fadeDuration > 0f ? elapsed / fadeDuration : 1f);
                yield return null;
            }
            fadePanel.alpha = targetAlpha;
        }

        void SwitchArea(BrazilArea area, Transform spawn)
        {
            foreach (BrazilArea entry in areas)
                if (entry != null) entry.gameObject.SetActive(entry == area);
            CurrentArea = area;
            player.areaBounds = area.map;
            player.Teleport(spawn.position);
            followCamera.mapBounds = area.map;
            followCamera.SnapToTarget();
            Physics2D.SyncTransforms();
        }

        void OnDisable()
        {
            StopAllCoroutines();
            if (player != null) player.SetControlsLocked(false);
            if (fadePanel != null) { fadePanel.alpha = 0f; fadePanel.blocksRaycasts = false; }
            IsTransitioning = false;
        }
    }
}