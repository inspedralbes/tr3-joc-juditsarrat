using UnityEngine;

public class MovementController : MonoBehaviour
{
    public new Rigidbody2D rigidbody { get; private set; }
    private Vector2 direction = Vector2.zero;
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

    public string playerId;
    public bool isLocalPlayer = false;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        if (rigidbody != null) {
            rigidbody.bodyType = RigidbodyType2D.Dynamic;
            rigidbody.gravityScale = 0;
            rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        activeSpriteRenderer = spriteRendererDown;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        bool up = Input.GetKey(inputUp) || Input.GetKey(KeyCode.UpArrow);
        bool down = Input.GetKey(inputDown) || Input.GetKey(KeyCode.DownArrow);
        bool left = Input.GetKey(inputLeft) || Input.GetKey(KeyCode.LeftArrow);
        bool right = Input.GetKey(inputRight) || Input.GetKey(KeyCode.RightArrow);

        if (up) SetDirection(Vector2.up, spriteRendererUp);
        else if (down) SetDirection(Vector2.down, spriteRendererDown);
        else if (left) SetDirection(Vector2.left, spriteRendererLeft);
        else if (right) SetDirection(Vector2.right, spriteRendererRight);
        else SetDirection(Vector2.zero, activeSpriteRenderer);
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer) return;

        Vector2 translation = direction * speed * Time.fixedDeltaTime;
        rigidbody.MovePosition(rigidbody.position + translation);

        if (direction != Vector2.zero && GameManager.Instance != null && !GameManager.Instance.isTraining) {
            SendMovementToServer(rigidbody.position);
        }
    }

    private void SendMovementToServer(Vector2 newPosition)
    {
        if (Time.time - lastSentTime > 0.03f)
        {
            
            string x = newPosition.x.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string y = newPosition.y.ToString(System.Globalization.CultureInfo.InvariantCulture);
            string sId = GameManager.Instance != null ? GameManager.Instance.sessionId : "";
            string json = "{\"type\":\"player-move\",\"playerId\":\"" + playerId + "\",\"sessionId\":\"" + sId + "\",\"x\":" + x + ",\"y\":" + y + "}";
            WebSocketManager.Instance.SendRaw(json);
            lastSentTime = Time.time;
        }
    }

    public void SetDirection(Vector2 newDirection, AnimatedSpriteRenderer spriteRenderer)
    {
        direction = newDirection;
        spriteRendererUp.enabled = spriteRenderer == spriteRendererUp;
        spriteRendererDown.enabled = spriteRenderer == spriteRendererDown;
        spriteRendererLeft.enabled = spriteRenderer == spriteRendererLeft;
        spriteRendererRight.enabled = spriteRenderer == spriteRendererRight;

        activeSpriteRenderer = spriteRenderer;
        activeSpriteRenderer.idle = direction == Vector2.zero;
    }

    // Llamado por GameManager para animar el jugador remoto
    public void ApplyRemoteMovement(Vector2 dir)
    {
        if (spriteRendererDown == null) return; // sin referencias, salir

        if (dir.magnitude < 0.01f)
        {
            if (activeSpriteRenderer != null) activeSpriteRenderer.idle = true;
            return;
        }

        AnimatedSpriteRenderer target;
        if (Mathf.Abs(dir.y) > Mathf.Abs(dir.x))
            target = dir.y > 0 ? spriteRendererUp : spriteRendererDown;
        else
            target = dir.x > 0 ? spriteRendererRight : spriteRendererLeft;

        if (target == null) return;

        spriteRendererUp.enabled    = (target == spriteRendererUp);
        spriteRendererDown.enabled  = (target == spriteRendererDown);
        spriteRendererLeft.enabled  = (target == spriteRendererLeft);
        spriteRendererRight.enabled = (target == spriteRendererRight);

        activeSpriteRenderer = target;
        activeSpriteRenderer.idle = false; // arranca la animacion de caminar
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Explosion") || other.GetComponent<Explosion>() != null)
        {
            // Permitimos la muerte del jugador local incluso en entrenamiento 
            // (para que pueda ver el Game Over si está probando la escena).
            if (GameManager.Instance != null && GameManager.Instance.isTraining && !isLocalPlayer) return;
            
            if (GetComponent<SimpleAgent>() == null) DeathSequence();
        }
    }

    private void DeathSequence()
    {
        enabled = false;
        if (spriteRendererDeath != null) {
            spriteRendererUp.enabled = false;
            spriteRendererDown.enabled = false;
            spriteRendererLeft.enabled = false;
            spriteRendererRight.enabled = false;
            spriteRendererDeath.enabled = true;
        }
        Invoke(nameof(OnDeathSequenceEnded), 1.25f);
    }

    private void OnDeathSequenceEnded()
    {
        gameObject.SetActive(false);
        
        string idToReport = playerId;
        if (isLocalPlayer && GameManager.Instance != null) {
            idToReport = GameManager.Instance.localPlayerId;
        }

        GameManager.Instance?.OnPlayerDeath(idToReport);
    }
}
