using TMPro;
using UnityEngine;

public class FieldButton : MonoBehaviour
{
    private TicTacToManager ticTacToManager;
    internal int index; // Der Index des Feldes im Spielfeld-Array

    // Wir fügen eine Player-Eigenschaft hinzu, die den aktuellen Spieler für dieses Feld speichert
    public int Player { get; private set; } = -1; // -1 bedeutet, dass das Feld leer ist

    // Sperrstatus für das Feld
    public bool IsBlocked { get; private set; } = false; // Wenn das Feld gesperrt ist, ist IsBlocked true

    private TextMeshProUGUI buttonText; // Text-Komponente für den Button-Text

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ticTacToManager = FindObjectOfType<TicTacToManager>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>(); // Holen der Text-Komponente des Buttons
    }

    // Wird aufgerufen, wenn der Button geklickt wird
    public void OnButtonClicked()
    {
        // Wenn das Feld bereits belegt oder gesperrt ist, nichts tun
        if (Player != -1 || IsBlocked)
            return;

        // Spielzug an den Manager weitergeben
        ticTacToManager.OnButtonClickedInManager(this);
    }

    // Setzt das Symbol für den aktuellen Spieler (X oder O)
    public void SetField(int currentPlayer)
    {
        // Wenn das Feld gesperrt ist, nichts setzen
        if (IsBlocked)
            return;

        // Das Symbol für den aktuellen Spieler setzen
        Player = currentPlayer;

        // Je nach Spieler X oder O anzeigen
        SetButtonText(currentPlayer == 0 ? "X" : "O");
    }

    // Setzt den Text des Buttons
    public void SetButtonText(string text)
    {
        if (buttonText != null)
        {
            buttonText.text = text; // Setzt den Text auf den Button
        }
    }

    // Setzt den Text für gesperrte Felder auf "BLOCKED"
    public void SetBlocked()
    {
        IsBlocked = true; // Das Feld wird als gesperrt markiert
        SetButtonText("BLOCKED"); // Zeigt "BLOCKED" im Text an
    }

    // Hebt die Sperrung des Feldes auf
    public void ReleaseBlock()
    {
        IsBlocked = false; // Das Feld wird als nicht gesperrt markiert
        SetButtonText(""); // Setzt den Text zurück (wird später mit X oder O überschrieben)
    }

    // Optional: Eine Methode zum Zurücksetzen des Feldes (falls nötig)
    public void ResetField()
    {
        Player = -1; // Feld wieder leer machen
        IsBlocked = false; // Sperrung aufheben
        SetButtonText(""); // Text zurücksetzen
    }
}
