using UnityEngine;

namespace NexaQuest.Brasil
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class MapTransition : MonoBehaviour
    {
        public BrazilWorld world;
        public BrazilArea destinationArea;
        public Transform destinationSpawnPoint;

        void Reset() { GetComponent<BoxCollider2D>().isTrigger = true; }
        void OnTriggerEnter2D(Collider2D other)
        {
            if (world == null || !GetComponent<BoxCollider2D>().isTrigger) return;
            PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
            if (player != null && player == world.player)
                world.TravelTo(destinationArea, destinationSpawnPoint);
        }
    }
}