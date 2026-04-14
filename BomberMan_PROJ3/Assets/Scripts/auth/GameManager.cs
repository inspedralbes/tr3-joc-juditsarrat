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
    }
    
private void Start()
{
    Debug.Log("[GameManager] Iniciando asignacción de roles...");
    
    localPlayerId = AuthManager.Instance?.JugadorActual?.id;
    int index = AuthManager.Instance != null ? AuthManager.Instance.PlayerIndex : 0;
    
    // 1. Encontrar todos los jugadores pre-colocados en la escena que tengan MovementController
    MovementController[] allPlayers = FindObjectsByType<MovementController>(FindObjectsSortMode.None);
    
    // Ordenar por nombre para consistencia (Player 1, Player 2) si están nombrados así
    System.Array.Sort(allPlayers, (a, b) => string.Compare(a.name, b.name));

    if (allPlayers.Length < 2) {
        Debug.LogWarning($"[GameManager] Se han encontrado {allPlayers.Length} jugadores en la escena. Se esperaban 2.");
    }

    // 2. Asignar roles local/remoto basándose en el PlayerIndex (0 o 1)
    for (int i = 0; i < allPlayers.Length; i++) {
        if (i == index) {
            // Este es nuestro jugador local (el que controlamos con el teclado)
            localPlayer = allPlayers[i].gameObject;
            allPlayers[i].enabled = true;
            
            // Aseguramos que el BombController local responda
            var bc = localPlayer.GetComponent<BombController>();
            if (bc != null) bc.enabled = true;

            // Deshabilitar control remoto en nuestro propio muñeco
            var rpc = localPlayer.GetComponent<RemotePlayerController>();
            if (rpc != null) rpc.enabled = false;

            Debug.Log($"[GameManager] ✅ Eres el Jugador {i + 1} ({localPlayer.name})");
        } else {
            // Este es el otro jugador (que se moverá por red)
            GameObject targetRemote = allPlayers[i].gameObject;
            
            // Deshabilitar el control por teclado local para este muñeco
            allPlayers[i].enabled = false;
            var bc = targetRemote.GetComponent<BombController>();
            if (bc != null) bc.enabled = false;

            // Habilitar o añadir el controlador remoto para que reciba los mensajes WS
            remotePlayer = targetRemote;
            RemotePlayerController rpc = remotePlayer.GetComponent<RemotePlayerController>();
            if (rpc == null) rpc = remotePlayer.AddComponent<RemotePlayerController>();
            
            rpc.enabled = true;
            rpc.Initialize(""); 
            
            Debug.Log($"[GameManager] 👤 El otro es el Jugador {i + 1} ({remotePlayer.name})");
        }
    }

    // 3. Registrar el listener de WebSocket para recibir los movimientos del rival
    var wsManager = WebSocketManager.GetOrCreate();
    if (wsManager != null)
    {
        wsManager.OnMessageReceived += HandleWebSocketMessage;
        Debug.Log("[GameManager] ✅ WebSocket listener registrado");
    }
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
        Debug.Log("[GameManager] Mensaje: " + json);
        WsMessage msg = JsonUtility.FromJson<WsMessage>(json);

        if (msg == null) return;

        // Si el mensaje es nuestro, lo ignoramos para no duplicar acciones
        if (msg.playerId == localPlayerId) return;
        
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
    try
    {
        Debug.Log("[GameManager] Bomba remota en: (" + x + ", " + y + ")");
        
        // Crear el mismo prefab de bomba que usa el local
        if (bombPrefab != null) {
            Instantiate(bombPrefab, new Vector3(x, y, 0), Quaternion.identity);
        }
    }
    catch (System.Exception e)
    {
        Debug.LogError("[GameManager] Error creando bomba remota: " + e.Message);
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
}