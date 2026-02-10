using UnityEngine;

public class PongBall : MonoBehaviour
{
	private Rigidbody2D rb;
	private bool hasStarted = false;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		rb.linearVelocity = Vector2.zero;
	}

	public void LaunchBall()
	{
		if (hasStarted) return;

		float sx = Random.Range(0, 2) == 0 ? -1f : 1f;
		float sy = Random.Range(-1f, 1f);
		Vector2 direction = new Vector2(sx, sy).normalized;

		if (PongManager.instance != null)
		{
			rb.linearVelocity = direction * PongManager.instance.currentBallVelocity;
			hasStarted = true;
		}
	}

	void FixedUpdate()
	{
		if (hasStarted && PongManager.instance != null)
		{
			// Hält den Ball immer auf der aktuell im Manager definierten Geschwindigkeit
			rb.linearVelocity = rb.linearVelocity.normalized * PongManager.instance.currentBallVelocity;
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Paddle") || collision.gameObject.name.Contains("Paddle"))
		{
			float relativeHitPos = transform.position.y - collision.transform.position.y;
			float paddleHeight = collision.collider.bounds.extents.y;
			float normalizedHitPos = relativeHitPos / paddleHeight;

			float directionX = (transform.position.x > 0) ? -1f : 1f;
			float directionY = -normalizedHitPos;

			Vector2 newDirection = new Vector2(directionX, directionY).normalized;
			rb.linearVelocity = newDirection * PongManager.instance.currentBallVelocity;
		}
	}
}