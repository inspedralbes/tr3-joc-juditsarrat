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
    
    private void Start()
{
    Debug.Log("[GameManager] Iniciando...");
    
    // Verificar AuthManager
    if (AuthManager.Instance != null && AuthManager.Instance.JugadorActual != null)
    {
        localPlayerId = AuthManager.Instance.JugadorActual.id;
        Debug.Log("[GameManager] Jugador local: " + localPlayerId);
    }
    else
    {
        Debug.LogWarning("[GameManager] AuthManager no disponible");
    }
    
    localPlayer = GameObject.FindWithTag("Player");
    
    // Escuchar WebSocket
    if (WebSocketManager.Instance != null)
    {
        WebSocketManager.Instance.OnMessageReceived += HandleWebSocketMessage;
        Debug.Log("[GameManager] WebSocket listener registrado");
    }
    else
    {
        Debug.LogError("[GameManager] WebSocketManager.Instance es null");
    }
}
   private void HandleWebSocketMessage(string message)
{
    Debug.Log("[GameManager] Mensaje: " + message);
    
    if (message.Contains("\"type\":\"player-moved\""))
    {
        ExtractAndMoveRemotePlayer(message);
    }
    
    if (message.Contains("\"type\":\"bomb-placed\""))
    {
        ExtractAndCreateBomb(message);
    }
}

private void ExtractAndCreateBomb(string json)
{
    try
    {
        int xStart = json.IndexOf("\"x\":") + 5;
        int xEnd = json.IndexOf(",", xStart);
        float x = float.Parse(json.Substring(xStart, xEnd - xStart));
        
        int yStart = json.IndexOf("\"y\":") + 5;
        int yEnd = json.IndexOf("}", yStart);
        float y = float.Parse(json.Substring(yStart, yEnd - yStart));
        
        Debug.Log("[GameManager] Bomba en: (" + x + ", " + y + ")");
        
        // Crear prefab de bomba
        Instantiate(bombPrefab, new Vector3(x, y, 0), Quaternion.identity);
    }
    catch (System.Exception e)
    {
        Debug.LogError("[GameManager] Error: " + e.Message);
    }
}
    public GameObject GetBombPrefab()
{
    return bombPrefab;
}
    private void ExtractAndMoveRemotePlayer(string json)
    {
        try
        {
            int xStart = json.IndexOf("\"x\":") + 5;
            int xEnd = json.IndexOf(",", xStart);
            float x = float.Parse(json.Substring(xStart, xEnd - xStart));
            
            int yStart = json.IndexOf("\"y\":") + 5;
            int yEnd = json.IndexOf("}", yStart);
            float y = float.Parse(json.Substring(yStart, yEnd - yStart));
            
            if (remotePlayer == null)
            {
                remotePlayer = Instantiate(remotePlayerPrefab, new Vector3(x, y, 0), Quaternion.identity);
                remotePlayer.name = "RemotePlayer";
            }
            else
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
            Debug.LogError("[GameManager] Error: " + e.Message);
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