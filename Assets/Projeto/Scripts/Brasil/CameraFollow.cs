using UnityEngine;

namespace NexaQuest.Brasil
{
    [RequireComponent(typeof(Camera))]
    public class CameraFollow : MonoBehaviour
    {
        public Transform target;
        public SpriteRenderer mapBounds;
        [Min(0.001f)] public float smoothTime = 0.06f;
        [Min(1)] public int pixelsPerUnit = 100;
        Vector3 velocity;
        Vector3 smoothPosition;
        Camera view;

        void Awake() { view = GetComponent<Camera>(); smoothPosition = transform.position; }
        void LateUpdate()
        {
            if (target == null) return;
            smoothPosition = Vector3.SmoothDamp(smoothPosition, DesiredPosition(), ref velocity, smoothTime);
            transform.position = Snap(ClampToMap(smoothPosition));
        }
        Vector3 DesiredPosition() { return ClampToMap(new Vector3(target.position.x, target.position.y, -10f)); }
        Vector3 ClampToMap(Vector3 p)
        {
            if (view == null) view = GetComponent<Camera>();
            if (mapBounds == null) return p;
            Bounds b = mapBounds.bounds;
            float h = view.orthographicSize;
            float w = h * view.aspect;
            p.x = b.size.x <= w * 2 ? b.center.x : Mathf.Clamp(p.x, b.min.x + w, b.max.x - w);
            p.y = b.size.y <= h * 2 ? b.center.y : Mathf.Clamp(p.y, b.min.y + h, b.max.y - h);
            return p;
        }
        Vector3 Snap(Vector3 p)
        {
            p.x = Mathf.Round(p.x * pixelsPerUnit) / pixelsPerUnit;
            p.y = Mathf.Round(p.y * pixelsPerUnit) / pixelsPerUnit;
            return p;
        }
        public void SnapToTarget()
        {
            if (target == null) return;
            velocity = Vector3.zero;
            smoothPosition = DesiredPosition();
            transform.position = Snap(smoothPosition);
        }
    }
}