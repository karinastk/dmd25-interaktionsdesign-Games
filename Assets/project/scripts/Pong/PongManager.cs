using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PongManager : MonoBehaviour
{
	// Enum zur Identifikation der Spieler
	public enum Player { Player1, Player2 }

	// Singleton-Instanz für globalen Zugriff
	public static PongManager instance;

	[Header("Audio Settings")]
	public AudioSource musicSource;        // Hintergrundmusik
	public AudioSource sfxSource;          // Soundeffekte
	public AudioClip startBannerSound;     // Sound beim Spielstart
	public AudioClip buttonClickSound;     // UI-Klick-Sound
	public AudioClip eventWarningSound;    // Sound für Special Events
	public AudioClip winSound;             // Sieg-Sound
	public AudioClip drawSound;            // Unentschieden-Sound
	public AudioClip pointScoredSound;     // Punkt erzielt Sound

	[Header("Game Settings")]
	public GameObject ballPrefab;          // Ball Prefab
	public float ballStartVelocity = 10f;  // Normale Ballgeschwindigkeit
	public float ballFastVelocity = 18f;   // Erhöhte Geschwindigkeit bei Event
	[HideInInspector] public float currentBallVelocity; // Aktuelle Geschwindigkeit

	[Header("Timer Settings")]
	public float gameTimeInSeconds = 180f; // Gesamtdauer des Spiels
	public TextMeshProUGUI timerText;      // Timer UI Text

	[Header("Catch Up Settings")]
	public int scoreDifferenceThreshold = 5; // Punktedifferenz für Aufhol-Event

	[Header("Obstacle Settings")]
	public GameObject obstacleContainer;   // Container für Hindernisse

	[Header("Special Events")]
	public bool controlsInverted = false;  // Gibt an, ob Steuerung invertiert ist
	public TextMeshProUGUI eventText;      // Textanzeige für Events

	// Warnfarbe für Eventtexte (RGBA)
	private Color eventWarningColor = new Color(255f / 255f, 201f / 255f, 115f / 255f, 90f / 255f);

	[Header("UI References")]
	public TextMeshProUGUI player1ScoreText; // Punkteanzeige Spieler 1
	public TextMeshProUGUI player2ScoreText; // Punkteanzeige Spieler 2
	public TextMeshProUGUI winnerText;       // Gewinnertext
	public GameObject gameOverObject;        // Game Over UI
	public GameObject startMenuObject;       // Startmenü UI

	// Interne Statusvariablen
	private bool isGameRunning = false;
	private bool isTimerRunning = false;
	private bool isShuttingDown = false;
	private bool isFirstStart = true;

	private int player1Score;
	private int player2Score;
	private float timeRemaining;

	private void Awake()
	{
		// Singleton-Pattern: Nur eine Instanz darf existieren
		if (instance == null) instance = this;
		else Destroy(gameObject);
	}

	void Start()
	{
		isShuttingDown = false;

		// UI Initialisierung
		gameOverObject.SetActive(false);
		startMenuObject.SetActive(true);

		// Musik starten, falls vorhanden
		if (musicSource != null && !musicSource.isPlaying) musicSource.Play();

		// Hindernisse zu Beginn deaktivieren
		if (obstacleContainer != null) obstacleContainer.SetActive(false);

		// Event-Text vorbereiten (unsichtbar)
		if (eventText != null)
		{
			eventText.gameObject.SetActive(false);
			Color c = eventWarningColor;
			c.a = 0;
			eventText.color = c;
		}

		// Spielwerte zurücksetzen
		timeRemaining = gameTimeInSeconds;
		currentBallVelocity = ballStartVelocity;
		player1Score = 0;
		player2Score = 0;

		UpdateScoreUI();
		UpdateTimerUI();
	}

	void Update()
	{
		// Spielstart mit Leertaste
		if (isGameRunning && isFirstStart)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				isFirstStart = false;
				isTimerRunning = true;

				// Spezial-Events starten
				StartCoroutine(InvertControlRoutine());
				StartCoroutine(SpeedEventRoutine());
				StartCoroutine(ObstacleRoutine());

				LaunchCurrentBall();
			}
		}

		// Timer-Logik
		if (isTimerRunning)
		{
			if (timeRemaining > 0)
			{
				timeRemaining -= Time.deltaTime;
				UpdateTimerUI();
			}
			else
			{
				timeRemaining = 0;
				EndMatch();
			}
		}
	}

	// Spielt einen Soundeffekt ab
	public void PlaySound(AudioClip clip)
	{
		if (sfxSource != null && clip != null)
		{
			sfxSource.PlayOneShot(clip);
		}
	}

	// Startet das Spiel aus dem Menü
	public void StartGame()
	{
		PlaySound(buttonClickSound);
		startMenuObject.SetActive(false);

		// UI wieder aktivieren
		if (timerText != null) timerText.gameObject.SetActive(true);
		if (player1ScoreText != null) player1ScoreText.gameObject.SetActive(true);
		if (player2ScoreText != null) player2ScoreText.gameObject.SetActive(true);

		isGameRunning = true;
		isFirstStart = true;
		isTimerRunning = false;
		timeRemaining = gameTimeInSeconds;
		controlsInverted = false;
		currentBallVelocity = ballStartVelocity;

		if (obstacleContainer != null) obstacleContainer.SetActive(false);

		StopAllCoroutines(); // Sicherheitshalber alte Coroutines stoppen
		PlaySound(startBannerSound);

		ResetBall(false); // Ball erzeugen, aber noch nicht starten
	}

	// Wird aufgerufen, wenn ein Tor erzielt wurde
	public void OnGoalScored(Player player)
	{
		if (!isGameRunning || isShuttingDown) return;

		PlaySound(pointScoredSound);

		// Punktestand erhöhen
		if (player == Player.Player1) player1Score++;
		else player2Score++;

		UpdateScoreUI();
		CheckForPaddlePowerup(); // Prüfen auf Aufholmechanik
		RemoveAllBalls();        // Alte Bälle entfernen

		if (isGameRunning) ResetBall(true);
	}

	// Event: Steuerung wird invertiert
	IEnumerator InvertControlRoutine()
	{
		yield return new WaitForSeconds(30f);

		if (isGameRunning)
		{
			controlsInverted = true;
			PlaySound(eventWarningSound);
			yield return StartCoroutine(FadeMessage("CONTROLS SWAPPED!", eventWarningColor));

			yield return new WaitForSeconds(15f);

			controlsInverted = false;
			yield return StartCoroutine(FadeMessage("CONTROLS NORMAL", eventWarningColor));
		}
	}

	// Event: Ball wird schneller
	IEnumerator SpeedEventRoutine()
	{
		yield return new WaitForSeconds(90f);

		if (isGameRunning)
		{
			currentBallVelocity = ballFastVelocity;
			PlaySound(eventWarningSound);
			yield return StartCoroutine(FadeMessage("FAST BALL!", eventWarningColor));

			yield return new WaitForSeconds(15f);

			currentBallVelocity = ballStartVelocity;
			yield return StartCoroutine(FadeMessage("SPEED NORMAL", eventWarningColor));
		}
	}

	// Event: Hindernisse erscheinen
	IEnumerator ObstacleRoutine()
	{
		yield return new WaitForSeconds(150f);

		if (isGameRunning && obstacleContainer != null)
		{
			obstacleContainer.SetActive(true);
			PlaySound(eventWarningSound);
			yield return StartCoroutine(FadeMessage("FINAL STAGE: OBSTACLES!", eventWarningColor));

			yield return new WaitForSeconds(15f);

			obstacleContainer.SetActive(false);
			yield return StartCoroutine(FadeMessage("OBSTACLES GONE", eventWarningColor));
		}
	}

	// Blendet Event-Text ein und wieder aus
	public IEnumerator FadeMessage(string message, Color textColor)
	{
		if (eventText != null)
		{
			eventText.text = message;
			eventText.gameObject.SetActive(true);

			float duration = 0.8f;
			float elapsed = 0f;
			float targetAlpha = textColor.a;

			// Einblenden
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				textColor.a = Mathf.Lerp(0, targetAlpha, elapsed / duration);
				eventText.color = textColor;
				yield return null;
			}

			yield return new WaitForSeconds(1.5f);

			// Ausblenden
			elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				textColor.a = Mathf.Lerp(targetAlpha, 0, elapsed / duration);
				eventText.color = textColor;
				yield return null;
			}

			eventText.gameObject.SetActive(false);
		}
	}

	// Prüft, ob ein Spieler deutlich zurückliegt (Catch-Up Mechanik)
	private void CheckForPaddlePowerup()
	{
		Pong.PongPaddle[] paddles = Object.FindObjectsByType<Pong.PongPaddle>(FindObjectsSortMode.None);
		int diff = Mathf.Abs(player1Score - player2Score);

		if (diff == scoreDifferenceThreshold)
		{
			foreach (var paddle in paddles)
			{
				if (player2Score == player1Score + scoreDifferenceThreshold && paddle.isPlayer1)
					paddle.StartCatchUpEvent(8f);
				else if (player1Score == player2Score + scoreDifferenceThreshold && !paddle.isPlayer1)
					paddle.StartCatchUpEvent(8f);
			}
		}
	}

	// Startet den aktuell existierenden Ball
	private void LaunchCurrentBall()
	{
		PongBall ball = Object.FindFirstObjectByType<PongBall>();
		if (ball != null) ball.LaunchBall();
	}

	// Erstellt einen neuen Ball
	public void ResetBall(bool autoLaunch)
	{
		if (!isShuttingDown)
		{
			GameObject newBall = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);

			if (autoLaunch)
			{
				PongBall ballScript = newBall.GetComponent<PongBall>();
				if (ballScript != null) ballScript.LaunchBall();
			}
		}
	}

	// Aktualisiert Punktestand UI
	void UpdateScoreUI()
	{
		player1ScoreText.text = player1Score.ToString();
		player2ScoreText.text = player2Score.ToString();
	}

	// Aktualisiert Timer UI
	void UpdateTimerUI()
	{
		int minutes = Mathf.FloorToInt(timeRemaining / 60);
		int seconds = Mathf.FloorToInt(timeRemaining % 60);
		timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
	}

	// Beendet das Match
	void EndMatch()
	{
		isTimerRunning = false;
		isGameRunning = false;

		if (musicSource != null) musicSource.Stop();

		DetermineWinner();
	}

	// Bestimmt Gewinner und zeigt Game Over UI
	void DetermineWinner()
	{
		gameOverObject.SetActive(true);
		RemoveAllBalls();

		if (timerText != null) timerText.gameObject.SetActive(false);
		if (player1ScoreText != null) player1ScoreText.gameObject.SetActive(false);
		if (player2ScoreText != null) player2ScoreText.gameObject.SetActive(false);

		if (player1Score == player2Score)
		{
			PlaySound(drawSound);
			winnerText.text = "IT'S A DRAW!\n" + player1Score + " : " + player2Score;
		}
		else
		{
			PlaySound(winSound);
			string winnerStr = (player1Score > player2Score) ? "PLAYER 1 WON!" : "PLAYER 2 WON!";
			winnerText.text = winnerStr + "\n" + player1Score + " : " + player2Score;
		}
	}

	// Entfernt alle aktiven Bälle
	private void RemoveAllBalls()
	{
		GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball");
		foreach (GameObject b in balls)
			Destroy(b);
	}

	// Startet Szene neu
	public void RestartGame()
	{
		PlaySound(buttonClickSound);
		isShuttingDown = true;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	// Wird beim Beenden der Anwendung aufgerufen
	private void OnApplicationQuit()
	{
		isShuttingDown = true;
	}
}
