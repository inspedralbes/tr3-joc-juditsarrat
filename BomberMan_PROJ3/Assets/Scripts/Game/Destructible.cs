using UnityEngine;

public class Destructible : MonoBehaviour
{
    public float destructionTime = 1f;
    
    [Range(0f, 1f)]
    public float itemSpawnChance = 0.2f;
    public GameObject[] spawnableItems;

    private void Start()
    {
        Destroy(gameObject, destructionTime);
    }

    private void OnDestroy()
    {
        if (spawnableItems.Length > 0 && Application.isPlaying)
        {
            Vector2 pos = transform.position;
            int seed = Mathf.RoundToInt(pos.x * 1000f) + Mathf.RoundToInt(pos.y * 100f);
            System.Random rng = new System.Random(seed);

            if (rng.NextDouble() < itemSpawnChance)
            {
                int randomIndex = rng.Next(0, spawnableItems.Length);
                Instantiate(spawnableItems[randomIndex], transform.position, Quaternion.identity);
            }
        }
    }
}
