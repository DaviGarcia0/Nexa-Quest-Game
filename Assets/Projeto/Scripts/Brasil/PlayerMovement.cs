using UnityEngine;

namespace NexaQuest.Brasil
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
    public class PlayerMovement : MonoBehaviour
    {
        [Min(0f)] public float moveSpeed = 2f;
        public SpriteRenderer areaBounds;
        [Min(0.01f)] public float walkAnimationSpeed = 1f;
        [Min(0.01f)] public float idleAnimationSpeed = 1f;
        public bool ControlsLocked { get; private set; }
        public Vector2 MoveInput { get; private set; }
        public int FacingDirection { get; private set; }
        Rigidbody2D body;
        Animator animator;
        Bounds movementBounds;
        bool hasMovementBounds;
        static readonly int Direction = Animator.StringToHash("Direction");
        static readonly int Speed = Animator.StringToHash("Speed");
        static readonly int WalkRate = Animator.StringToHash("WalkRate");
        static readonly int IdleRate = Animator.StringToHash("IdleRate");

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }

        void Update()
        {
            SetMovementInput(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
        }

        // Separado da leitura do teclado para permitir testes do mesmo movimento.
        public void SetMovementInput(Vector2 input)
        {
            MoveInput = ControlsLocked ? Vector2.zero : Vector2.ClampMagnitude(input, 1f);
            if (MoveInput.sqrMagnitude > 0.001f)
            {
                // Diagonais mantem uma das quatro poses, com prioridade horizontal.
                FacingDirection = Mathf.Abs(MoveInput.x) >= Mathf.Abs(MoveInput.y)
                    ? (MoveInput.x < 0f ? 1 : 2) : (MoveInput.y < 0f ? 0 : 3);
            }
            animator.SetInteger(Direction, FacingDirection);
            animator.SetFloat(Speed, MoveInput.sqrMagnitude);
            animator.SetFloat(WalkRate, walkAnimationSpeed);
            animator.SetFloat(IdleRate, idleAnimationSpeed);
        }

        void FixedUpdate()
        {
            Vector2 next = body.position + MoveInput * moveSpeed * Time.fixedDeltaTime;
            // Limite retangular da area, sem criar colisores de cenario.
            if (areaBounds != null)
            {
                Bounds bounds = hasMovementBounds ? movementBounds : areaBounds.bounds;
                next.x = Mathf.Clamp(next.x, bounds.min.x + 0.25f, bounds.max.x - 0.25f);
                next.y = Mathf.Clamp(next.y, bounds.min.y + 0.05f, bounds.max.y - 0.55f);
            }
            body.MovePosition(next);
        }

        public void SetControlsLocked(bool locked)
        {
            ControlsLocked = locked;
            SetMovementInput(Vector2.zero);
            body.velocity = Vector2.zero;
        }

        public void Teleport(Vector3 position)
        {
            body.velocity = Vector2.zero;
            body.position = position;
            transform.position = position;
        }

        // Inclui as saidas posicionadas na borda sem mover os triggers do cenario.
        public void SetAreaBounds(SpriteRenderer map)
        {
            areaBounds = map;
            hasMovementBounds = map != null;
            if (map == null) return;
            movementBounds = map.bounds;
            var area = map.GetComponentInParent<BrazilArea>();
            foreach (var exit in area.GetComponentsInChildren<MapTransition>())
            {
                var trigger = exit.GetComponent<BoxCollider2D>();
                if (!trigger.enabled || !trigger.isTrigger) continue;
                var bounds = trigger.bounds;
                bounds.Expand(new Vector3(0.5f, 1.1f, 0));
                movementBounds.Encapsulate(bounds);
            }
        }

        void OnDisable()
        {
            MoveInput = Vector2.zero;
            if (body != null) body.velocity = Vector2.zero;
        }
    }
}
