using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Bomb : MonoBehaviour
{
    public float fuseTime = 3f;
    public Explosion explosionPrefab;
    public LayerMask explosionLayerMask;
    public float explosionDuration = 1f;
    public int explosionRadius = 1;

    public Tilemap destructibleTiles;
    public Destructible destructiblePrefab;

    private void Start()
    {
        // Si no se han asignado por inspector (prefab), intentamos encontrarlos en la escena
        if (destructibleTiles == null) {
            GameObject grid = GameObject.Find("Grid"); // Común en proyectos de tutoriales
            if (grid != null) {
                // Buscar en los hijos del Grid (donde suelen estar los Tilemaps)
                Tilemap[] maps = grid.GetComponentsInChildren<Tilemap>();
                foreach (var map in maps) {
                    if (map.name.Contains("Destructible")) {
                        destructibleTiles = map;
                        break;
                    }
                }
            }
            if (destructibleTiles == null) destructibleTiles = FindFirstObjectByType<Tilemap>();
        }

        StartCoroutine(FuseRoutine());
    }

    private IEnumerator FuseRoutine()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    private void Explode()
    {
        Vector2 position = transform.position;
        position.x = Mathf.Round(position.x);
        position.y = Mathf.Round(position.y);

        // Crear explosión central
        Explosion centralExplosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        centralExplosion.SetActiveRenderer(centralExplosion.start);
        centralExplosion.DestroyAfter(explosionDuration);

        // Disparar explosiones en 4 direcciones
        ExplodeDirection(position, Vector2.up, explosionRadius);
        ExplodeDirection(position, Vector2.down, explosionRadius);
        ExplodeDirection(position, Vector2.left, explosionRadius);
        ExplodeDirection(position, Vector2.right, explosionRadius);

        Destroy(gameObject);
    }

    private void ExplodeDirection(Vector2 position, Vector2 direction, int length)
    {
        if (length <= 0) return;

        position += direction;

        // Verificar colisión con paredes o bloques
        if (Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, explosionLayerMask))
        {
            ClearDestructible(position);
            return; // Detener la propagación
        }

        Explosion explosion = Instantiate(explosionPrefab, position, Quaternion.identity);
        explosion.SetActiveRenderer(length > 1 ? explosion.middle : explosion.end);
        explosion.SetDirection(direction);
        explosion.DestroyAfter(explosionDuration);

        ExplodeDirection(position, direction, length - 1);
    }

    private void ClearDestructible(Vector2 position)
    {
        // 1. Intentar borrar si es un TILE (Tilemap)
        if (destructibleTiles != null) 
        {
            Vector3Int cell = destructibleTiles.WorldToCell(position);
            TileBase tile = destructibleTiles.GetTile(cell);

            if (tile != null)
            {
                if (destructiblePrefab != null) {
                    Instantiate(destructiblePrefab, position, Quaternion.identity);
                }
                destructibleTiles.SetTile(cell, null);
                return; // Si era un tile, hemos terminado
            }
        }

        // 2. Intentar borrar si es un GameObjects (Si no era tile, buscamos objetos físicos)
        Collider2D collider = Physics2D.OverlapBox(position, Vector2.one / 2f, 0f, explosionLayerMask);
        if (collider != null)
        {
            // Solo lo borramos si es de la capa Destructible para no borrar paredes reales
            if (collider.gameObject.layer == LayerMask.NameToLayer("Destructible"))
            {
                 // Si tiene el prefab de restos, lo instanciamos
                if (destructiblePrefab != null) {
                    Instantiate(destructiblePrefab, collider.transform.position, Quaternion.identity);
                }
                Destroy(collider.gameObject);
            }
        }
    }
}
