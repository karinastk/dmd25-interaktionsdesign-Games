using System.Collections;
using System.Collections.Generic;
using TMPro; // Nötig für TextMeshPro (UI Texte)
using UnityEngine;

public class BreakoutManager : MonoBehaviour
{
	// Das "Singleton"-Muster: Erlaubt es, von überall mit 'BreakoutManager.instance' 
	// auf diesen Manager zuzugreifen, ohne eine Referenz suchen zu müssen.
	public static BreakoutManager instance;

	[Header("Audio Settings")]
	public AudioSource backgroundMusicSource; // Die Komponente, die Musik abspielt
	public AudioSource sfxSource;             // Die Komponente für Soundeffekte

	[Space(10)] // Erzeugt eine Lücke im Unity-Inspector
	public AudioClip backgroundMusicClip;
	public AudioClip startButtonClickSound;
	public AudioClip menuButtonClickSound;
	public AudioClip blockHitSound;         // JEDER Treffer (wird vom Block-Skript getriggert)
	public AudioClip blockDestroyedSound;   // Wenn der Block ganz verschwindet
	public AudioClip explosionSound;        // Sound für das Leertasten-Event
	public AudioClip powerUpPickupSound;    // Wenn das Paddle ein Item berührt
	public AudioClip restartSound;
	public AudioClip gameOverSound;
	public AudioClip winSound;

	[Header("Ball")]
	public GameObject ballPrefab; // Die Vorlage für den Ball
	private GameObject currentBall; // Der aktuell im Spiel befindliche Ball

	[Header("Paddle")]
	public Breakout.BreakoutPaddle paddle; // Referenz auf das Paddle-Skript

	[Header("Powerup Prefabs (Drops)")]
	public GameObject bigPaddleDropPrefab;   // Das fallende Item für großes Paddle
	public GameObject doubleBallDropPrefab;  // Das fallende Item für Kanonen
	public GameObject explosionDropPrefab;   // Das fallende Item für Explosionen

	// Dictionary: Speichert wie eine Liste, welcher Block welches Power-Up "in sich trägt"
	private Dictionary<BreakoutBlock, GameObject> powerUpAssignment = new Dictionary<BreakoutBlock, GameObject>();

	private bool explosionReady = false; // Wird true, wenn man das Explosions-Powerup einsammelt
	private Coroutine explosionTimerCoroutine; // Speichert den laufenden Timer des Powerups
	private Coroutine ghostBlocksCoroutine; // Speichert den Ablauf der Geisterblöcke

	[Header("UI Panels")]
	public GameObject startPanel;    // Das Menü am Anfang
	public GameObject gameOverPanel; // Das Menü am Ende

	[Header("UI Text")]
	public TextMeshProUGUI livesText;
	public TextMeshProUGUI timerText;
	public TextMeshProUGUI resultTitleText;
	public TextMeshProUGUI resultStatsText;

	[Header("Game Settings")]
	public int totalLives = 3;            // Start-Leben
	private int currentLives;             // Aktuelle Leben im Spielverlauf
	public float startTimerValue = 60f;   // Start-Zeit in Sekunden
	private float timeRemaining;          // Laufende Uhr
	private bool timerRunning = false;
	private bool gameEnded = false;
	private int remainingBlocks;          // Zähler: Wie viele Blöcke sind noch im Level?

	// Ein "Property": Andere Skripte können es lesen, aber nicht verändern
	public bool GameStarted { get; private set; } = false;

	void Awake()
	{
		// Singleton-Logik: Falls es schon einen Manager gibt, lösche mich. 
		// Sonst bin ich die 'instance'.
		if (instance == null) instance = this;
		else Destroy(gameObject);
	}

	void Start()
	{
		// Grundzustand beim Laden der Szene herstellen
		startPanel.SetActive(true);
		gameOverPanel.SetActive(false);
		currentLives = totalLives;
		UpdateLivesUI();
		timeRemaining = startTimerValue;
		UpdateTimerText();
		SetupBlocksAndPowerups(); // Blöcke zählen und Power-Ups verstecken
		ResetBall();

		// Musik beim Starten vorbereiten
		if (backgroundMusicSource != null && backgroundMusicClip != null)
		{
			backgroundMusicSource.clip = backgroundMusicClip;
			backgroundMusicSource.loop = true; // Musik soll sich wiederholen
			backgroundMusicSource.Play();
		}
	}

	// Bereitet das Level vor: Zählt Blöcke und verteilt Powerups
	void SetupBlocksAndPowerups()
	{
		BreakoutBlock[] allBlocks = FindObjectsOfType<BreakoutBlock>(true);
		remainingBlocks = allBlocks.Length;
		powerUpAssignment.Clear();

		List<BreakoutBlock> tempPool = new List<BreakoutBlock>(allBlocks);

		AssignToPool(tempPool, bigPaddleDropPrefab, 3);
		AssignToPool(tempPool, doubleBallDropPrefab, 3);
		AssignToPool(tempPool, explosionDropPrefab, 3);
	}

	void AssignToPool(List<BreakoutBlock> pool, GameObject prefab, int amount)
	{
		for (int i = 0; i < amount; i++)
		{
			if (pool.Count == 0) break;
			int r = Random.Range(0, pool.Count);
			if (!powerUpAssignment.ContainsKey(pool[r]))
				powerUpAssignment.Add(pool[r], prefab);
			pool.RemoveAt(r);
		}
	}

	public void OnBlockDestroyed(Vector3 pos, BreakoutBlock block)
	{
		if (gameEnded) return;

		PlaySFX(blockDestroyedSound);

		if (powerUpAssignment.ContainsKey(block))
			Instantiate(powerUpAssignment[block], pos, Quaternion.identity);

		remainingBlocks--;
		if (remainingBlocks <= 0) WinGame();
	}

	public void ActivateExplosion(float dur)
	{
		if (explosionTimerCoroutine != null) StopCoroutine(explosionTimerCoroutine);
		explosionTimerCoroutine = StartCoroutine(ExplosionReadyTimer(dur));
	}

	IEnumerator ExplosionReadyTimer(float dur)
	{
		explosionReady = true;
		yield return new WaitForSeconds(dur);
		explosionReady = false;
	}

	void TriggerManualExplosion()
	{
		explosionReady = false;
		if (explosionTimerCoroutine != null) StopCoroutine(explosionTimerCoroutine);

		if (currentBall != null && currentBall.activeInHierarchy)
		{
			PlaySFX(explosionSound);

			Collider2D[] hits = Physics2D.OverlapCircleAll(currentBall.transform.position, 2.5f);
			foreach (var hit in hits)
			{
				if (hit.CompareTag("Block"))
				{
					BreakoutBlock block = hit.GetComponent<BreakoutBlock>();
					if (block != null) block.TakeHit();
				}
			}
		}
	}

	void Update()
	{
		if (!GameStarted || gameEnded) return;

		if (timerRunning)
		{
			if (timeRemaining > 0)
			{
				timeRemaining -= Time.deltaTime;
				UpdateTimerText();
			}
			else
			{
				timeRemaining = 0;
				GameOver();
			}
		}

		if (explosionReady && Input.GetKeyDown(KeyCode.Space)) TriggerManualExplosion();
	}

	public void StartGame()
	{
		PlaySFX(startButtonClickSound);
		GameStarted = true;
		startPanel.SetActive(false);
		timerRunning = true;

		if (ghostBlocksCoroutine != null) StopCoroutine(ghostBlocksCoroutine);
		ghostBlocksCoroutine = StartCoroutine(GhostBlockRoutine());
	}

	public void PlayMenuSound() { PlaySFX(menuButtonClickSound); }

	IEnumerator GhostBlockRoutine()
	{
		while (!gameEnded)
		{
			yield return new WaitForSeconds(30f);

			BreakoutBlock[] allBlocks = FindObjectsOfType<BreakoutBlock>();
			List<BreakoutBlock> availableBlocks = new List<BreakoutBlock>();
			foreach (var b in allBlocks) if (b.gameObject.activeInHierarchy) availableBlocks.Add(b);

			if (availableBlocks.Count > 0)
			{
				List<BreakoutBlock> chosenOnes = new List<BreakoutBlock>();
				int amount = Mathf.Min(6, availableBlocks.Count);

				for (int i = 0; i < amount; i++)
				{
					int r = Random.Range(0, availableBlocks.Count);
					chosenOnes.Add(availableBlocks[r]);
					availableBlocks.RemoveAt(r);
				}

				foreach (var b in chosenOnes) b.SetGhostMode(true);
				yield return new WaitForSeconds(3f);
				foreach (var b in chosenOnes) b.SetGhostMode(false);
			}
		}
	}

	public void OnDeath()
	{
		if (!GameStarted || gameEnded) return;
		currentLives--;
		UpdateLivesUI();

		if (currentLives <= 0) GameOver();
		else if (currentBall != null)
			currentBall.GetComponent<BreakoutBall>().ResetBallOnDeath();
	}

	void WinGame() { EndGameCleanUp(); PlaySFX(winSound); ShowResult(true); }
	void GameOver() { EndGameCleanUp(); PlaySFX(gameOverSound); ShowResult(false); }

	void EndGameCleanUp()
	{
		gameEnded = true;
		timerRunning = false;
		if (ghostBlocksCoroutine != null) StopCoroutine(ghostBlocksCoroutine);
	}

	void ShowResult(bool won)
	{
		gameOverPanel.SetActive(true);
		if (currentBall) currentBall.GetComponent<Rigidbody2D>().simulated = false;
		resultTitleText.text = won ? "VICTORY!" : "GAME OVER";
		resultStatsText.text = "TIME: " + Mathf.CeilToInt(timeRemaining) + "\nLIVES: " + currentLives;
	}

	public void SetMainBallActive(bool active)
	{
		if (currentBall == null) return;
		currentBall.SetActive(active);
		if (active) currentBall.GetComponent<BreakoutBall>().ResetBall();
	}

	public void RestartGame()
	{
		PlaySFX(restartSound);
		StopAllCoroutines();

		explosionTimerCoroutine = null;
		ghostBlocksCoroutine = null;
		gameEnded = false;
		GameStarted = false;
		timerRunning = false;
		explosionReady = false;
		timeRemaining = startTimerValue;
		currentLives = totalLives;

		gameOverPanel.SetActive(false);
		startPanel.SetActive(true);

		if (currentBall != null) DestroyImmediate(currentBall);
		GameObject[] powerUps = GameObject.FindGameObjectsWithTag("PowerUp");
		foreach (GameObject p in powerUps) Destroy(p);
		DoubleBall[] extraBalls = FindObjectsOfType<DoubleBall>();
		foreach (DoubleBall eb in extraBalls) Destroy(eb.gameObject);

		BreakoutBlock[] allBlocks = FindObjectsOfType<BreakoutBlock>(true);
		foreach (var b in allBlocks) b.ResetBlock();

		if (paddle != null) paddle.ResetPaddle();
		SetupBlocksAndPowerups();
		ResetBall();
		UpdateLivesUI(); UpdateTimerText();

		if (backgroundMusicSource != null && !backgroundMusicSource.isPlaying)
			backgroundMusicSource.Play();
	}

	// UI-Aktualisierungen
	void UpdateLivesUI() { if (livesText != null) livesText.text = "Lives: " + currentLives; }
	void UpdateTimerText() { if (timerText != null) timerText.text = "Time: " + Mathf.CeilToInt(timeRemaining); }

	public void ResetBall()
	{
		if (currentBall == null)
			currentBall = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);
		currentBall.GetComponent<BreakoutBall>().ResetBall();
	}

	public void PlaySFX(AudioClip clip)
	{
		if (sfxSource != null && clip != null)
		{
			sfxSource.PlayOneShot(clip);
		}
	}
}