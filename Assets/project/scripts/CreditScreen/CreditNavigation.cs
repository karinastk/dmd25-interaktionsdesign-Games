using UnityEngine;

public class CreditNavigation : MonoBehaviour
{
	[Header("Menü-Referenzen")]
	public GameObject mainStartMenu;      // Das allererste Hauptmenü
	public GameObject creditSelection;   // Dein Credit-Screen mit den 4 Buttons

	[Header("Spiel-Credit-Texte/Panels")]
	public GameObject creditsGame1;      // Panel für Spiel 1
	public GameObject creditsGame2;      // Panel für Spiel 2
	public GameObject creditsGame3;      // Panel für Spiel 3

	// 1. Button: Credits Spiel 1
	public void OpenCreditsGame1()
	{
		HideAllCreditSubPanels();
		creditsGame1.SetActive(true);
	}

	// 2. Button: Credits Spiel 2
	public void OpenCreditsGame2()
	{
		HideAllCreditSubPanels();
		creditsGame2.SetActive(true);
	}

	// 3. Button: Credits Spiel 3
	public void OpenCreditsGame3()
	{
		HideAllCreditSubPanels();
		creditsGame3.SetActive(true);
	}

	// 4. Button: ZURÜCK ZUM MAIN MENU
	public void BackToMainMenu()
	{
		creditSelection.SetActive(false);
		HideAllCreditSubPanels();
		mainStartMenu.SetActive(true);
	}

	// Hilfsfunktion, um die einzelnen Texte zu verstecken
	private void HideAllCreditSubPanels()
	{
		if (creditsGame1 != null) creditsGame1.SetActive(false);
		if (creditsGame2 != null) creditsGame2.SetActive(false);
		if (creditsGame3 != null) creditsGame3.SetActive(false);
	}
}