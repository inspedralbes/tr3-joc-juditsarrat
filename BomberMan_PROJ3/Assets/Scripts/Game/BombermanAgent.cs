using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

/// <summary>
/// Agent ML-Agents per a Bomberman 2D - ADAPTAT AL TEU PROJECTE
/// 
/// Aquesta versió és compatible amb:
/// - GameManager (jugador local vs remot)
/// - MovementController (control de moviment)
/// - WebSocket (enviament de posicions i bombes)
/// - Tilemap (detecció d'obstacles)
/// 
/// ASSUMPCIÓ: Aquest script es col·loca al GameObject del jugador LOCAL.
/// </summary>
public class BombermanAgent : Agent
{
    [Header("References")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private MovementController movementController;
    private BombController bombController;
    
    [Header("Configuració del tauler")]
    [SerializeField] private int gridWidth = 13;
    [SerializeField] private int gridHeight = 13;
    [SerializeField] private float cellSize = 1f;
    
    [Header("Configuració d'entrenament")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float penaltyPerStep = 0.001f;
    [SerializeField] private float rewardEnemyKill = 0.5f;
    [SerializeField] private float rewardLevelComplete = 2f;
    [SerializeField] private float penaltyDeath = 1f;
    
    // Estat de l'agent
    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.zero;
    private Vector2 currentGridPosition;
    
    private int enemiesKilledThisEpisode = 0;
    private int totalEnemiesInLevel = 0;
    private bool isAlive = true;
    private float episodeStartTime;

    public override void Initialize()
    {
        // Cercar references
        if (gameManager == null)
            gameManager = GameManager.Instance;
        
        if (movementController == null)
            movementController = GetComponent<MovementController>();
        
        bombController = GetComponent<BombController>();
        rb = GetComponent<Rigidbody2D>();
        
        if (movementController == null)
            Debug.LogError("[BombermanAgent] MovementController no trobar!");
        
        // Inicialitzar posició de graella
        UpdateGridPosition();
        
        // Registrar callbacks per a rewarding
        SetupCallbacks();
    }

    private void SetupCallbacks()
    {
        // AQUÍ: registra callbacks amb els teus sistemes
        // Per exemple, quan es mata un enemic, crida OnEnemyDefeated()
        // Depèn de com tinguis implementat el sistema de detecció d'enemics
    }

    public override void OnEpisodeBegin()
    {
        isAlive = true;
        enemiesKilledThisEpisode = 0;
        episodeStartTime = Time.time;
        moveDirection = Vector2.zero;
        
        // Reset de la posició inicial (esquina superior esquerra)
        transform.position = new Vector3(1 * cellSize, 1 * cellSize, 0);
        
        if (rb != null)
            rb.linearVelocity = Vector2.zero;
        
        // Aquí crideuries al GameManager per resetejar l'escena
        // gameManager?.ResetGame(); // Si tens aquesta funció
        
        UpdateGridPosition();
        Debug.Log("[BombermanAgent] 🎮 Episodi nova: Begin!");
    }

    /// <summary>
    /// OBSERVACIONS: informació que veu l'agent
    /// Total: ~14 observacions
    /// </summary>
    public override void CollectObservations(VectorSensor sensor)
    {
        UpdateGridPosition();
        
        // 1. Posició normalitzada (2)
        sensor.AddObservation(currentGridPosition.x / gridWidth);
        sensor.AddObservation(currentGridPosition.y / gridHeight);
        
        // 2. Velocitat actual (2)
        Vector2 vel = rb != null ? rb.linearVelocity : Vector2.zero;
        sensor.AddObservation(vel.x / moveSpeed);
        sensor.AddObservation(vel.y / moveSpeed);
        
        // 3. Detecció de cèl·les blocades (4)
        sensor.AddObservation(IsBlockedInDirection(Vector2.up) ? 1f : 0f);
        sensor.AddObservation(IsBlockedInDirection(Vector2.down) ? 1f : 0f);
        sensor.AddObservation(IsBlockedInDirection(Vector2.left) ? 1f : 0f);
        sensor.AddObservation(IsBlockedInDirection(Vector2.right) ? 1f : 0f);
        
        // 4. Distancia al jugador remot (enemic) (1)
        float distToRemote = GetDistanceToRemotePlayer();
        sensor.AddObservation(distToRemote / (gridWidth + gridHeight));
        
        // 5. Ratio d'enemics morts (1)
        float killRatio = totalEnemiesInLevel > 0 ? enemiesKilledThisEpisode / (float)totalEnemiesInLevel : 0f;
        sensor.AddObservation(killRatio);
        
        // 6. Estat viu/mort (1)
        sensor.AddObservation(isAlive ? 1f : 0f);
        
        // 7. Temps relatiu de l'episodi (1) - opcional
        float episodeProgress = Mathf.Min((Time.time - episodeStartTime) / 30f, 1f);
        sensor.AddObservation(episodeProgress);
        
        // TOTAL: 2 + 2 + 4 + 1 + 1 + 1 + 1 = 12 observacions
    }

    /// <summary>
    /// ACCIONS: què pot fer l'agent
    /// Action space: 1 branca DISCRETA de mida 6
    /// 
    /// 0 = no fer res
    /// 1 = moure amunt
    /// 2 = moure avall
    /// 3 = moure esquerra
    /// 4 = moure dreta
    /// 5 = plantar bomba
    /// </summary>
    public override void OnActionReceived(ActionBuffers actions)
    {
        if (!isAlive)
            return;
        
        int action = actions.DiscreteActions[0];
        moveDirection = Vector2.zero;
        bool shouldPlantBomb = false;

        switch (action)
        {
            case 1: // Amunt
                moveDirection = Vector2.up;
                break;
            case 2: // Avall
                moveDirection = Vector2.down;
                break;
            case 3: // Esquerra
                moveDirection = Vector2.left;
                break;
            case 4: // Dreta
                moveDirection = Vector2.right;
                break;
            case 5: // Bomba
                shouldPlantBomb = true;
                break;
        }

        // Aplicar moviment via MovementController
        if (movementController != null && moveDirection != Vector2.zero)
        {
            // Assegurar que el MovementController recorri la direcció
            movementController.SetMovementDirection(moveDirection);
        }
        else if (rb != null)
        {
            // Fallback si no tens MovementController
            rb.linearVelocity = moveDirection * moveSpeed;
        }

        // Plantar bomba
        if (shouldPlantBomb && bombController != null)
        {
            bombController.PlaceBomb(transform.position);
        }

        // PENALITZACIÓ per cada pas (motiva que es doni pressa)
        AddReward(-penaltyPerStep);
    }

    /// <summary>
    /// HEURISTIC: control manual per provar
    /// </summary>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var action = actionsOut.DiscreteActions;
        action[0] = 0;

        if (Input.GetKey(KeyCode.UpArrow)) action[0] = 1;
        else if (Input.GetKey(KeyCode.DownArrow)) action[0] = 2;
        else if (Input.GetKey(KeyCode.LeftArrow)) action[0] = 3;
        else if (Input.GetKey(KeyCode.RightArrow)) action[0] = 4;
        else if (Input.GetKey(KeyCode.Space)) action[0] = 5;
    }

    /// <summary>
    /// Callbacks per a rewarding - crida'ls des del teu sistema de joc
    /// </summary>

    public void OnEnemyDefeated()
    {
        enemiesKilledThisEpisode++;
        AddReward(rewardEnemyKill);
        Debug.Log($"[Agent] 💥 Enemic mort! Total: {enemiesKilledThisEpisode}/{totalEnemiesInLevel}");
    }

    public void OnLevelCompleted()
    {
        if (isAlive)
        {
            AddReward(rewardLevelComplete);
            Debug.Log("[Agent] 🎉 NIVELL COMPLETAT!");
            EndEpisode();
        }
    }

    public void OnAgentDeath()
    {
        if (!isAlive) return; // Evitar doble penalització
        
        isAlive = false;
        AddReward(-penaltyDeath);
        Debug.Log("[Agent] ☠️ L'AGENT HA MORT");
        EndEpisode();
    }

    public void SetTotalEnemies(int count)
    {
        totalEnemiesInLevel = count;
    }

    /// <summary>
    /// DETECCIÓ D'OBSTACLES
    /// Adaptat al teu sistema de Tilemap
    /// </summary>

    private bool IsBlockedInDirection(Vector2 direction)
    {
        // Usar Raycasts per detectar obstacles
        Vector3 rayOrigin = transform.position;
        float rayDistance = cellSize * 0.9f;

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, rayDistance);
        
        // Considerar bloquejat si toca una paret o un obstacle destructible
        if (hit.collider != null)
        {
            // ADAPTA AQUESTS TAGS AL TEU JOCS
            return hit.collider.CompareTag("Wall") || 
                   hit.collider.CompareTag("Obstacle") || 
                   hit.collider.CompareTag("DestructibleTile");
        }
        
        return false;
    }

    private float GetDistanceToRemotePlayer()
    {
        // Obtenir la posició del jugador remot (l'enemic IA)
        if (gameManager != null && gameManager.remotePlayer != null)
        {
            float dist = Vector2.Distance(transform.position, gameManager.remotePlayer.transform.position);
            return dist;
        }
        
        // Si no hi ha jugador remot, retorna distancia màxima
        return gridWidth + gridHeight;
    }

    private void UpdateGridPosition()
    {
        // Convertir posició mundial a coordenades de graella
        currentGridPosition.x = Mathf.Round(transform.position.x / cellSize);
        currentGridPosition.y = Mathf.Round(transform.position.y / cellSize);
    }

    /// <summary>
    /// Integració amb WebSocket per enviar moviments
    /// (Opcional - depèn de com ho tinguis implementat)
    /// </summary>
    private void SendMovementToRemote()
    {
        // Si tens WebSocketManager, pots enviar l'acció aquí
        // var wsManager = WebSocketManager.GetOrCreate();
        // if (wsManager != null)
        // {
        //     var msg = new { type = "player-moved", x = transform.position.x, y = transform.position.y };
        //     wsManager.SendMessage(JsonUtility.ToJson(msg));
        // }
    }

    // Getters útils
    public bool IsAgentAlive() => isAlive;
    public int GetEnemiesKilled() => enemiesKilledThisEpisode;
    public Vector2 GetGridPosition() => currentGridPosition;
}