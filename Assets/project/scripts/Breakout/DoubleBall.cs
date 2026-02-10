using UnityEngine;

public class DoubleBall : MonoBehaviour
{
	void Start()
	{
		// Löscht das Projektil automatisch nach 1.2 Sek (Speicherschutz)
		Destroy(gameObject, 1.2f);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		// Wenn ein Block getroffen wird
		if (other.CompareTag("Block"))
		{
			BreakoutBlock block = other.GetComponent<BreakoutBlock>();
			if (block != null)
			{
				block.TakeHit(); // Schaden verursachen & Sound auslösen
			}
			Destroy(gameObject); // Projektil bei Treffer entfernen
		}

		// Bei Wandkontakt löschen
		if (other.CompareTag("Wall"))
		{
			Destroy(gameObject);
		}
	}
}