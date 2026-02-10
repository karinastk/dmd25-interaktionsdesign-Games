using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PongManager : MonoBehaviour
{
	public enum Player { Player1, Player2 }
	public static PongManager instance;

	[Header("Game Settings")]
	public GameObject ballPrefab;
	public float ballStartVelocity = 10f;
	public float ballFastVelocity = 18f;
	[HideInInspector] public float currentBallVelocity;

	[Header("Timer Settings")]
	public float gameTimeInSeconds = 180f;
	public TextMeshProUGUI timerText;

	[Header("Catch Up Settings")]
	[Tooltip("Ab welcher Punktedifferenz soll das Paddle vergrößert werden?")]
	public int scoreDifferenceThreshold = 5;

	[Header("Special Events")]
	public bool controlsInverted = false;
	public TextMeshProUGUI eventText;

	[Header("UI References")]
	public TextMeshProUGUI player1ScoreText;
	public TextMeshProUGUI player2ScoreText;
	public TextMeshProUGUI winnerText;
	public GameObject gameOverObject;
	public GameObject startMenuObject;

	private bool isGameRunning = false;
	private bool isTimerRunning = false;
	private bool isShuttingDown = false;
	private bool isFirstStart = true;

	private int player1Score;
	private int player2Score;
	private float timeRemaining;

	private void Awake()
	{
		if (instance == null) instance = this;
		else Destroy(gameObject);
	}

	void Start()
	{
		isShuttingDown = false;
		gameOverObject.SetActive(false);
		startMenuObject.SetActive(true);

		if (eventText != null)
		{
			eventText.gameObject.SetActive(false);
			Color c = eventText.color;
			c.a = 0;
			eventText.color = c;
		}

		timeRemaining = gameTimeInSeconds;
		currentBallVelocity = ballStartVelocity;
		player1Score = 0;
		player2Score = 0;

		UpdateScoreUI();
		UpdateTimerUI();
	}

	void Update()
	{
		if (isGameRunning && isFirstStart)
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				isFirstStart = false;
				isTimerRunning = true;
				LaunchCurrentBall();
			}
		}

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

	public void StartGame()
	{
		startMenuObject.SetActive(false);
		isGameRunning = true;
		isFirstStart = true;
		isTimerRunning = false;
		timeRemaining = gameTimeInSeconds;
		controlsInverted = false;
		currentBallVelocity = ballStartVelocity;

		StopAllCoroutines();
		StartCoroutine(InvertControlRoutine());
		StartCoroutine(SpeedEventRoutine());

		ResetBall(false);
	}

	IEnumerator InvertControlRoutine()
	{
		yield return new WaitForSeconds(30f);
		if (isGameRunning)
		{
			controlsInverted = true;
			yield return StartCoroutine(FadeMessage("CONTROLS SWAPPED!", Color.yellow));
			yield return new WaitForSeconds(15f);
			controlsInverted = false;
			yield return StartCoroutine(FadeMessage("CONTROLS NORMAL", Color.green));
		}
	}

	IEnumerator SpeedEventRoutine()
	{
		yield return new WaitForSeconds(90f);
		if (isGameRunning)
		{
			currentBallVelocity = ballFastVelocity;
			yield return StartCoroutine(FadeMessage("FAST BALL!", Color.red));
			yield return new WaitForSeconds(15f);
			currentBallVelocity = ballStartVelocity;
			yield return StartCoroutine(FadeMessage("SPEED NORMAL", Color.cyan));
		}
	}

	public IEnumerator FadeMessage(string message, Color textColor)
	{
		if (eventText != null)
		{
			eventText.text = message;
			eventText.gameObject.SetActive(true);
			float duration = 0.8f;
			float elapsed = 0f;

			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				textColor.a = Mathf.Lerp(0, 1f, elapsed / duration);
				eventText.color = textColor;
				yield return null;
			}
			yield return new WaitForSeconds(1.5f);
			elapsed = 0f;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				textColor.a = Mathf.Lerp(1f, 0, elapsed / duration);
				eventText.color = textColor;
				yield return null;
			}
			eventText.gameObject.SetActive(false);
		}
	}

	public void OnGoalScored(Player player)
	{
		if (!isGameRunning || isShuttingDown) return;

		if (player == Player.Player1) player1Score++;
		else player2Score++;

		UpdateScoreUI();
		CheckForPaddlePowerup();

		RemoveAllBalls();
		if (isGameRunning) ResetBall(true);
	}

	private void CheckForPaddlePowerup()
	{
		Pong.PongPaddle[] paddles = Object.FindObjectsByType<Pong.PongPaddle>(FindObjectsSortMode.None);
		foreach (var paddle in paddles)
		{
			// Nutzt nun die manuell einstellbare Variable scoreDifferenceThreshold
			if (player2Score >= player1Score + scoreDifferenceThreshold && paddle.isPlayer1)
			{
				paddle.StartCatchUpEvent(8f);
			}
			else if (player1Score >= player2Score + scoreDifferenceThreshold && !paddle.isPlayer1)
			{
				paddle.StartCatchUpEvent(8f);
			}
		}
	}

	private void LaunchCurrentBall()
	{
		PongBall ball = Object.FindFirstObjectByType<PongBall>();
		if (ball != null) ball.LaunchBall();
	}

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

	void UpdateScoreUI() { player1ScoreText.text = player1Score.ToString(); player2ScoreText.text = player2Score.ToString(); }
	void UpdateTimerUI() { int minutes = Mathf.FloorToInt(timeRemaining / 60); int seconds = Mathf.FloorToInt(timeRemaining % 60); timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds); }
	void EndMatch() { isTimerRunning = false; isGameRunning = false; DetermineWinner(); }
	void DetermineWinner() { gameOverObject.SetActive(true); RemoveAllBalls(); string winnerStr = (player1Score > player2Score) ? "PLAYER 1 WON!" : (player2Score > player1Score) ? "PLAYER 2 WON!" : "IT'S A DRAW!"; winnerText.text = winnerStr + "\n" + player1Score + " : " + player2Score; }
	private void RemoveAllBalls() { GameObject[] balls = GameObject.FindGameObjectsWithTag("Ball"); foreach (GameObject b in balls) Destroy(b); }
	public void RestartGame() { isShuttingDown = true; SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); }
	private void OnApplicationQuit() { isShuttingDown = true; }
}