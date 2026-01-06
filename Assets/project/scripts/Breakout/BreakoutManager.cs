using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BreakoutManager : MonoBehaviour
{  
    public static BreakoutManager instance;
    public GameObject ballPrefab;
    public Transform ballStart;

    public float ballStartVelocity = 5;
    public int lives = 10;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI loserText;
    public GameObject gameOverObject;

    private uint deaths;
    

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
        {
            Debug.LogWarning(
                "BreakoutManager already exists. " +
                "Destroying " + name
                );

            Destroy(gameObject);
        }
    }

    void Start()
    {
        gameOverObject.SetActive(false);

        ResetBall();
        UpdateDeathCount();
    }

    public void OnDeath()
    {
        deaths++;

        UpdateDeathCount();

        if (deaths == lives)
            GameOver();
   
        else
            ResetBall();
    }

    public void ResetBall()
    {
        //Instantiate(ballPrefab);

        //Instantiate(ballPrefab, gameObject.transform);
        Instantiate(
            ballPrefab, 
            ballStart.position, 
            Quaternion.identity
            );
    }

    void UpdateDeathCount()
    {
        scoreText.text =  "Leben: " + (lives - deaths); // tode: 1 Oder Leben: 9
    }

    void GameOver()
    {
        gameOverObject.SetActive(true);
        loserText.text = "Du bist ein Loser. :(";
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}