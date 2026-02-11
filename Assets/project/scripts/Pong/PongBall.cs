using UnityEngine;

public class PongBall : MonoBehaviour
{
	private Rigidbody2D rb;
	private bool hasStarted = false;

	[Header("Audio Settings")]
	public AudioClip hitSound;
	private AudioSource audioSource;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		rb.linearVelocity = Vector2.zero; // Ball bleibt beim Spawn erst einmal stehen

		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
		{
			audioSource = gameObject.AddComponent<AudioSource>();
		}

		audioSource.playOnAwake = false;
		audioSource.loop = false;
	}

	// Wird vom PongManager aufgerufen, wenn Space gedrückt wird
	public void LaunchBall()
	{
		if (hasStarted) return;

		// Bestimmt zufällig, ob der Ball nach links oder rechts startet
		float sx = Random.Range(0, 2) == 0 ? -1f : 1f;
		float sy = Random.Range(-1f, 1f);
		Vector2 direction = new Vector2(sx, sy).normalized;

		if (PongManager.instance != null)
		{
			// Nutzt die Geschwindigkeit, die im Manager eingestellt ist
			rb.linearVelocity = direction * PongManager.instance.currentBallVelocity;
			hasStarted = true;
		}
	}

	void FixedUpdate()
	{
		if (hasStarted && PongManager.instance != null)
		{
			// WICHTIG: Stellt sicher, dass der Ball nie langsamer oder schneller wird als gewollt
			// (z.B. nach Kollisionen oder wenn das "Fast Ball" Event startet)
			rb.linearVelocity = rb.linearVelocity.normalized * PongManager.instance.currentBallVelocity;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		// Spielt den Sound bei jedem Aufprall ab
		if (audioSource != null && hitSound != null)
		{
			audioSource.PlayOneShot(hitSound);
		}

		// Berechnet den Abprallwinkel basierend darauf, wo der Ball das Paddle trifft
		if (collision.gameObject.CompareTag("Paddle") || collision.gameObject.name.Contains("Paddle"))
		{
			// Ermittelt die Treffposition relativ zur Mitte des Schlägers
			float relativeHitPos = transform.position.y - collision.transform.position.y;
			float paddleHeight = collision.collider.bounds.extents.y;
			float normalizedHitPos = relativeHitPos / paddleHeight;

			// Bestimmt die neue Richtung (X dreht sich um, Y hängt vom Treffpunkt ab)
			float directionX = (transform.position.x > 0) ? -1f : 1f;
			float directionY = normalizedHitPos; // Oben getroffen -> fliegt nach oben weg

			Vector2 newDirection = new Vector2(directionX, directionY).normalized;
			rb.linearVelocity = newDirection * PongManager.instance.currentBallVelocity;
		}
	}
}