using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Breakout
{
	public class BreakoutPaddle : MonoBehaviour
	{
		public BreakoutControls controls; // Referenz auf Geschwindigkeits- und Tasten-Einstellungen

		[Header("PowerUp Settings")]
		public GameObject bigPaddlePrefab;
		public GameObject doubleBallProjectilePrefab; // Die kleinen Kugeln für die Kanonen
		public GameObject explosionEffectPrefab;
		public Transform[] projectileSpawnPoints; // Die "Mündungen" der Kanonen am Paddle

		[Header("Ball Physics")]
		[Range(10f, 90f)]
		public float maxBounceAngle = 40f; // Maximale Schräge, in der der Ball abprallen kann

		private Vector3 startPosition;
		private Vector3 originalScale;
		private Coroutine powerUpCoroutine;     // Hält den Timer für "Großes Paddle"
		private Coroutine doubleBallCoroutine;  // Hält den Timer für die Kanonen

		// Liste, um die abgeschossenen kleinen Kugeln zu zählen
		private List<GameObject> activeProjectiles = new List<GameObject>();

		void Awake()
		{
			// Speichert die Startwerte, um sie beim Reset oder nach Power-Ups wiederherzustellen
			startPosition = transform.position;
			originalScale = transform.localScale;
		}

		void Update()
		{
			// Nur bewegen, wenn der Manager das Spiel freigegeben hat
			if (!BreakoutManager.instance.GameStarted) return;

			if (Input.GetKey(controls.leftKey)) Move(-1);
			if (Input.GetKey(controls.rightKey)) Move(1);
		}

		void Move(int dir)
		{
			// deltaTime macht die Bewegung unabhängig von der Bildrate (FPS)
			float moveDistance = dir * controls.paddleSpeed * Time.deltaTime;
			// Clamp verhindert, dass das Paddle links oder rechts aus dem Bildschirm fährt
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
					// BERECHNUNG DES ABPRALLWINKELS:
					// Wir messen, wie weit der Ball von der Mitte des Paddles entfernt aufschlägt.
					float paddleWidth = collision.collider.bounds.size.x;
					float relativeHitPoint = (collision.transform.position.x - transform.position.x) / (paddleWidth / 2);
					float currentSpeed = ballRb.linearVelocity.magnitude;

					// Erzeugt eine Drehung basierend auf dem Treffpunkt
					float angle = relativeHitPoint * maxBounceAngle;
					Quaternion rotation = Quaternion.Euler(0, 0, -angle);
					Vector2 bounceDirection = rotation * Vector2.up;

					// Ball mit der ursprünglichen Geschwindigkeit in die neue Richtung schicken
					ballRb.linearVelocity = bounceDirection * currentSpeed;
				}
			}
		}

		// POWER-UP: PADDLE VERGRÖSSERN
		public void ActivateBigPaddle(float duration, float multiplier)
		{
			if (powerUpCoroutine != null) StopCoroutine(powerUpCoroutine);
			powerUpCoroutine = StartCoroutine(BigPaddleTimer(duration, multiplier));
		}

		IEnumerator BigPaddleTimer(float duration, float multiplier)
		{
			transform.localScale = new Vector3(originalScale.x * multiplier, originalScale.y, originalScale.z);
			yield return new WaitForSeconds(duration); // Zeit abwarten
			transform.localScale = originalScale;     // Zurück zur normalen Größe
			powerUpCoroutine = null;
		}

		// POWER-UP: KANONEN (Double Ball)
		public void ActivateDoubleBall(float duration)
		{
			if (doubleBallCoroutine != null) StopCoroutine(doubleBallCoroutine);
			doubleBallCoroutine = StartCoroutine(DoubleBallRoutine(duration));
		}

		IEnumerator DoubleBallRoutine(float duration)
		{
			// Während die Kanonen schießen, pausiert der Hauptball oft (oder wird hier deaktiviert)
			BreakoutManager.instance.SetMainBallActive(false);

			float elapsed = 0;
			float fireRate = 0.6f;
			int maxProjectiles = 10; // Maximale Anzahl an Kugeln gleichzeitig auf dem Schirm

			while (elapsed < duration)
			{
				// Bereinigt die Liste von Kugeln, die bereits zerstört wurden
				activeProjectiles.RemoveAll(item => item == null);
				int freeSlots = Mathf.Max(0, maxProjectiles - activeProjectiles.Count);

				// Wenn wir noch Kapazität zum Schießen haben
				if (projectileSpawnPoints != null && projectileSpawnPoints.Length >= 2)
				{
					if (freeSlots >= 2)
					{
						SpawnProjectile(projectileSpawnPoints[0].position, Vector2.up);
						SpawnProjectile(projectileSpawnPoints[1].position, Vector2.up);
					}
				}
				else if (freeSlots >= 2) // Fallback: Falls keine Kanonen-Punkte im Editor zugewiesen wurden
				{
					SpawnProjectile(transform.position + new Vector3(-0.5f, 0.5f, 0), Vector2.up);
					SpawnProjectile(transform.position + new Vector3(0.5f, 0.5f, 0), Vector2.up);
				}

				yield return new WaitForSeconds(fireRate);
				elapsed += fireRate;
			}

			BreakoutManager.instance.SetMainBallActive(true);
			doubleBallCoroutine = null;
		}

		void SpawnProjectile(Vector3 spawnPos, Vector2 dir)
		{
			if (doubleBallProjectilePrefab == null) return;

			// Erzeugt die Kugel und fügt sie der Liste hinzu
			GameObject proj = Instantiate(doubleBallProjectilePrefab, spawnPos, Quaternion.identity);
			activeProjectiles.Add(proj);

			Rigidbody2D rb = proj.GetComponent<Rigidbody2D>();
			if (rb) rb.linearVelocity = dir.normalized * 13f; // Schiebt die Kugel nach oben
		}

		// ALLES ZURÜCKSETZEN (bei Game Over oder Neustart)
		public void ResetPaddle()
		{
			StopAllCoroutines(); // Alle Power-Up Timer stoppen
			transform.localScale = originalScale;
			transform.position = startPosition;

			// Alle noch fliegenden Projektile löschen
			foreach (var proj in activeProjectiles)
			{
				if (proj != null) Destroy(proj);
			}
			activeProjectiles.Clear();

			powerUpCoroutine = null;
			doubleBallCoroutine = null;
		}
	}
}