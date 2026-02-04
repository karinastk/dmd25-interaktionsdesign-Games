using UnityEngine;

public class BreakoutBall : MonoBehaviour
{
	private Rigidbody2D rb;
	private CircleCollider2D ballCollider;
	private bool launched = false;

	private Vector3 offset;

	public float paddleBallOffsetY = 0.05f;

	void Awake()
	{
		rb = GetComponent<Rigidbody2D>();
		ballCollider = GetComponent<CircleCollider2D>();

		rb.linearVelocity = Vector2.zero;
		rb.simulated = false;
	}

	void Update()
	{
		if (!BreakoutManager.instance.GameStarted)
			return;

		if (!launched)
		{
			Transform paddle = GameObject.FindWithTag("Paddle").transform;
			transform.position = paddle.position + offset;

			if (Input.GetKeyDown(KeyCode.Space))
			{
				Launch();
			}
		}
	}

	void Launch()
	{
		launched = true;
		rb.simulated = true;

		float speed = BreakoutManager.instance.ballStartVelocity;
		rb.velocity = new Vector2(0, speed);

		BreakoutManager.instance.StartTimer();
	}

	void CalculateOffset()
	{
		Transform paddle = GameObject.FindWithTag("Paddle").transform;
		BoxCollider2D paddleCollider = paddle.GetComponent<BoxCollider2D>();

		float paddleTop = paddleCollider.bounds.max.y;
		float ballRadius = ballCollider.radius * transform.localScale.y;

		offset = new Vector3(
			0,
			(paddleTop - paddle.position.y) + ballRadius + paddleBallOffsetY,
			0
		);
	}

	public void ResetBallOnDeath()
	{
		launched = false;
		rb.simulated = false;
		rb.velocity = Vector2.zero;
		rb.angularVelocity = 0;

		CalculateOffset();

		Transform paddle = GameObject.FindWithTag("Paddle").transform;
		transform.position = paddle.position + offset;
	}

	public void ResetBall()
	{
		launched = false;
		rb.simulated = false;
		rb.velocity = Vector2.zero;
		rb.angularVelocity = 0;

		CalculateOffset();

		Transform paddle = GameObject.FindWithTag("Paddle").transform;
		transform.position = paddle.position + offset;
	}
}
