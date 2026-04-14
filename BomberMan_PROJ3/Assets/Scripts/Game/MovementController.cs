using UnityEngine;

public class MovementController : MonoBehaviour
{
    public new Rigidbody2D rigidbody { get; private set; }
    private Vector2 direction = Vector2.down;
    public float speed = 5f;

    public KeyCode inputUp = KeyCode.W;
    public KeyCode inputDown = KeyCode.S;
    public KeyCode inputLeft = KeyCode.A;
    public KeyCode inputRight = KeyCode.D;

    private float lastSentTime = 0f; 

    public AnimatedSpriteRenderer spriteRendererUp;
    public AnimatedSpriteRenderer spriteRendererDown;
    public AnimatedSpriteRenderer spriteRendererLeft;
    public AnimatedSpriteRenderer spriteRendererRight;
    public AnimatedSpriteRenderer spriteRendererDeath;
    private AnimatedSpriteRenderer activeSpriteRenderer;


    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        activeSpriteRenderer = spriteRendererDown;
    }
    

    private void Update()
    {
        if (Input.GetKey(inputUp))
        {
            SetDirection(Vector2.up , spriteRendererUp);
        } else if (Input.GetKey(inputDown))
        {
            SetDirection(Vector2.down, spriteRendererDown);
        } else if (Input.GetKey(inputLeft))
        {
            SetDirection(Vector2.left , spriteRendererLeft);
        } else if (Input.GetKey(inputRight))
        {
            SetDirection(Vector2.right , spriteRendererRight);
        } else if (Input.GetKeyDown(KeyCode.Space)) 
        { 
          PlaceBomb();}
        else
        {
          SetDirection(Vector2.zero, activeSpriteRenderer);   
        }
}


private void FixedUpdate()
{
    Vector2 position = rigidbody.position;
    Vector2 translation = direction * speed * Time.fixedDeltaTime;
    rigidbody.MovePosition(position + translation);
    
    // Si es va a moure i te WebSocket, enviar al servidor
    if (direction != Vector2.zero && WebSocketManager.Instance != null)
    {
        SendMovementToServer(position + translation);
    }
}

private void SendMovementToServer(Vector2 newPosition)
{
    // Enviar cada 0.1 segons per no saturar
    if (Time.time - lastSentTime > 0.1f)
    {
        PositionalMessage msg = new PositionalMessage { x = newPosition.x, y = newPosition.y };
        string json = JsonUtility.ToJson(msg);
        WebSocketManager.Instance.SendMessage("player-move", json);
        lastSentTime = Time.time;
    }
}


private void SetDirection(Vector2 newDirection, AnimatedSpriteRenderer spriteRenderer)
    {
        direction = newDirection;

       spriteRendererUp.enabled = spriteRenderer == spriteRendererUp;
       spriteRendererDown.enabled = spriteRenderer == spriteRendererDown;
       spriteRendererLeft.enabled = spriteRenderer == spriteRendererLeft;
       spriteRendererRight.enabled = spriteRenderer == spriteRendererRight;

        activeSpriteRenderer = spriteRenderer;
        activeSpriteRenderer.idle = direction == Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Explosion")){
            DeathSequence();
        }
    }

    private void DeathSequence()
    {
        enabled = false;
        GetComponent<BombController>().enabled = false ;

        spriteRendererUp.enabled = false;
        spriteRendererDown.enabled = false;
        spriteRendererLeft.enabled = false;
        spriteRendererRight.enabled = false;
        spriteRendererDeath.enabled = true;

        Invoke(nameof(OnDeathSequenceEnded), 1.25f);


    }

  [SerializeField] private GameObject bombPrefab;

private void PlaceBomb()
{
    if (bombPrefab == null)
    {
        Debug.LogError("[Movement] bombPrefab no asignado en Inspector");
        return;
    }
    
    Vector3 bombPos = transform.position;
    
    // Crear bomba localmente
    Instantiate(bombPrefab, bombPos, Quaternion.identity);
    Debug.Log("[Movement] Bomba creada en: " + bombPos);
    
    // Enviar al servidor
    string data = JsonUtility.ToJson(new { x = bombPos.x, y = bombPos.y });
    if (WebSocketManager.Instance != null)
    {
        WebSocketManager.Instance.SendMessage("place-bomb", data);
    }
}    private void OnDeathSequenceEnded()
    {
      gameObject.SetActive(false);
      FindObjectOfType<GameManager>().CheckWinState();
    }
} 