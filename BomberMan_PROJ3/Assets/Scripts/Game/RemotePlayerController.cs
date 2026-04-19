using UnityEngine;

public class RemotePlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 targetPosition;
    public float speed = 10f;

    [Header("Animations")]
    public AnimatedSpriteRenderer spriteRendererUp;
    public AnimatedSpriteRenderer spriteRendererDown;
    public AnimatedSpriteRenderer spriteRendererLeft;
    public AnimatedSpriteRenderer spriteRendererRight;
    public AnimatedSpriteRenderer spriteRendererDeath;
    private AnimatedSpriteRenderer activeSpriteRenderer;

    private float lastMoveTime;
    private bool initialized = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;
        targetPosition = transform.position;
    }

    private void Start()
    {
        InitSprites();
    }

    private void InitSprites()
    {
        if (initialized) return;

        MovementController mc = GetComponent<MovementController>();
        if (mc != null)
        {
            if (spriteRendererUp    == null) spriteRendererUp    = mc.spriteRendererUp;
            if (spriteRendererDown  == null) spriteRendererDown  = mc.spriteRendererDown;
            if (spriteRendererLeft  == null) spriteRendererLeft  = mc.spriteRendererLeft;
            if (spriteRendererRight == null) spriteRendererRight = mc.spriteRendererRight;
            if (spriteRendererDeath == null) spriteRendererDeath = mc.spriteRendererDeath;
        }

        activeSpriteRenderer = spriteRendererDown;
        initialized = true;
    }

    // ─── Llamado desde GameManager cuando llega un mensaje de red ───────────
    public void MoveToPosition(Vector2 newPosition)
    {
        if (!initialized) InitSprites();

        Vector2 currentPos = rb != null ? rb.position : (Vector2)transform.position;
        Vector2 dir = (newPosition - currentPos);

        if (dir.magnitude > 0.01f)
        {
            lastMoveTime = Time.time;
            PlayWalkAnimation(dir.normalized);
        }

        targetPosition = newPosition;
    }

    public void PlayDeathAnimation()
    {
        if (rb != null) rb.simulated = false;
        if (spriteRendererUp    != null) spriteRendererUp.enabled    = false;
        if (spriteRendererDown  != null) spriteRendererDown.enabled  = false;
        if (spriteRendererLeft  != null) spriteRendererLeft.enabled  = false;
        if (spriteRendererRight != null) spriteRendererRight.enabled = false;
        if (spriteRendererDeath != null) spriteRendererDeath.enabled = true;
    }

    private void FixedUpdate()
    {
        if (rb == null) return;

        float distance = Vector2.Distance(targetPosition, rb.position);

        if (distance > 3f)
        {
            rb.position = targetPosition;
            return;
        }

        // Moverse hacia el target
        if (distance > 0.01f)
        {
            Vector2 nextPos = Vector2.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);
            rb.MovePosition(nextPos);
        }

       
        if (Time.time - lastMoveTime > 0.15f)
        {
            if (activeSpriteRenderer != null && !activeSpriteRenderer.idle)
                activeSpriteRenderer.idle = true;
        }
    }

    // ─── Activa el sprite correcto y pone en marcha la animación ────────────
    private void PlayWalkAnimation(Vector2 dir)
    {
        AnimatedSpriteRenderer nextRenderer;

        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
            nextRenderer = (dir.y > 0) ? spriteRendererUp : spriteRendererDown;
        else
            nextRenderer = (dir.x > 0) ? spriteRendererRight : spriteRendererLeft;

        if (nextRenderer == null) return;

        // Mostrar solo el sprite de la dirección correcta
        if (spriteRendererUp    != null) spriteRendererUp.enabled    = (nextRenderer == spriteRendererUp);
        if (spriteRendererDown  != null) spriteRendererDown.enabled  = (nextRenderer == spriteRendererDown);
        if (spriteRendererLeft  != null) spriteRendererLeft.enabled  = (nextRenderer == spriteRendererLeft);
        if (spriteRendererRight != null) spriteRendererRight.enabled = (nextRenderer == spriteRendererRight);

        // Desactivar idle -> se reproducirá la animación de caminar
        activeSpriteRenderer = nextRenderer;
        activeSpriteRenderer.idle = false;
    }
}