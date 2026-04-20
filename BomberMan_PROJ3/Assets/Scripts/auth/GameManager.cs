using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public string sessionId { get; private set; } = System.Guid.NewGuid().ToString();

    public GameObject[] players;

    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private GameObject remotePlayerPrefab;

    public GameObject localPlayer;
    public GameObject remotePlayer;
    private string localPlayerId;
    private Vector2 lastRemotePosition;

    private string localPlayerName;
    private string remotePlayerName;
    private bool gameEnded = false;

    [Header("Training Mode")]
    public bool isTraining = false;

    [Header("Stage Reset")]
    public Tilemap destructibleTiles;
    public TileBase blockTile;
    private List<Vector3Int> initialBlockPositions = new List<Vector3Int>();

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
        localPlayerId = AuthManager.Instance?.JugadorActual?.id;
        if (string.IsNullOrEmpty(localPlayerId)) localPlayerId = "local_player_id";

        localPlayerName = AuthManager.Instance?.JugadorActual?.username ?? "Jugador Local";
        remotePlayerName = "Jugador Remoto";

        if (destructibleTiles != null) SaveInitialLayout();

        if (isTraining)
        {
            // ── MODO ENTRENAMIENTO ──────────────────────────────────────────
            
            foreach (var p in players)
            {
                if (p == null) continue;
                var mc = p.GetComponent<MovementController>();
                if (mc == null) continue;

                if (mc.isLocalPlayer)
                {
                    mc.enabled = true;
                    localPlayer = p;
                    Debug.Log($"[GameManager] Training: Local Player = {p.name}");
                }
                
            }
            
            return;
        }

        // ── MODO MULTIJUGADOR ───────────────────────────────────────────────
        int myIndex = AuthManager.Instance != null ? AuthManager.Instance.PlayerIndex : 0;
        Debug.Log($"[GameManager] My Player Index: {myIndex}, Local ID: {localPlayerId}");

    
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null) continue;
            
            MovementController mc = players[i].GetComponent<MovementController>();
            RemotePlayerController rpc = players[i].GetComponent<RemotePlayerController>();
            Rigidbody2D rb = players[i].GetComponent<Rigidbody2D>();

            if (mc == null) continue;

            if (i == myIndex)
            {
                mc.isLocalPlayer = true;
                mc.playerId = localPlayerId;
                mc.enabled = true;
                if (rpc != null) rpc.enabled = false;
                if (rb != null) rb.bodyType = RigidbodyType2D.Dynamic;

                localPlayer = players[i];
                Debug.Log($"[GameManager] Asignado Local Player a {players[i].name}");
            }
            else
            {
                mc.isLocalPlayer = false;
                mc.enabled = false; 
                if (rpc != null) rpc.enabled = true;
                if (rb != null) rb.bodyType = RigidbodyType2D.Kinematic;

                remotePlayer = players[i];
                lastRemotePosition = players[i].transform.position;
                Debug.Log($"[GameManager] Asignado Remote Player a {players[i].name}");
            }
        }

        var wsManager = WebSocketManager.GetOrCreate();
        if (wsManager != null) wsManager.OnMessageReceived += HandleWebSocketMessage;
    }

    private void SaveInitialLayout()
    {
        initialBlockPositions.Clear();
        BoundsInt bounds = destructibleTiles.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (destructibleTiles.HasTile(pos)) initialBlockPositions.Add(pos);
        }
    }

    public void ResetStage()
    {
        if (destructibleTiles == null || !isTraining) return;

        destructibleTiles.ClearAllTiles();
        foreach (Vector3Int pos in initialBlockPositions)
            destructibleTiles.SetTile(pos, blockTile);

        GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");
        foreach (var b in bombs) Destroy(b);

        if (localPlayer != null) localPlayer.SetActive(true);
    }

    private void HandleWebSocketMessage(string json)
    {
        var data = JsonUtility.FromJson<WsMessage>(json);
        if (data == null || string.IsNullOrEmpty(data.type)) return;

        if (!string.IsNullOrEmpty(data.sessionId) && data.sessionId == sessionId) {
            Debug.Log("[GameManager] Ignorando (eco de sessionId)");
            return;
        }

        if ((data.type == "player-move" || data.type == "player-moved") && data.playerId != localPlayerId)
        {
            Debug.Log($"[GameManager] Moviendo REMOTE a {data.x}, {data.y}");
            MoveRemotePlayer(data.x, data.y);
        }
        else if ((data.type == "place-bomb" || data.type == "bomb-placed") && data.playerId != localPlayerId)
        {
            Debug.Log($"[GameManager] BOMBA REMOTE plantada en {data.x}, {data.y}");
            CreateRemoteBomb(data.x, data.y);
        }
        else if (data.type == "player-death")
            OnRemotePlayerDeath();
    }

    private void MoveRemotePlayer(float x, float y)
    {
        if (remotePlayer == null) return;

        Vector2 newPos = new Vector2(x, y);
        Vector2 dir = (newPos - lastRemotePosition).normalized;

        var mc = remotePlayer.GetComponent<MovementController>();
        if (mc != null) mc.ApplyRemoteMovement(dir);

        var rpc = remotePlayer.GetComponent<RemotePlayerController>();
        if (rpc != null)
        {
            rpc.MoveToPosition(newPos);
        }
        else
        {
            var rb = remotePlayer.GetComponent<Rigidbody2D>();
            if (rb != null) rb.MovePosition(newPos);
        }

        lastRemotePosition = newPos;
    }

    public void OnRemotePlayerDeath()
    {
        if (remotePlayer == null) return;
        var rpc = remotePlayer.GetComponent<RemotePlayerController>();
        if (rpc != null) rpc.PlayDeathAnimation();
    }

    public void OnPlayerDeath(string playerId)
    {
        if (gameEnded || isTraining) return;
        gameEnded = true;

        bool localPlayerDied = (localPlayerId == playerId);
        string winnerName = localPlayerDied ? remotePlayerName : localPlayerName;
        
        // ── REPORTAR ESTADÍSTIQUES (NOMÉS MULTIJUGADOR) ──
        if (StatsService.Instance != null)
        {
            List<PlayerResultData> results = new List<PlayerResultData>();
            
            string winnerId = localPlayerDied ? "remote_opponent" : localPlayerId; 
           
            if (!localPlayerDied)
            {
                results.Add(new PlayerResultData { playerId = localPlayerId, score = 100 });
                results.Add(new PlayerResultData { playerId = playerId, score = 0 });
                StatsService.Instance.ReportGameResult(sessionId, localPlayerId, results);
            }
            else
            {
                results.Add(new PlayerResultData { playerId = localPlayerId, score = 0 });
                StatsService.Instance.ReportGameResult(sessionId, "remote_winner", results);
            }
        }

        GameOverData.WinnerName = winnerName;
        GameOverData.IsLocalWinner = !localPlayerDied;

        SceneManager.LoadScene("GameOverScene");
    }

    private void CreateRemoteBomb(float x, float y)
    {
        if (bombPrefab != null) {
            GameObject bombObj = Instantiate(bombPrefab, new Vector3(x, y, 0), Quaternion.identity);
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

    [System.Serializable]
    public class WsMessage
    {
        public string type;
        public string playerId;
        public string sessionId;
        public float x;
        public float y;
    }
}
