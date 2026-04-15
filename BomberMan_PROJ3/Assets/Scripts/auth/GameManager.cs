using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    public GameObject[] players;

    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject remotePlayerPrefab;
    
    private GameObject localPlayer;
    private GameObject remotePlayer;
    private string localPlayerId;

    private string localPlayerName;      // ← NUEVO

    private string remotePlayerName;     // ← NUEVO
    private bool gameEnded = false; 
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    [System.Serializable]
    public class WsMessage {
        public string type;
        public string playerId;
        public float x;
        public float y;
        public float dataX; // Fallback
        public float dataY; // Fallback
    }
    
private void Start()
{
    localPlayerId = AuthManager.Instance?.JugadorActual?.id;
    localPlayerName = AuthManager.Instance?.JugadorActual?.username ?? "Jugador Local";
    remotePlayerName = "Rival"; // En una versió millorada, això vindria del servidor o lobby

    int index = AuthManager.Instance != null ? AuthManager.Instance.PlayerIndex : 0;
    Debug.Log($"[GameManager] Local: {localPlayerName} | Remote: {remotePlayerName}");

    // 1. Encontrar todos los jugadores
    MovementController[] allPlayers = FindObjectsByType<MovementController>(FindObjectsSortMode.None);
    System.Array.Sort(allPlayers, (a, b) => string.Compare(a.name, b.name));

    // 2. Asignar roles
    for (int i = 0; i < allPlayers.Length; i++) {
        if (i == index) {
            localPlayer = allPlayers[i].gameObject;
            allPlayers[i].enabled = true;
            
            // Assignar ID local
            allPlayers[i].playerId = localPlayerId;
            
            Debug.Log($"[GameManager] ✅ ERES LOCAL: {localPlayer.name} (ID={localPlayerId})");
        } else {
            remotePlayer = allPlayers[i].gameObject;
            allPlayers[i].enabled = false;
            
            // Assignar ID remot (el que sigui que no sigui el local)
            // En un futur això podria venir d'una llista de jugadors
            allPlayers[i].playerId = "rival_id"; 
            
            RemotePlayerController rpc = remotePlayer.GetComponent<RemotePlayerController>();
            if (rpc == null) rpc = remotePlayer.AddComponent<RemotePlayerController>();
            
            // Vincular sprites para que el remoto también se anime
            MovementController mc = allPlayers[i];
            rpc.spriteRendererUp = mc.spriteRendererUp;
            rpc.spriteRendererDown = mc.spriteRendererDown;
            rpc.spriteRendererLeft = mc.spriteRendererLeft;
            rpc.spriteRendererRight = mc.spriteRendererRight;

            rpc.Initialize("");
            Debug.Log($"[GameManager] 👤 EL OTRO ES REMOTE: {remotePlayer.name} (ID=rival_id)");
        }
    }

    // 3. Registrar WebSocket
    var wsManager = WebSocketManager.GetOrCreate();
    if (wsManager != null) wsManager.OnMessageReceived += HandleWebSocketMessage;
}

private void RegisterWebSocketListener()
{
    if (WebSocketManager.Instance != null)
    {
        WebSocketManager.Instance.OnMessageReceived += HandleWebSocketMessage;
        Debug.Log("[GameManager] ✅ WebSocket listener registrado");
    }
    else
    {
        Debug.LogError("[GameManager] ❌ WebSocketManager.Instance sigue siendo null");
    }
}
    private void HandleWebSocketMessage(string json)
    {
        // Log crudo para inspección manual
        Debug.Log("[WS-Raw] " + json);
        
        WsMessage msg = JsonUtility.FromJson<WsMessage>(json);
        if (msg == null || string.IsNullOrEmpty(msg.type)) return;

        // Si el mensaje es nuestro, lo ignoramos 
        if (!string.IsNullOrEmpty(localPlayerId) && msg.playerId == localPlayerId) return;
        
        if (msg.type == "player-moved")
        {
            MoveRemotePlayer(msg.x, msg.y);
        }
        
        if (msg.type == "bomb-placed")
        {
            CreateRemoteBomb(msg.x, msg.y);
        }
    }

    private void CreateRemoteBomb(float x, float y)
    {
        try {
            Debug.Log($"[GameManager] Bomba enemiga en: ({x}, {y})");
            if (bombPrefab != null) {
                GameObject bombObj = Instantiate(bombPrefab, new Vector3(x, y, 0), Quaternion.identity);
                
                // Configurar con los valores del BombController local para que sea idéntico
                BombController localBC = localPlayer?.GetComponent<BombController>();
                if (localBC != null) {
                    Bomb b = bombObj.GetComponent<Bomb>();
                    if (b == null) b = bombObj.AddComponent<Bomb>();
                    
                    b.fuseTime = localBC.bombFuseTime;
                    b.explosionPrefab = localBC.explosionPrefab;
                    b.explosionLayerMask = localBC.explosionLayerMask;
                    b.explosionDuration = localBC.explosionDuration;
                    b.explosionRadius = localBC.explosionRadius;
                    b.destructibleTiles = localBC.destructibleTiles;
                    b.destructiblePrefab = localBC.destructiblePrefab;
                }
            }
        }
        catch (System.Exception e) {
            Debug.LogError("[GameManager] Error: " + e.Message);
        }
    }
    public GameObject GetBombPrefab()
{
    return bombPrefab;
}
    private void MoveRemotePlayer(float x, float y)
    {
        try
        {
            if (remotePlayer != null)
            {
                RemotePlayerController controller = remotePlayer.GetComponent<RemotePlayerController>();
                if (controller != null)
                {
                    controller.MoveToPosition(new Vector2(x, y));
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("[GameManager] Error moviendo rival: " + e.Message);
        }
    }
    
    public void CheckWinState()
    {
        int aliveCount = 0;
        foreach (GameObject player in players)
        {
            if (player.activeSelf)
            {
                aliveCount++;
            }
        }
        if (aliveCount <= 1)
        {
            Invoke(nameof(NewRound), 3f);
        }
    }
    
    private void NewRound()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    private void OnDestroy()
    {
        if (WebSocketManager.Instance != null)
        {
            WebSocketManager.Instance.OnMessageReceived -= HandleWebSocketMessage;
        }
    }

   // Busca esto en tu GameManager y déjalo exactamente así:
public void OnPlayerDeath(string playerId) 
{
    if (gameEnded) return; // Evitar llamadas múltiples
    
    gameEnded = true;
    Debug.Log("[GameManager] El jugador " + playerId + " ha muerto.");
    
    // Determinar quién ganó
    bool localPlayerDied = (localPlayerId == playerId);
    bool localWinner = !localPlayerDied;
    
    string winnerName = localWinner ? localPlayerName : remotePlayerName;
    
    Debug.Log($"[GameManager] 🎉 GANADOR: {winnerName} (Local Winner: {localWinner})");
    
    // Guardar los datos en GameOverData
    GameOverData.WinnerName = winnerName;
    GameOverData.IsLocalWinner = localWinner;
    
    // Cargar la escena de Game Over
    SceneManager.LoadScene("GameOverScene");
}

private System.Collections.IEnumerator WaitAndLoad()
{
    yield return new WaitForSeconds(2f);
    UnityEngine.SceneManagement.SceneManager.LoadScene("GameOverScene");
}
}