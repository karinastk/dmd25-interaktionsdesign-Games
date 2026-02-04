using TMPro;
using UnityEngine;

public class BreakoutManager : MonoBehaviour
{
	public static BreakoutManager instance;

	[Header("Ball")]
	public GameObject ballPrefab;
	public Transform ballStart;
	public float ballStartVelocity = 5f;
	private GameObject currentBall;

	[Header("Paddle")]
	public Breakout.BreakoutPaddle paddle;

	private Vector3 paddleStartPos;

	[Header("UI Panels")]
	public GameObject startPanel;
	public GameObject gameOverObject;

	[Header("Result UI")]
	public TextMeshProUGUI resultTitleText;
	public TextMeshProUGUI resultStatsText;

	[Header("Lives")]
	public int lives = 10;
	private int currentLives;

	[Header("Timer")]
	public TextMeshProUGUI scoreText;
	public TextMeshProUGUI timerText;
	public float startTime = 60f;
	private float timeRemaining;
	private bool timerRunning = false;

	private int remainingBlocks;
	private bool gameEnded = false;

	public bool GameStarted { get; private set; } = false;

	void Awake()
	{
		if (instance == null)
			instance = this;
		else
			Destroy(gameObject);
	}

	void Start()
	{
		startPanel.SetActive(true);
		gameOverObject.SetActive(false);

		timeRemaining = startTime;
		UpdateTimerText();

		currentLives = lives;
		UpdateLivesUI();

		paddleStartPos = paddle.transform.position;

		remainingBlocks = GameObject.FindGameObjectsWithTag("Block").Length;
		gameEnded = false;

		ResetBall();
	}

	void Update()
	{
		if (!timerRunning || gameEnded)
			return;

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

	public void StartGame()
	{
		GameStarted = true;
		startPanel.SetActive(false);
		ResetBall();
	}

	public void StartTimer()
	{
		if (!GameStarted || gameEnded)
			return;

		timerRunning = true;
	}

	public void OnDeath()
	{
		if (!GameStarted || gameEnded)
			return;

		currentLives--;
		UpdateLivesUI();

		if (currentLives <= 0)
		{
			GameOver();
		}
		else
		{
			currentBall.GetComponent<BreakoutBall>().ResetBallOnDeath();
		}
	}

	public void OnBlockDestroyed()
	{
		if (gameEnded)
			return;

		remainingBlocks--;

		if (remainingBlocks <= 0)
		{
			WinGame();
		}
	}

	void WinGame()
	{
		gameEnded = true;
		timerRunning = false;
		GameStarted = false;

		ShowResult(true);
	}

	public void GameOver()
	{
		if (gameEnded)
			return;

		gameEnded = true;
		timerRunning = false;
		GameStarted = false;

		ShowResult(false);
	}

	void ShowResult(bool won)
	{
		gameOverObject.SetActive(true);

		if (currentBall != null)
		{
			Rigidbody2D rb = currentBall.GetComponent<Rigidbody2D>();
			rb.linearVelocity = Vector2.zero;
			rb.simulated = false;
		}

		resultTitleText.text = won ? "YOU WON!" : "YOU LOST";

		int time = Mathf.CeilToInt(timeRemaining);
		resultStatsText.text =
			"Time: " + time + "\n" +
			"Lives: " + currentLives;
	}

	public void RestartGame()
	{
		gameOverObject.SetActive(false);
		startPanel.SetActive(true);

		// Timer & Leben zurücksetzen
		timeRemaining = startTime;
		UpdateTimerText();
		currentLives = lives;
		UpdateLivesUI();

		gameEnded = false;
		GameStarted = false;

		// Paddle zurücksetzen
		paddle.ResetPaddle();

		// Ball zurücksetzen
		ResetBall();

		// Blöcke zurücksetzen
		BreakoutBlock[] blocks = FindObjectsOfType<BreakoutBlock>();
		remainingBlocks = blocks.Length;
		foreach (BreakoutBlock block in blocks)
		{
			block.ResetBlock();
		}
	}

	void UpdateLivesUI()
	{
		scoreText.text = "LIVES: " + currentLives;
	}

	void UpdateTimerText()
	{
		int seconds = Mathf.CeilToInt(timeRemaining);
		timerText.text = "TIME: " + seconds;
	}

	public void ResetBall()
	{
		if (currentBall == null)
			currentBall = Instantiate(ballPrefab, ballStart.position, Quaternion.identity);

		currentBall.GetComponent<BreakoutBall>().ResetBall();
	}
}
