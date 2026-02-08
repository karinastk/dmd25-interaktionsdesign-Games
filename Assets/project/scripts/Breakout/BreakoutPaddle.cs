using UnityEngine;
using System.Collections;

namespace Breakout
{
	public class BreakoutPaddle : MonoBehaviour
	{
		public BreakoutControls controls;

		[Header("PowerUp Settings")]
		public GameObject bigPaddlePrefab;
		public GameObject doubleBallProjectilePrefab;
		public GameObject explosionEffectPrefab;

		[Header("Ball Physics")]
		[Range(10f, 90f)]
		public float maxBounceAngle = 40f;

		private Vector3 startPosition;
		private Vector3 originalScale;
		private Coroutine powerUpCoroutine;

		void Awake()
		{
			startPosition = transform.position;
			originalScale = transform.localScale;
		}

		void Update()
		{
			if (!BreakoutManager.instance.GameStarted) return;
			if (Input.GetKey(controls.leftKey)) Move(-1);
			if (Input.GetKey(controls.rightKey)) Move(1);
		}

		void Move(int dir)
		{
			float moveDistance = dir * controls.paddleSpeed * Time.deltaTime;
			float newX = Mathf.Clamp(transform.position.x + moveDistance, controls.minX, controls.maxX);
			transform.position = new Vector3(newX, transform.position.y, transform.position.z);
		}

		private void OnCollisionEnter2D(Collision2D collision)
		{
			if (collision.gameObject.CompareTag("Ball"))
			{
				Rigidbody2D ballRb = collision.gameObject.GetComponent<Rigidbody2D>();
				if (ballRb != null)
				{
					float paddleWidth = collision.collider.bounds.size.x;
					float relativeHitPoint = (collision.transform.position.x - transform.position.x) / (paddleWidth / 2);
					float currentSpeed = ballRb.linearVelocity.magnitude;

					float angle = relativeHitPoint * maxBounceAngle;
					Quaternion rotation = Quaternion.Euler(0, 0, -angle);
					Vector2 bounceDirection = rotation * Vector2.up;

					ballRb.linearVelocity = bounceDirection * currentSpeed;
				}
			}
		}

		public void ActivateBigPaddle(float duration, float multiplier)
		{
			if (powerUpCoroutine != null) StopCoroutine(powerUpCoroutine);
			powerUpCoroutine = StartCoroutine(BigPaddleTimer(duration, multiplier));
		}

		IEnumerator BigPaddleTimer(float duration, float multiplier)
		{
			transform.localScale = new Vector3(originalScale.x * multiplier, originalScale.y, originalScale.z);
			yield return new WaitForSeconds(duration);
			transform.localScale = originalScale;
			powerUpCoroutine = null;
		}

		public void ActivateDoubleBall(float duration)
		{
			StartCoroutine(DoubleBallRoutine(duration));
		}

		IEnumerator DoubleBallRoutine(float duration)
		{
			BreakoutManager.instance.SetMainBallActive(false);

			float elapsed = 0;
			// Schussrate auf 0.6 Sekunden erhöht (weniger CPU-Last)
			float fireRate = 0.6f;

			while (elapsed < duration)
			{
				SpawnProjectile(new Vector2(-0.15f, 1));
				SpawnProjectile(new Vector2(0.15f, 1));
				yield return new WaitForSeconds(fireRate);
				elapsed += fireRate;
			}

			BreakoutManager.instance.SetMainBallActive(true);
		}

		void SpawnProjectile(Vector2 dir)
		{
			if (doubleBallProjectilePrefab == null) return;
			GameObject proj = Instantiate(doubleBallProjectilePrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
			Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
			// Geschwindigkeit leicht erhöht, damit sie schneller das Feld verlassen
			if (rb) rb.linearVelocity = dir.normalized * 13f;
		}

		public void ResetPaddle()
		{
			StopAllCoroutines();
			transform.localScale = originalScale;
			transform.position = startPosition;
			powerUpCoroutine = null;
		}
	}
}