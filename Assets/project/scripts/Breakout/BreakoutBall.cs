using UnityEngine;

public class BreakoutBall : MonoBehaviour
{
	private Rigidbody2D rb;
	private CircleCollider2D ballCollider;
	private bool launched = false; // Status, ob der Ball frei fliegt oder noch am Paddle klebt
	private Vector3 offset;        // Abstand zum Paddle-Zentrum

	[Header("Settings")]
	public float paddleBallOffsetY = -0.15f; // Feinjustierung der Ball-Position auf dem Paddle
	public float ballSpeed = 7f;             // Die konstante Geschwindigkeit des Balls

	void Awake()
	{
		// Komponenten beim Start referenzieren
		rb = GetComponent<Rigidbody2D>();
		ballCollider = GetComponent<CircleCollider2D>();
	}

	void Update()
	{
		// Nichts tun, wenn das Spiel noch im Start-Menü ist
		if (!BreakoutManager.instance.GameStarted) return;

		// Wenn der Ball noch nicht abgeschossen wurde, folgt er dem Paddle
		if (!launched)
		{
			GameObject paddleObj = GameObject.FindWithTag("Paddle");
			if (paddleObj != null)
			{
				// Wir berechnen den Abstand jeden Frame neu, falls das Paddle 
				// durch ein Power-Up seine Größe ändert.
				CalculateOffset();
				transform.position = paddleObj.transform.position + offset;
			}

			// Abschuss mit der Leertaste
			if (Input.GetKeyDown(KeyCode.Space)) Launch();
		}
	}

	public void Launch()
	{
		launched = true;
		rb.simulated = true; // Physik einschalten
		rb.linearVelocity = Vector2.up * ballSpeed; // Mit fester Geschwindigkeit nach oben schießen
	}

	void FixedUpdate()
	{
		// Verhindert, dass der Ball durch Kollisionen langsamer oder schneller wird
		if (launched)
		{
			rb.linearVelocity = rb.linearVelocity.normalized * ballSpeed;
		}
	}

	// Berechnet die exakte Position des Balls oben auf der Paddle-Oberfläche
	public void CalculateOffset()
	{
		GameObject paddleObj = GameObject.FindWithTag("Paddle");
		if (paddleObj == null) return;

		Collider2D paddleCol = paddleObj.GetComponent<Collider2D>();
		// Radius unter Berücksichtigung der Skalierung berechnen
		float ballRadius = ballCollider.radius * transform.localScale.y;

		float yPos = 0.5f;
		if (paddleCol != null)
		{
			// Berechnet den Punkt direkt über der Oberkante des Paddles
			yPos = (paddleCol.bounds.max.y - paddleObj.transform.position.y) + ballRadius + paddleBallOffsetY;
		}

		offset = new Vector3(0, yPos, 0);
	}

	// Setzt den Ball zurück in den "Warten"-Zustand (z.B. nach Leben-Verlust)
	public void ResetBallOnDeath()
	{
		launched = false;
		rb.simulated = false; // Physik pausieren, damit er nicht vom Paddle fällt
		rb.linearVelocity = Vector2.zero;
		rb.angularVelocity = 0;
		CalculateOffset();
	}

	public void ResetBall() { ResetBallOnDeath(); }
}