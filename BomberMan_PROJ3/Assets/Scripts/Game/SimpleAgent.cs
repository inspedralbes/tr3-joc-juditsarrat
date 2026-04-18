using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class SimpleAgent : Agent
{
    [Header("Configuració")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float arenaLimit = 4f;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Transform target;

    [Header("Recompenses (v5)")]
    [SerializeField] private float rewardTarget = 5f;       // Subido de 2 a 5
    [SerializeField] private float rewardDestroyBlock = 0.4f; // Subido de 0.2 a 0.4
    [SerializeField] private float penaltyStep = -0.002f;   // Más presión de tiempo
    [SerializeField] private float penaltyDeath = -1f;      // Castigo menos terrorífico

    private Rigidbody2D rb;
    private BombController bombController;
    private Vector2 initialPosition;

    [Header("Detecció")]
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private LayerMask destructibleLayerMask;
    [SerializeField] private LayerMask dangerLayerMask;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        bombController = GetComponent<BombController>();
        initialPosition = transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        if (rb != null) {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        transform.localPosition = initialPosition;

        // RESETEAR EL ESCENARIO COMPLETO
        if (GameManager.Instance != null) {
            GameManager.Instance.ResetStage();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x / arenaLimit);
        sensor.AddObservation(transform.localPosition.y / arenaLimit);

        if (target != null && target.gameObject.activeInHierarchy) {
            sensor.AddObservation((target.localPosition.x - transform.localPosition.x) / arenaLimit);
            sensor.AddObservation((target.localPosition.y - transform.localPosition.y) / arenaLimit);
        } else {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }

        sensor.AddObservation(IsLayerNearby(Vector2.up, obstacleLayerMask) ? 1f : 0f);
        sensor.AddObservation(IsLayerNearby(Vector2.down, obstacleLayerMask) ? 1f : 0f);
        sensor.AddObservation(IsLayerNearby(Vector2.left, obstacleLayerMask) ? 1f : 0f);
        sensor.AddObservation(IsLayerNearby(Vector2.right, obstacleLayerMask) ? 1f : 0f);

        sensor.AddObservation(IsLayerNearby(Vector2.up, destructibleLayerMask) ? 1f : 0f);
        sensor.AddObservation(IsLayerNearby(Vector2.down, destructibleLayerMask) ? 1f : 0f);
        sensor.AddObservation(IsLayerNearby(Vector2.left, destructibleLayerMask) ? 1f : 0f);
        sensor.AddObservation(IsLayerNearby(Vector2.right, destructibleLayerMask) ? 1f : 0f);
        
        sensor.AddObservation(IsDanger(Vector2.zero) ? 1f : 0f);

        Bomb[] bombs = Object.FindObjectsByType<Bomb>(FindObjectsSortMode.None);
        if (bombs.Length > 0) {
            Bomb nearest = bombs[0];
            float minD = Vector2.Distance(transform.position, nearest.transform.position);
            foreach (var b in bombs) {
                float d = Vector2.Distance(transform.position, b.transform.position);
                if (d < minD) { minD = d; nearest = b; }
            }
            sensor.AddObservation((nearest.transform.position.x - transform.position.x) / arenaLimit);
            sensor.AddObservation((nearest.transform.position.y - transform.position.y) / arenaLimit);
        } else {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        Vector2 dir = Vector2.zero;

        switch (action)
        {
            case 1: dir = Vector2.up; break;
            case 2: dir = Vector2.down; break;
            case 3: dir = Vector2.left; break;
            case 4: dir = Vector2.right; break;
            case 5: if (bombController != null) bombController.PlaceBomb(); break;
        }

        rb.linearVelocity = dir * moveSpeed;
        AddReward(penaltyStep);
    }

    private bool IsLayerNearby(Vector2 direction, LayerMask mask) {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, cellSize * 0.8f, mask);
        return hit.collider != null;
    }

    private bool IsDanger(Vector2 direction) {
        Collider2D[] cols = Physics2D.OverlapCircleAll((Vector2)transform.position + (direction * cellSize), cellSize * 0.4f);
        foreach (var c in cols) if (c.GetComponent<Bomb>() != null || c.gameObject.layer == LayerMask.NameToLayer("Explosion")) return true;
        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.transform.CompareTag("Target")) {
            AddReward(rewardTarget); 
            EndEpisode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Explosion") || other.gameObject.layer == LayerMask.NameToLayer("Explosion"))
    {
        // 1. Si estamos entrenando (comando docker), reiniciamos rápido
        if (StepCount > 0 && GameManager.Instance.isTraining) 
        {
            AddReward(-1f);
            EndEpisode(); 
        }
        else 
        {
            // 2. Si estamos JUGANDO (isTraining desactivado), avisamos al GameManager
            // Usamos el ID que le diste a la IA en el Start: "rival_ia_id"
            GameManager.Instance.OnPlayerDeath("rival_ia_id");
        }
    }
}

    public void OnBlockDestroyed() { AddReward(rewardDestroyBlock); }
}