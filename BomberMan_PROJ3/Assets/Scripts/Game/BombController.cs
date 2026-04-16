using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BombController : MonoBehaviour
{
    [Header("Bomb")]
    public GameObject bombPrefab;
    public KeyCode inputKey = KeyCode.Space;
    public float bombFuseTime = 3f;
    public int bombAmount = 1;
    private int bombsRemaining;

    [Header("Explosion")]
    public Explosion explosionPrefab;
    public LayerMask explosionLayerMask;
    public float explosionDuration = 1f;
    public int explosionRadius = 1;

    [Header("Destructible")]
    public Tilemap destructibleTiles;
    public Destructible  destructiblePrefab;

    private void OnEnable(){
        bombsRemaining = bombAmount;
    }

    private void Update()
    {
        if(bombsRemaining > 0 && Input.GetKeyDown(inputKey)){
           PlaceBomb();
        }
    }

    public void PlaceBomb()
    {
        if (bombsRemaining > 0) {
            StartCoroutine(PlaceBombRoutine());
        }
    }

    public void PlaceBomb(Vector2 position)
    {
        if (bombsRemaining > 0) {
            StartCoroutine(PlaceBombRoutine(position));
        }
    }

    private IEnumerator PlaceBombRoutine(Vector2? forcedPosition = null)
    {
         Vector2 position = forcedPosition ?? (Vector2)transform.position;
         position.x = Mathf.Round(position.x);
         position.y = Mathf.Round(position.y);

         // Instanciar localmente
         GameObject bombObj = Instantiate(bombPrefab, position, Quaternion.identity);
         
         // Configurar script Bomb dinámicamente si no está en el prefab
         Bomb bombScript = bombObj.GetComponent<Bomb>();
         if (bombScript == null) bombScript = bombObj.AddComponent<Bomb>();
         
         bombScript.fuseTime = bombFuseTime;
         bombScript.explosionPrefab = explosionPrefab;
         bombScript.explosionLayerMask = explosionLayerMask;
         bombScript.explosionDuration = explosionDuration;
         bombScript.explosionRadius = explosionRadius;
         bombScript.destructibleTiles = destructibleTiles;
         bombScript.destructiblePrefab = destructiblePrefab;

         bombsRemaining--;

         // Notificar al servidor
         if (WebSocketManager.Instance != null) {
             PositionalMessage msg = new PositionalMessage { x = position.x, y = position.y };
             WebSocketManager.Instance.SendMessage("place-bomb", JsonUtility.ToJson(msg));
         }

         // Recuperar la bomba después del tiempo de espera
         yield return new WaitForSeconds(bombFuseTime);
         bombsRemaining++;
    }

    private void Explode(Vector2 position, Vector2 direction, int length)
    {
        if(length <= 0){
            return;
        }

        position += direction;

        if (Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, explosionLayerMask ))
        {
            ClearDestructible(position);
          return;   
        }

        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity );
         explosion.SetActiveRenderer(length > 1 ? explosion.middle : explosion.end);
         explosion.SetDirection(direction);
         explosion.DestroyAfter(explosionDuration);
         Destroy(explosion.gameObject, explosionDuration);

         Explode(position,direction, length - 1);

    }

    private void ClearDestructible(Vector2 position){
      Vector3Int cell = destructibleTiles.WorldToCell(position);
      TileBase tile = destructibleTiles.GetTile(cell);

      if(tile != null)
      {
        Instantiate(destructiblePrefab, position, Quaternion.identity);
        destructibleTiles.SetTile(cell, null);
      }
    }

    public void AddBomb()
    {
        bombAmount++;
        bombsRemaining++;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.gameObject.layer == LayerMask.NameToLayer("Bomb")){
            other.isTrigger = false;
        }
    }

    
}
