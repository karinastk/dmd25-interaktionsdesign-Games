using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
	public void LoadTicTacToe()
	{
		SceneManager.LoadScene("TicTacToe");
	}

	public void LoadBreakout()
	{
		SceneManager.LoadScene("Breakout");
	}

	public void LoadPong()
	{
		SceneManager.LoadScene("Pong");
	}

	public void LoadMainMenu()
	{
		SceneManager.LoadScene("MainMenu");
	}

	public void QuitGame()
	{
		Application.Quit();
	}
}
