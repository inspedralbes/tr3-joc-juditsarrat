using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public enum ItemType
    {
        ExtraBomb,
        BlastRadius,
        SpeedIncrease,
    }

    public ItemType type;

    private void OnItemPickup(GameObject player)
    {
      switch (type)
      {
        case ItemType.ExtraBomb:
        player.GetComponent<BombController>().AddBomb();
        break;

        case ItemType.BlastRadius:
        player.GetComponent<BombController>().explosionRadius++;
        break;
      
        case ItemType.SpeedIncrease:
        player.GetComponent<MovementController>().speed++;
        break;
    }

    Destroy(gameObject);
}

   private void OnTriggerEnter2D(Collider2D other)
    {
        // En vez de mirar si tiene la etiqueta "Player", comprobamos si "sabe poner bombas". 
        // Así tanto "Player 1", como la "IA" (Target), pueden recoger los items sin importar su etiqueta.
        if (other.GetComponent<BombController>() != null)
        {
            OnItemPickup(other.gameObject);
        }
    }
}
