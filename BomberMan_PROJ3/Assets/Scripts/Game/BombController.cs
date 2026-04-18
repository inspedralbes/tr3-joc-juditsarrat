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
         
         // Configurar script Bomb dinámicamente preservando los del prefab si los nuestros están vacíos
         Bomb bombScript = bombObj.GetComponent<Bomb>();
         if (bombScript == null) bombScript = bombObj.AddComponent<Bomb>();
         
         if (bombFuseTime > 0) bombScript.fuseTime = bombFuseTime;
         if (explosionPrefab != null) bombScript.explosionPrefab = explosionPrefab;
         if (explosionLayerMask != 0) bombScript.explosionLayerMask = explosionLayerMask;
         if (explosionDuration > 0) bombScript.explosionDuration = explosionDuration;
         if (explosionRadius > 0) bombScript.explosionRadius = explosionRadius;
         if (destructibleTiles != null) bombScript.destructibleTiles = destructibleTiles;
         if (destructiblePrefab != null) bombScript.destructiblePrefab = destructiblePrefab;

         bombsRemaining--;

// Solo intentará enviar si el objeto existe Y no es nulo internamente
if (WebSocketManager.Instance != null && WebSocketManager.Instance.gameObject.activeInHierarchy) {
    try {
        string xb = position.x.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string yb = position.y.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string sId = GameManager.Instance != null ? GameManager.Instance.sessionId : "";
        string playerCode = GameManager.Instance != null && GameManager.Instance.localPlayer != null 
                             ? GameManager.Instance.localPlayer.GetComponent<MovementController>().playerId : "";
        
        string json = "{\"type\":\"place-bomb\",\"playerId\":\"" + playerCode + "\",\"sessionId\":\"" + sId + "\",\"x\":" + xb + ",\"y\":" + yb + "}";
        WebSocketManager.Instance.SendRaw(json);
    } catch {
        // Ignoramos el error durante el entrenamiento para que no se detenga el agente
    }
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
