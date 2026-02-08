using UnityEngine;

public class DoubleBall : MonoBehaviour
{
	void Start()
	{
		// Sehr kurze Lebenszeit, damit der Arbeitsspeicher nicht vollgestopft wird
		Destroy(gameObject, 1.2f);
	}

	private void OnTriggerEnter2D(Collider2D other)
	{
		if (other.CompareTag("Block"))
		{
			BreakoutBlock block = other.GetComponent<BreakoutBlock>();
			if (block != null)
			{
				block.TakeHit();
			}
			// Sofort löschen nach Treffer
			Destroy(gameObject);
		}

		// Auch löschen, wenn es die obere Wand oder seitliche Wände trifft
		if (other.CompareTag("Wall"))
		{
			Destroy(gameObject);
		}
	}
}