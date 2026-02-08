using UnityEngine;

public class BreakoutBlock : MonoBehaviour
{
	[Header("Block Hits")]
	public int maxHits = 1;
	private int currentHits;

	[Header("Crack Sprites")]
	public Sprite[] damageSprites;

	private SpriteRenderer sr;
	private Collider2D col; // Erkennt automatisch Box-, Circle- oder Polygon-Collider
	private Sprite startSprite;

	// Status-Check für den Manager
	public bool IsInGhostMode { get; private set; }

	void Awake()
	{
		sr = GetComponent<SpriteRenderer>();
		col = GetComponent<Collider2D>();
		currentHits = maxHits;

		if (sr != null)
			startSprite = sr.sprite;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		// Wenn wir unsichtbar sind, ignorieren wir die Kollision komplett
		if (IsInGhostMode) return;

		if (collision.gameObject.CompareTag("Ball"))
		{
			TakeHit();
		}
	}

	public void TakeHit()
	{
		// Zusätzlicher Schutz: Kein Schaden im Ghost-Mode (z.B. durch Explosionen)
		if (IsInGhostMode) return;

		currentHits--;

		if (currentHits <= 0)
		{
			BreakoutManager.instance.OnBlockDestroyed(transform.position, this);
			gameObject.SetActive(false);
			return;
		}

		UpdateCracks();
	}

	// Steuerung durch den BreakoutManager (alle 30 Sek.)
	public void SetGhostMode(bool isGhost)
	{
		IsInGhostMode = isGhost;

		// Grafik ausblenden
		if (sr != null)
			sr.enabled = !isGhost;

		// Physik ausschalten (Ball fliegt hindurch)
		if (col != null)
			col.enabled = !isGhost;
	}

	void UpdateCracks()
	{
		int index = maxHits - currentHits - 1;
		if (index >= 0 && index < damageSprites.Length && sr != null)
		{
			sr.sprite = damageSprites[index];
		}
	}

	public void ResetBlock()
	{
		currentHits = maxHits;
		IsInGhostMode = false;

		if (sr != null)
		{
			sr.sprite = startSprite;
			sr.enabled = true;
		}

		if (col != null)
			col.enabled = true;

		gameObject.SetActive(true);
	}
}