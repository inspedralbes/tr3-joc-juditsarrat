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

    [Header("Recompenses")]
    [SerializeField] private float rewardTarget = 2f; // Pugem la recompensa per guanyar
    [SerializeField] private float rewardDestroyBlock = 0.2f;
    [SerializeField] private float penaltyStep = -0.0005f; // Penalització per pas més suau
    [SerializeField] private float penaltyBombSpam = -0.05f; // Penalització per posar bomba sense pensar
    [SerializeField] private float penaltyDeath = -1.5f; // Gran penalització per morir

    private Rigidbody2D rb;
    private BombController bombController;
    private Vector2 initialPosition;

    private float lastResetTime = 0f;
    private float resetCooldown = 0.5f;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody2D>();
        bombController = GetComponent<BombController>();
        initialPosition = transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        Debug.Log("[SimpleAgent] 🔄 Reiniciant episodi...");
        lastResetTime = Time.time;
        
        this.enabled = true;
        
        if (rb != null) {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.bodyType = RigidbodyType2D.Dynamic;
        }

        transform.localPosition = initialPosition;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 1. Posició de l'agent (2)
        sensor.AddObservation(transform.localPosition.x / arenaLimit);
        sensor.AddObservation(transform.localPosition.y / arenaLimit);

        // 2. Posició del target (2)
        if (target != null) {
            sensor.AddObservation((target.localPosition.x - transform.localPosition.x) / arenaLimit);
            sensor.AddObservation((target.localPosition.y - transform.localPosition.y) / arenaLimit);
        } else {
            sensor.AddObservation(0f);
            sensor.AddObservation(0f);
        }

        // 3. Detecció d'obstacles i BOMBRES (8)
        // Ara mirem també si hi ha bombes per no morir
        sensor.AddObservation(IsObstacle(Vector2.up) ? 1f : 0f);
        sensor.AddObservation(IsObstacle(Vector2.down) ? 1f : 0f);
        sensor.AddObservation(IsObstacle(Vector2.left) ? 1f : 0f);
        sensor.AddObservation(IsObstacle(Vector2.right) ? 1f : 0f);
        
        sensor.AddObservation(IsDanger(Vector2.up) ? 1f : 0f);
        sensor.AddObservation(IsDanger(Vector2.down) ? 1f : 0f);
        sensor.AddObservation(IsDanger(Vector2.left) ? 1f : 0f);
        sensor.AddObservation(IsDanger(Vector2.right) ? 1f : 0f);
    }

    [Header("Detecció")]
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private LayerMask dangerLayerMask; // Per a bombes i explosions

    private bool IsObstacle(Vector2 direction)
    {
        float rayDistance = cellSize * 0.8f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayDistance, obstacleLayerMask);
        return hit.collider != null;
    }

    private bool IsDanger(Vector2 direction)
    {
        float rayDistance = cellSize * 2f; // Mirem més lluny el perill
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, rayDistance, dangerLayerMask);
        return hit.collider != null;
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
            case 5: 
                if (bombController != null) {
                    bombController.PlaceBomb();
                    AddReward(penaltyBombSpam); // Penalitzem posar bombes per evitar spam innecessari
                }
                break;
        }

        rb.linearVelocity = dir * moveSpeed;
        
        // Recompensa per apropar-se al target
        if (target != null) {
            float distance = Vector2.Distance(transform.localPosition, target.localPosition);
            AddReward(0.01f / (distance + 1f));
        }

        AddReward(penaltyStep);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var a = actionsOut.DiscreteActions;
        a[0] = 0;
        if (Input.GetKey(KeyCode.UpArrow)) a[0] = 1;
        else if (Input.GetKey(KeyCode.DownArrow)) a[0] = 2;
        else if (Input.GetKey(KeyCode.LeftArrow)) a[0] = 3;
        else if (Input.GetKey(KeyCode.RightArrow)) a[0] = 4;
        else if (Input.GetKey(KeyCode.Space)) a[0] = 5;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (Time.time - lastResetTime < resetCooldown) return;

        if (collision.transform.CompareTag("Target"))
        {
            Debug.Log("[SimpleAgent] 🎯 Reset: Target detectat!");
            AddReward(rewardTarget);
            EndEpisode();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time - lastResetTime < resetCooldown) return;

        // Si la explosión toca al agente, éste "muere"
        // Asegúrate de que tus explosiones estén en una Layer llamada "Explosion"
        if (other.gameObject.layer == LayerMask.NameToLayer("Explosion"))
        {
            Debug.Log("[SimpleAgent] 💥 Reset: Explosió detectada!");
            AddReward(-1f); // Gran penalización por morir
            EndEpisode();  // Reiniciamos el episodio
        }
    }

    // Crida aquesta funció des del script que gestiona la destrucció de blocs per donar recompensa
    public void OnBlockDestroyed()
    {
        AddReward(rewardDestroyBlock);
    }
}
