using UnityEngine;

public class RemotePlayerController : MonoBehaviour
{
    private string playerId;
    private Rigidbody2D rb;
    private Vector2 targetPosition;
    public float speed = 5f;

    [Header("Animations")]
    public AnimatedSpriteRenderer spriteRendererUp;
    public AnimatedSpriteRenderer spriteRendererDown;
    public AnimatedSpriteRenderer spriteRendererLeft;
    public AnimatedSpriteRenderer spriteRendererRight;
    private AnimatedSpriteRenderer activeSpriteRenderer;
    
    public void Initialize(string id)
    {
        playerId = id;
        rb = GetComponent<Rigidbody2D>();
        targetPosition = transform.position;
        // Posicionamiento inmediato inicial
        if (rb != null) rb.position = transform.position;
        
        activeSpriteRenderer = spriteRendererDown;
    }
    
    public void MoveToPosition(Vector2 newPosition)
    {
        targetPosition = newPosition;
    }
    
    private void FixedUpdate()
    {
        if (rb == null) return;

        float distance = Vector2.Distance(targetPosition, rb.position);
        
        // Si la distancia es muy grande (lag), teletransportar inmediatamente
        if (distance > 2.0f)
        {
            rb.position = targetPosition;
            rb.linearVelocity = Vector2.zero;
            UpdateAnimation(Vector2.zero);
            return;
        }

        // Si ya estamos muy cerca, detenerse (Zona muerta)
        if (distance < 0.05f)
        {
            rb.linearVelocity = Vector2.zero;
            UpdateAnimation(Vector2.zero);
            return;
        }

        // Calcular dirección de movimiento para la animación
        Vector2 moveDirection = (targetPosition - rb.position).normalized;
        UpdateAnimation(moveDirection);

        // Mover gradualmente hacia la posición objetivo usando MovePosition
        Vector2 nextPos = Vector2.MoveTowards(rb.position, targetPosition, speed * Time.fixedDeltaTime);
        rb.MovePosition(nextPos);
    }

    private void UpdateAnimation(Vector2 dir)
    {
        if (spriteRendererUp == null) return; // Si no están asignados, ignoramos

        if (dir == Vector2.zero)
        {
            if (activeSpriteRenderer != null) activeSpriteRenderer.idle = true;
            return;
        }

        AnimatedSpriteRenderer nextRenderer = activeSpriteRenderer;

        // Determinar qué renderer activar basado en la dirección dominante
        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
        {
            nextRenderer = (dir.y > 0) ? spriteRendererUp : spriteRendererDown;
        }
        else
        {
            nextRenderer = (dir.x > 0) ? spriteRendererRight : spriteRendererLeft;
        }

        if (nextRenderer != activeSpriteRenderer)
        {
            if (spriteRendererUp != null) spriteRendererUp.enabled = (nextRenderer == spriteRendererUp);
            if (spriteRendererDown != null) spriteRendererDown.enabled = (nextRenderer == spriteRendererDown);
            if (spriteRendererLeft != null) spriteRendererLeft.enabled = (nextRenderer == spriteRendererLeft);
            if (spriteRendererRight != null) spriteRendererRight.enabled = (nextRenderer == spriteRendererRight);
            
            activeSpriteRenderer = nextRenderer;
        }

        if (activeSpriteRenderer != null) activeSpriteRenderer.idle = false;
    }
}