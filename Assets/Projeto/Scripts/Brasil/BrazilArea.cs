using UnityEngine;

namespace NexaQuest.Brasil
{
    public class BrazilArea : MonoBehaviour
    {
        public SpriteRenderer map;
        public Transform defaultSpawn;
        public bool ContainsSpawn(Transform spawn)
        {
            return spawn != null && spawn.IsChildOf(transform);
        }
    }
}