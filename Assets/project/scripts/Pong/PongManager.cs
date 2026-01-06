
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PongManager : MonoBehaviour
{
    public enum Player
    {
        Player1,
        Player2
    }

    public static PongManager instance;
    public GameObject ballPrefab;
    public float ballStartVelocity = 5;
    public int maxPoints = 10;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI winnerText;
    public GameObject gameOverObject;

    private uint player1Score;
    private uint player2Score;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Debug.LogWarning(
                "PongManager already exists. " +
                "Destroying " + name
                );

            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameOverObject.SetActive(false);

        ResetBall();
        UpdateScore();
    }

    public void OnGoalScored(Player player)
    {
        if (player == Player.Player1)
            player1Score++;
        else
            player2Score++;

        UpdateScore();

        if (player1Score == maxPoints)
            GameOver(Player.Player1);
        else if (player2Score == maxPoints)
            GameOver(Player.Player2);
        else
            ResetBall();
    }

    public void ResetBall()
    {
        Instantiate(ballPrefab);

        //Instantiate(ballPrefab, gameObject.transform);
        //Instantiate(ballPrefab, new Vector3(1,1,1), Quaternion.identity);
    }

    void UpdateScore()
    {
        scoreText.text = player1Score + " : " + player2Score; // 0 : 0
    }

    void GameOver(Player winner)
    {
        gameOverObject.SetActive(true);
        winnerText.text = winner.ToString();
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}