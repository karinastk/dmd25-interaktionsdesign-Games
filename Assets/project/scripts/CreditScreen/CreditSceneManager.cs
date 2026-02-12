using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditSceneManager : MonoBehaviour
{
	[Header("Menü-Bereiche")]
	// Das Objekt, das deine 4 Haupt-Buttons (Spiel 1, 2, 3, Main Menu) enthält
	public GameObject selectionMenu;

	[Header("Die Credit-Panels")]
	public GameObject creditsPanelGame1;
	public GameObject creditsPanelGame2;
	public GameObject creditsPanelGame3;

	[Header("Szenen-Einstellungen")]
	public string mainMenuSceneName = "MainMenu";

	[Header("Audio-Einstellungen")]
	public AudioSource audioSource; // Die AudioSource am Manager-Objekt
	public AudioClip clickSound;    // Dein Klick-Sound Asset

	void Start()
	{
		// Sicherstellen, dass beim Start nur die Auswahl-Buttons da sind
		ReturnToSelection();

		// Falls die AudioSource nicht zugewiesen wurde, versuchen wir sie zu finden
		if (audioSource == null)
			audioSource = GetComponent<AudioSource>();
	}

	// --- Sound Funktion ---
	public void PlayClickSound()
	{
		if (audioSource != null && clickSound != null)
		{
			audioSource.PlayOneShot(clickSound);
		}
	}

	// --- Funktionen für die Haupt-Buttons ---

	public void ShowCreditsGame1()
	{
		PlayClickSound();
		HideAll();
		selectionMenu.SetActive(false);
		creditsPanelGame1.SetActive(true);
	}

	public void ShowCreditsGame2()
	{
		PlayClickSound();
		HideAll();
		selectionMenu.SetActive(false);
		creditsPanelGame2.SetActive(true);
	}

	public void ShowCreditsGame3()
	{
		PlayClickSound();
		HideAll();
		selectionMenu.SetActive(false);
		creditsPanelGame3.SetActive(true);
	}

	public void BackToMainMenu()
	{
		PlayClickSound();
		SceneManager.LoadScene(mainMenuSceneName);
	}

	// --- Zurück zur Auswahl (für die 3 Buttons auf den Panels) ---

	public void ReturnToSelection()
	{
		PlayClickSound();
		HideAll();
		selectionMenu.SetActive(true);
	}

	private void HideAll()
	{
		if (creditsPanelGame1 != null) creditsPanelGame1.SetActive(false);
		if (creditsPanelGame2 != null) creditsPanelGame2.SetActive(false);
		if (creditsPanelGame3 != null) creditsPanelGame3.SetActive(false);
	}
}