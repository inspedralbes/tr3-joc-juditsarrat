using UnityEngine;

public class RemotePlayerController : MonoBehaviour
{
    private string playerId;
    private Rigidbody2D rb;
    private Vector2 targetPosition;
    public float speed = 5f;
    
    public void Initialize(string id)
    {
        playerId = id;
        rb = GetComponent<Rigidbody2D>();
        targetPosition = transform.position;
    }
    
    public void MoveToPosition(Vector2 newPosition)
    {
        targetPosition = newPosition;
    }
    
    private void FixedUpdate()
    {
        // Interpolación suave hacia la posición destino
        Vector2 direction = (targetPosition - rb.position).normalized;
        rb.linearVelocity = direction * speed;
    }
}