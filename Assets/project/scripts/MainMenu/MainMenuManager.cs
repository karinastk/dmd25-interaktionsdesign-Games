using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
	// Lädt eine Szene anhand des Namens
	public void LoadSceneByName(string sceneName)
	{
		// Überprüfen, ob die Szene in Build Settings ist (optional)
		if (Application.CanStreamedLevelBeLoaded(sceneName))
		{
			SceneManager.LoadScene(sceneName);
		}
		else
		{
			Debug.LogError("Szene '" + sceneName + "' wurde nicht gefunden oder ist nicht in den Build Settings!");
		}
	}

	// Beendet das Spiel
	public void QuitGame()
	{
		Application.Quit();
		Debug.Log("Spiel beendet"); // Wird im Editor angezeigt
	}
}
