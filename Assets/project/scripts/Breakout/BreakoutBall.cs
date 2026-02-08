using UnityEngine;

public class BreakoutBall : MonoBehaviour
{
	private Rigidbody2D rb;
	private CircleCollider2D ballCollider;
	private bool launched = false;
	private Vector3 offset;

	[Header("Settings")]
	public float paddleBallOffsetY = -0.15f;
	public float ballSpeed = 7f;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		ballCollider = GetComponent<CircleCollider2D>();
	}

	void Update()
	{
		if (!BreakoutManager.instance.GameStarted) return;

		if (!launched)
		{
			GameObject paddleObj = GameObject.FindWithTag("Paddle");
			if (paddleObj != null)
			{
				transform.position = paddleObj.transform.position + offset;
			}
			if (Input.GetKeyDown(KeyCode.Space)) Launch();
		}
	}

	public void Launch()
	{
		launched = true;
		rb.simulated = true;
		rb.linearVelocity = Vector2.up * ballSpeed;
	}

	void FixedUpdate()
	{
		if (launched) rb.linearVelocity = rb.linearVelocity.normalized * ballSpeed;
	}

	public void CalculateOffset()
	{
		GameObject paddleObj = GameObject.FindWithTag("Paddle");
		if (paddleObj == null) return;

		Collider2D paddleCol = paddleObj.GetComponent<Collider2D>();
		float ballRadius = ballCollider.radius * transform.localScale.y;

		float yPos = 0.5f;
		if (paddleCol != null)
			yPos = (paddleCol.bounds.max.y - paddleObj.transform.position.y) + ballRadius + paddleBallOffsetY;

		offset = new Vector3(0, yPos, 0);
	}

	public void ResetBallOnDeath()
	{
		launched = false;
		rb.simulated = false;
		rb.linearVelocity = Vector2.zero;
		rb.angularVelocity = 0;
		CalculateOffset();
	}

	public void ResetBall() { ResetBallOnDeath(); }
}