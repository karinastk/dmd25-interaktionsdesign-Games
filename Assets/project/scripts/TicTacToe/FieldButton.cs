using TMPro;
using UnityEngine;

public class FieldButton : MonoBehaviour
{
	private TicTacToeManager manager; // Referenz zum Spielmanager
	public int index;                // Index des Feldes im Spielfeldarray

	public int Player { get; private set; } = -1; // -1 = leer, 0 = Player1, 1 = Player2
	public bool IsBlocked { get; private set; } = false; // Ob das Feld blockiert ist

	TextMeshProUGUI buttonText; // Textanzeige auf dem Button

	void Start()
	{
		// Spielmanager finden und Textkomponente des Buttons holen
		manager = FindObjectOfType<TicTacToeManager>();
		buttonText = GetComponentInChildren<TextMeshProUGUI>();
	}

	// Wird aufgerufen, wenn der Button im Spiel angeklickt wird
	public void OnButtonClicked()
	{
		manager.OnButtonClickedInManager(this); // Übergibt das Feld an den Manager
	}

	// Normales Setzen eines Feldes
	public void SetField(int player)
	{
		if (IsBlocked) return;          // Blockierte Felder können nicht gesetzt werden
		Player = player;                // Spieler setzen
		SetText(player == 0 ? "X" : "O"); // Text anzeigen
	}

	// Erzwingt das Setzen eines Feldes (OverwriteEnemy Feature)
	public void ForceSetField(int player)
	{
		Player = player;  // Spieler setzen
		IsBlocked = false; // Blockierung aufheben, falls gesetzt
		SetText(player == 0 ? "X" : "O");
	}

	// Feld blockieren (BlockField Feature)
	public void SetBlocked()
	{
		IsBlocked = true;
		SetText("BLOCKED"); // Anzeigen, dass das Feld blockiert ist
	}

	// Blockierung aufheben (nach Gegnerzug oder Auflösung)
	public void ReleaseBlock()
	{
		IsBlocked = false;
		SetText(""); // Text leeren
	}

	// Feld komplett zurücksetzen (für Bomb oder Neustart)
	public void ResetField()
	{
		Player = -1;
		IsBlocked = false;
		SetText("");
	}

	// Interne Methode, um Text auf dem Button zu ändern
	void SetText(string t)
	{
		if (buttonText != null)
			buttonText.text = t;
	}
}
