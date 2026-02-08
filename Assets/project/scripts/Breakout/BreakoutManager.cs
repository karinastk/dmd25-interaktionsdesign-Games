using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BreakoutManager : MonoBehaviour
{
	public static BreakoutManager instance;

	[Header("Ball")]
	public GameObject ballPrefab;
	private GameObject currentBall;

	[Header("Paddle")]
	public Breakout.BreakoutPaddle paddle;

	[Header("Powerup Prefabs (Drops)")]
	public GameObject bigPaddleDropPrefab;
	public GameObject doubleBallDropPrefab;
	public GameObject explosionDropPrefab;

	private Dictionary<BreakoutBlock, GameObject> powerUpAssignment = new Dictionary<BreakoutBlock, GameObject>();
	private bool explosionReady = false;
	private Coroutine explosionTimerCoroutine;

	// NEU: Referenz für die Geister-Block-Routine
	private Coroutine ghostBlocksCoroutine;

	[Header("UI Panels")]
	public GameObject startPanel;
	public GameObject gameOverPanel;

	[Header("UI Text")]
	public TextMeshProUGUI livesText;
	public TextMeshProUGUI timerText;
	public TextMeshProUGUI resultTitleText;
	public TextMeshProUGUI resultStatsText;

	[Header("Game Settings")]
	public int lives = 3;
	private int currentLives;
	public float startTime = 60f;
	private float timeRemaining;
	private bool timerRunning = false;
	private bool gameEnded = false;
	private int remainingBlocks;

	public bool GameStarted { get; private set; } = false;

	void Awake()
	{
		if (instance == null) instance = this;
		else Destroy(gameObject);
	}

	void Start()
	{
		startPanel.SetActive(true);
		gameOverPanel.SetActive(false);
		currentLives = lives;
		UpdateLivesUI();
		timeRemaining = startTime;
		UpdateTimerText();
		SetupBlocksAndPowerups();
		ResetBall();
	}

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
			if (timeRemaining > 0) { timeRemaining -= Time.deltaTime; UpdateTimerText(); }
			else { timeRemaining = 0; GameOver(); }
		}

		if (explosionReady && Input.GetKeyDown(KeyCode.Space)) TriggerManualExplosion();
	}

	// ERGÄNZT: Startet die Geister-Block Routine beim Spielstart
	public void StartGame()
	{
		GameStarted = true;
		startPanel.SetActive(false);
		timerRunning = true;

		if (ghostBlocksCoroutine != null) StopCoroutine(ghostBlocksCoroutine);
		ghostBlocksCoroutine = StartCoroutine(GhostBlockRoutine());
	}

	// NEU: Die Routine für das Verschwinden der Blöcke
	IEnumerator GhostBlockRoutine()
	{
		while (!gameEnded)
		{
			yield return new WaitForSeconds(30f); // Alle 30 Sekunden warten

			// Finde alle Blöcke, die gerade aktiv im Spiel sind
			BreakoutBlock[] allBlocks = FindObjectsOfType<BreakoutBlock>();
			List<BreakoutBlock> availableBlocks = new List<BreakoutBlock>();

			foreach (var b in allBlocks)
			{
				if (b.gameObject.activeInHierarchy) availableBlocks.Add(b);
			}

			if (availableBlocks.Count > 0)
			{
				List<BreakoutBlock> chosenOnes = new List<BreakoutBlock>();
				int amount = Mathf.Min(6, availableBlocks.Count);

				// Wähle 6 zufällige Blöcke aus
				for (int i = 0; i < amount; i++)
				{
					int r = Random.Range(0, availableBlocks.Count);
					chosenOnes.Add(availableBlocks[r]);
					availableBlocks.RemoveAt(r);
				}

				// Blöcke unsichtbar & unantastbar machen
				foreach (var b in chosenOnes) b.SetGhostMode(true);

				yield return new WaitForSeconds(3f); // 3 Sekunden warten

				// Blöcke wieder normal machen
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
		else if (currentBall != null) currentBall.GetComponent<BreakoutBall>().ResetBallOnDeath();
	}

	void WinGame() { EndGameCleanUp(); ShowResult(true); }
	void GameOver() { EndGameCleanUp(); ShowResult(false); }

	// Hilfsfunktion zum sauberen Beenden
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
		resultTitleText.text = won ? "SIEG!" : "GAME OVER";
		resultStatsText.text = "ZEIT: " + Mathf.CeilToInt(timeRemaining) + "\nLEBEN: " + currentLives;
	}

	public void SetMainBallActive(bool active)
	{
		if (currentBall == null) return;
		currentBall.SetActive(active);
		if (active) currentBall.GetComponent<BreakoutBall>().ResetBall();
	}

	public void RestartGame()
	{
		StopAllCoroutines();
		explosionTimerCoroutine = null;
		ghostBlocksCoroutine = null; // Reset der Coroutine-Referenz

		gameEnded = false;
		GameStarted = false;
		timerRunning = false;
		explosionReady = false;
		timeRemaining = startTime;
		currentLives = lives;

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
	}

	void UpdateLivesUI() { if (livesText != null) livesText.text = "Leben: " + currentLives; }
	void UpdateTimerText() { if (timerText != null) timerText.text = "Zeit: " + Mathf.CeilToInt(timeRemaining); }

	public void ResetBall()
	{
		if (currentBall == null)
			currentBall = Instantiate(ballPrefab, Vector3.zero, Quaternion.identity);
		currentBall.GetComponent<BreakoutBall>().ResetBall();
	}
}