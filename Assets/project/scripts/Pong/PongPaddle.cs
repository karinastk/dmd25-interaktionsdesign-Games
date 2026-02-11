using UnityEngine;
using System.Collections;

namespace Pong
{
	public class PongPaddle : MonoBehaviour
	{
		public PongControls controls;

		[Header("Setup")]
		public bool isPlayer1;

		[Header("Catch Up Settings")]
		[Tooltip("Faktor für die Verlängerung (Width im Sprite Renderer)")]
		public float catchUpScaleMultiplier = 2.0f;

		private Vector3 startPosition;
		private SpriteRenderer spriteRenderer;
		private Vector2 normalSpriteSize;

		public enum Direction { Up, Down }

		void Start()
		{
			startPosition = transform.position;
			spriteRenderer = GetComponent<SpriteRenderer>();

			if (spriteRenderer != null)
			{
				// Speichert die Originalgröße des Schlägers
				normalSpriteSize = spriteRenderer.size;
			}
		}

		void Update()
		{
			// Prüft beim Manager, ob die Steuerung gerade vertauscht sein soll
			bool isInverted = PongManager.instance != null && PongManager.instance.controlsInverted;

			// Bewegungslogik unter Berücksichtigung der Invertierung
			if (Input.GetKey(controls.upKey))
				Move(isInverted ? Direction.Down : Direction.Up);

			if (Input.GetKey(controls.downKey))
				Move(isInverted ? Direction.Up : Direction.Down);
		}

		void Move(Direction direction)
		{
			float currentSpeed = controls.paddleSpeed;

			// Schläger wird schneller, wenn der Ball im "Fast Ball" Modus ist
			if (PongManager.instance != null && PongManager.instance.currentBallVelocity > PongManager.instance.ballStartVelocity)
			{
				currentSpeed *= 1.5f;
			}

			float moveDistance = currentSpeed * Time.deltaTime;
			moveDistance *= direction == Direction.Up ? 1 : -1;

			// Verhindert, dass der Schläger aus dem Spielfeld fährt
			float newY = transform.position.y + moveDistance;
			newY = Mathf.Clamp(newY, controls.minY, controls.maxY);

			transform.position = new Vector3(transform.position.x, newY, transform.position.z);
		}

		public void StartCatchUpEvent(float duration)
		{
			if (spriteRenderer == null) return;
			StopCoroutine("CatchUpRoutine");
			StartCoroutine(CatchUpRoutine(duration));
		}

		// Coroutine: Macht den Schläger für eine bestimmte Zeit größer
		IEnumerator CatchUpRoutine(float duration)
		{
			// Breite des Sprites multiplizieren (Schläger wird länger)
			spriteRenderer.size = new Vector2(normalSpriteSize.x * catchUpScaleMultiplier, normalSpriteSize.y);

			yield return new WaitForSeconds(duration);

			// Nach Ablauf der Zeit wieder auf Normalgröße setzen
			spriteRenderer.size = normalSpriteSize;
		}

		public void ResetPosition()
		{
			transform.position = startPosition;
			if (spriteRenderer != null) spriteRenderer.size = normalSpriteSize;
		}
	}
}