using UnityEngine;

public class BreakoutBlock : MonoBehaviour
{
	[Header("Block Hits")]
	public int maxHits = 1;      // Wie oft muss der Block getroffen werden?
	private int currentHits;     // Aktuelle Lebenspunkte des Blocks

	[Header("Crack Sprites")]
	public Sprite[] damageSprites; // Liste der Bilder für Risse (0 = erster Riss, etc.)

	private SpriteRenderer sr;
	private Collider2D col;
	private Sprite startSprite;  // Speichert das ursprüngliche Aussehen (ohne Risse)

	// Status, ob der Ball gerade einfach durch den Block hindurchfliegen kann
	public bool IsInGhostMode { get; private set; }

	void Awake()
	{
		sr = GetComponent<SpriteRenderer>();
		col = GetComponent<Collider2D>();
		currentHits = maxHits;

		// Start-Sprite merken, damit wir den Block später resetten können
		if (sr != null)
			startSprite = sr.sprite;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (IsInGhostMode) return; // Im Geist-Modus gibt es keine Kollision

		if (collision.gameObject.CompareTag("Ball"))
		{
			TakeHit();
		}
	}

	public void TakeHit()
	{
		if (IsInGhostMode) return;

		// SOUND: Meldet dem Manager, dass ein Treffer-Sound abgespielt werden soll
		if (BreakoutManager.instance != null)
			BreakoutManager.instance.PlaySFX(BreakoutManager.instance.blockHitSound);

		currentHits--;

		// Prüfen, ob der Block zerstört ist
		if (currentHits <= 0)
		{
			// Manager informieren (für Punkte, Power-Up Drops und Sieg-Check)
			BreakoutManager.instance.OnBlockDestroyed(transform.position, this);
			gameObject.SetActive(false); // Block "verschwindet" aus dem Spiel
			return;
		}

		// Wenn noch Leben übrig sind, zeige den nächsten Riss an
		UpdateCracks();
	}

	// Schaltet den Block unsichtbar und berührungslos (für das Ghost-Event)
	public void SetGhostMode(bool isGhost)
	{
		IsInGhostMode = isGhost;
		if (sr != null) sr.enabled = !isGhost;
		if (col != null) col.enabled = !isGhost;
	}

	void UpdateCracks()
	{
		// Berechnet, welches Riss-Bild basierend auf den verbleibenden Hits gezeigt wird
		int index = maxHits - currentHits - 1;

		if (index >= 0 && index < damageSprites.Length && sr != null)
		{
			sr.sprite = damageSprites[index];
		}
	}

	// Setzt den Block für eine neue Runde komplett zurück
	public void ResetBlock()
	{
		currentHits = maxHits;
		IsInGhostMode = false;
		if (sr != null)
		{
			sr.sprite = startSprite; // Ursprüngliches Bild wiederherstellen
			sr.enabled = true;
		}
		if (col != null) col.enabled = true;
		gameObject.SetActive(true);
	}
}